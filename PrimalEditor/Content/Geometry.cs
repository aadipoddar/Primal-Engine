using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using PrimalEditor.Common;
using PrimalEditor.DllWrapper;
using PrimalEditor.GameProject;
using PrimalEditor.Utilities;

namespace PrimalEditor.Content
{
	enum PrimitiveMeshType
	{
		Plane,
		Cube,
		UvSphere,
		IcoSphere,
		Cylinder,
		Capsule
	}

	[Flags]
	enum ElementsType
	{
		Position = 0x00,
		Normals = 0x01,
		TSpace = 0x03,
		Joints = 0x04,
		Colors = 0x08
	}

	class Mesh : ViewModelBase
	{
		public static int PositionSize = sizeof(float) * 3;

		private int _elementSize;
		public int ElementSize
		{
			get => _elementSize;
			set
			{
				if (_elementSize != value)
				{
					_elementSize = value;
					OnPropertyChanged(nameof(ElementSize));
				}
			}
		}

		private int _vertexCount;
		public int VertexCount
		{
			get => _vertexCount;
			set
			{
				if (_vertexCount != value)
				{
					_vertexCount = value;
					OnPropertyChanged(nameof(VertexCount));
				}
			}
		}

		private int _indexSize;
		public int IndexSize
		{
			get => _indexSize;
			set
			{
				if (_indexSize != value)
				{
					_indexSize = value;
					OnPropertyChanged(nameof(IndexSize));
				}
			}
		}

		private int _indexCount;
		public int IndexCount
		{
			get => _indexCount;
			set
			{
				if (_indexCount != value)
				{
					_indexCount = value;
					OnPropertyChanged(nameof(IndexCount));
				}
			}
		}

		private string _name;
		public string Name
		{
			get => _name;
			set
			{
				if (_name != value)
				{
					_name = value;
					OnPropertyChanged(nameof(Name));
				}
			}
		}

		public ElementsType ElementsType { get; set; }

		public byte[] Positions { get; set; }
		public byte[] Elements { get; set; }
		public byte[] Indices { get; set; }
	}

	class MeshLOD : ViewModelBase
	{
		private string _name;
		public string Name
		{
			get => _name;
			set
			{
				if (_name != value)
				{
					_name = value;
					OnPropertyChanged(nameof(Name));
				}
			}
		}

		private float _lodThreshold;
		public float LodThreshold
		{
			get => _lodThreshold;
			set
			{
				if (_lodThreshold != value)
				{
					_lodThreshold = value;
					OnPropertyChanged(nameof(LodThreshold));
				}
			}
		}

		public ObservableCollection<Mesh> Meshes { get; } = new ObservableCollection<Mesh>();
	}

	class LODGroup : ViewModelBase
	{
		private string _name;
		public string Name
		{
			get => _name;
			set
			{
				if (_name != value)
				{
					_name = value;
					OnPropertyChanged(nameof(Name));
				}
			}
		}

		public ObservableCollection<MeshLOD> LODs { get; } = new ObservableCollection<MeshLOD>();
	}

	class GeometryImportSettings : ViewModelBase
	{
		private bool _calculateNormals;
		public bool CalculateNormals
		{
			get => _calculateNormals;
			set
			{
				if (_calculateNormals != value)
				{
					_calculateNormals = value;
					OnPropertyChanged(nameof(CalculateNormals));
				}
			}
		}

		private bool _calculateTangents;
		public bool CalculateTangents
		{
			get => _calculateTangents;
			set
			{
				if (_calculateTangents != value)
				{
					_calculateTangents = value;
					OnPropertyChanged(nameof(CalculateTangents));
				}
			}
		}

		private float _smoothingAngle;
		public float SmoothingAngle
		{
			get => _smoothingAngle;
			set
			{
				if (_smoothingAngle != value)
				{
					_smoothingAngle = value;
					OnPropertyChanged(nameof(SmoothingAngle));
				}
			}
		}

		private bool _reverseHandedness;
		public bool ReverseHandedness
		{
			get => _reverseHandedness;
			set
			{
				if (_reverseHandedness != value)
				{
					_reverseHandedness = value;
					OnPropertyChanged(nameof(ReverseHandedness));
				}
			}
		}

		private bool _importEmbededTextures;
		public bool ImportEmbededTextures
		{
			get => _importEmbededTextures;
			set
			{
				if (_importEmbededTextures != value)
				{
					_importEmbededTextures = value;
					OnPropertyChanged(nameof(ImportEmbededTextures));
				}
			}
		}

		private bool _importAnimations;
		public bool ImportAnimations
		{
			get => _importAnimations;
			set
			{
				if (_importAnimations != value)
				{
					_importAnimations = value;
					OnPropertyChanged(nameof(ImportAnimations));
				}
			}
		}

		public float SmootingAngle { get; internal set; }

		public GeometryImportSettings()
		{
			CalculateNormals = false;
			CalculateTangents = false;
			SmoothingAngle = 178f;
			ReverseHandedness = false;
			ImportEmbededTextures = true;
			ImportAnimations = true;
		}

		public void ToBinary(BinaryWriter writer)
		{
			writer.Write(CalculateNormals);
			writer.Write(CalculateTangents);
			writer.Write(SmoothingAngle);
			writer.Write(ReverseHandedness);
			writer.Write(ImportEmbededTextures);
			writer.Write(ImportAnimations);
		}

		public void FromBinary(BinaryReader reader)
		{
			CalculateNormals = reader.ReadBoolean();
			CalculateTangents = reader.ReadBoolean();
			SmoothingAngle = reader.ReadSingle();
			ReverseHandedness = reader.ReadBoolean();
			ImportEmbededTextures = reader.ReadBoolean();
			ImportAnimations = reader.ReadBoolean();
		}
	}

	class Geometry : Asset
	{
		private readonly List<LODGroup> _lodGroups = new();
		private readonly object _lock = new();

		public GeometryImportSettings ImportSettings { get; } = new GeometryImportSettings();

		public LODGroup GetLODGroup(int lodGroup = 0)
		{
			Debug.Assert(lodGroup >= 0 && lodGroup < _lodGroups.Count);
			return (lodGroup < _lodGroups.Count) ? _lodGroups[lodGroup] : null;
		}

		public void FromRawData(byte[] data)
		{
			Debug.Assert(data?.Length > 0);
			_lodGroups.Clear();
			using BinaryReader reader = new(new MemoryStream(data));

			// skip scene name string
			int s = reader.ReadInt32();
			reader.BaseStream.Position += s;

			// get number of LODs
			int numLODGroups = reader.ReadInt32();
			Debug.Assert(numLODGroups > 0);

			for (int i = 0; i < numLODGroups; ++i)
			{
				// get LOD group's name
				s = reader.ReadInt32();
				string lodGroupName;
				if (s > 0)
				{
					byte[] nameBytes = reader.ReadBytes(s);
					lodGroupName = Encoding.UTF8.GetString(nameBytes);
				}
				else
				{
					lodGroupName = $"lod_{ContentHelper.GetRandomString()}";
				}

				// get number of meshes in this LOD group
				int numMeshes = reader.ReadInt32();
				Debug.Assert(numMeshes > 0);
				List<MeshLOD> lods = ReadMeshLODs(numMeshes, reader);

				LODGroup lodGroup = new() { Name = lodGroupName };
				lods.ForEach(l => lodGroup.LODs.Add(l));

				_lodGroups.Add(lodGroup);

			}
		}

		private static List<MeshLOD> ReadMeshLODs(int numMeshes, BinaryReader reader)
		{
			List<int> lodIds = new();
			List<MeshLOD> lodList = new();
			for (int i = 0; i < numMeshes; ++i)
			{
				ReadMeshes(reader, lodIds, lodList);
			}

			return lodList;
		}

		private static void ReadMeshes(BinaryReader reader, List<int> lodIds, List<MeshLOD> lodList)
		{
			// get mesh's name
			int s = reader.ReadInt32();
			string meshName;

			if (s > 0)
			{
				byte[] nameBytes = reader.ReadBytes(s);
				meshName = Encoding.UTF8.GetString(nameBytes);
			}
			else
			{
				meshName = $"mesh_{ContentHelper.GetRandomString()}";
			}

			Mesh mesh = new() { Name = meshName };

			int lodId = reader.ReadInt32();
			mesh.ElementSize = reader.ReadInt32();
			mesh.ElementsType = (ElementsType)reader.ReadInt32();
			mesh.VertexCount = reader.ReadInt32();
			mesh.IndexSize = reader.ReadInt32();
			mesh.IndexCount = reader.ReadInt32();
			float lodThreshold = reader.ReadSingle();

			int elementBufferSize = mesh.ElementSize * mesh.VertexCount;
			int indexBufferSize = mesh.IndexSize * mesh.IndexCount;

			mesh.Positions = reader.ReadBytes(Mesh.PositionSize * mesh.VertexCount);
			mesh.Elements = reader.ReadBytes(elementBufferSize);
			mesh.Indices = reader.ReadBytes(indexBufferSize);

			MeshLOD lod;
			if (ID.IsValid(lodId) && lodIds.Contains(lodId))
			{
				lod = lodList[lodIds.IndexOf(lodId)];
				Debug.Assert(lod != null);
			}
			else
			{
				lodIds.Add(lodId);
				lod = new MeshLOD() { Name = meshName, LodThreshold = lodThreshold };
				lodList.Add(lod);
			}

			lod.Meshes.Add(mesh);
		}

		public override void Import(string file)
		{
			Debug.Assert(File.Exists(file));
			Debug.Assert(!string.IsNullOrEmpty(FullPath));
			string extension = Path.GetExtension(file).ToLower();

			SourcePath = file;

			try
			{
				if (extension == ".fbx")
				{
					ImportFbx(file);
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message);
				string msg = $"Failed to read {file} for import";
				Debug.WriteLine(msg);
				Logger.Log(MessageType.Error, msg);
			}
		}

		private void ImportFbx(string file)
		{
			Logger.Log(MessageType.Info, $"Importing FBX file {file}");
			string tempPath = Application.Current.Dispatcher.Invoke(() => Project.Current.TempFolder);
			if (string.IsNullOrEmpty(tempPath)) return;

			lock (_lock)
			{
				if (!Directory.Exists(tempPath)) Directory.CreateDirectory(tempPath);
			}

			string tempFile = $"{tempPath}{ContentHelper.GetRandomString()}.fbx";
			File.Copy(file, tempFile, true);
			ContentToolsAPI.ImportFbx(tempFile, this);
		}

		public override void Load(string file)
		{
			Debug.Assert(File.Exists(file));
			Debug.Assert(Path.GetExtension(file).ToLower() == AssetFileExtension);

			try
			{
				byte[] data = null;
				using (BinaryReader reader = new(File.Open(file, FileMode.Open, FileAccess.Read)))
				{
					ReadAssetFileHeader(reader);
					ImportSettings.FromBinary(reader);
					int dataLength = reader.ReadInt32();
					Debug.Assert(dataLength > 0);
					data = reader.ReadBytes(dataLength);
				}

				Debug.Assert(data.Length > 0);

				using (BinaryReader reader = new(new MemoryStream(data)))
				{
					LODGroup lodGroup = new();
					lodGroup.Name = reader.ReadString();
					int lodCount = reader.ReadInt32();

					for (int i = 0; i < lodCount; ++i)
					{
						lodGroup.LODs.Add(BinaryToLOD(reader));
					}

					_lodGroups.Clear();
					_lodGroups.Add(lodGroup);
				}

			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message);
				Logger.Log(MessageType.Error, $"Failed to load geometry asset file from file: {file}");
			}
		}

		public override IEnumerable<string> Save(string file)
		{
			Debug.Assert(_lodGroups.Any());
			List<string> savedFiles = new();
			if (!_lodGroups.Any()) return savedFiles;

			string path = Path.GetDirectoryName(file) + Path.DirectorySeparatorChar;
			string fileName = Path.GetFileNameWithoutExtension(file);

			try
			{
				foreach (LODGroup lodGroup in _lodGroups)
				{
					Debug.Assert(lodGroup.LODs.Any());
					// use the name of most detailed LOD for file name
					string meshFileName = ContentHelper.SanitizeFileName(
						path + fileName + ((_lodGroups.Count > 1) ? "_" + ((lodGroup.LODs.Count > 1) ? lodGroup.Name : lodGroup.LODs[0].Name) : string.Empty)) + AssetFileExtension;
					// NOTE: we have to make a different id for each new asset file, but if a geometry asset file
					//       with the same name already exists then use its guid instead
					Guid = TryGetAssetInfo(meshFileName) is AssetInfo info && info.Type == Type ? info.Guid : Guid.NewGuid();
					byte[] data = null;
					using (BinaryWriter writer = new(new MemoryStream()))
					{
						writer.Write(lodGroup.Name);
						writer.Write(lodGroup.LODs.Count);
						List<byte> hashes = new();
						foreach (MeshLOD lod in lodGroup.LODs)
						{
							LODToBinary(lod, writer, out byte[] hash);
							hashes.AddRange(hash);
						}

						Hash = ContentHelper.ComputeHash(hashes.ToArray());
						data = (writer.BaseStream as MemoryStream).ToArray();
						Icon = GenerateIcon(lodGroup.LODs[0]);
					}

					Debug.Assert(data?.Length > 0);

					using (BinaryWriter writer = new(File.Open(meshFileName, FileMode.Create, FileAccess.Write)))
					{
						WriteAssetFileHeader(writer);
						ImportSettings.ToBinary(writer);
						writer.Write(data.Length);
						writer.Write(data);
					}

					Logger.Log(MessageType.Info, $"Saved geometry to {meshFileName}");
					savedFiles.Add(meshFileName);
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message);
				Logger.Log(MessageType.Error, $"Failed to save gemoetry to {file}");
			}

			return savedFiles;
		}

		private void LODToBinary(MeshLOD lod, BinaryWriter writer, out byte[] hash)
		{
			writer.Write(lod.Name);
			writer.Write(lod.LodThreshold);
			writer.Write(lod.Meshes.Count);

			long meshDataBegin = writer.BaseStream.Position;

			foreach (Mesh mesh in lod.Meshes)
			{
				writer.Write(mesh.Name);
				writer.Write(mesh.ElementSize);
				writer.Write((int)mesh.ElementsType);
				writer.Write(mesh.VertexCount);
				writer.Write(mesh.IndexSize);
				writer.Write(mesh.IndexCount);
				writer.Write(mesh.Positions);
				writer.Write(mesh.Elements);
				writer.Write(mesh.Indices);
			}

			long meshDataSize = writer.BaseStream.Position - meshDataBegin;
			Debug.Assert(meshDataSize > 0);
			byte[] buffer = (writer.BaseStream as MemoryStream).ToArray();
			hash = ContentHelper.ComputeHash(buffer, (int)meshDataBegin, (int)meshDataSize);
		}

		private MeshLOD BinaryToLOD(BinaryReader reader)
		{
			MeshLOD lod = new();
			lod.Name = reader.ReadString();
			lod.LodThreshold = reader.ReadSingle();
			int meshCount = reader.ReadInt32();

			for (int i = 0; i < meshCount; ++i)
			{
				Mesh mesh = new()
				{
					Name = reader.ReadString(),
					ElementSize = reader.ReadInt32(),
					ElementsType = (ElementsType)reader.ReadInt32(),
					VertexCount = reader.ReadInt32(),
					IndexSize = reader.ReadInt32(),
					IndexCount = reader.ReadInt32()
				};

				mesh.Positions = reader.ReadBytes(Mesh.PositionSize * mesh.VertexCount);
				mesh.Elements = reader.ReadBytes(mesh.ElementSize * mesh.VertexCount);
				mesh.Indices = reader.ReadBytes(mesh.IndexSize * mesh.IndexCount);

				lod.Meshes.Add(mesh);
			}

			return lod;
		}

		private byte[] GenerateIcon(MeshLOD lod)
		{
			int width = ContentInfo.IconWidth * 4; // 4x super sampling

			using MemoryStream memStream = new();
			BitmapSource bmp = null;
			// NOTE: It's not good practice to use a WPF control (view) in the ViewModel.
			//       But we need to make an exception for this case, for as log as we don't
			//       have a graphics renderer that we can use for screenshots.
			Application.Current.Dispatcher.Invoke(() =>
			{
				bmp = Editors.GeometryView.RenderToBitmap(new Editors.MeshRenderer(lod, null), width, width);
				bmp = new TransformedBitmap(bmp, new ScaleTransform(0.25, 0.25, 0.5, 0.5));

				memStream.SetLength(0);

				PngBitmapEncoder encoder = new();
				encoder.Frames.Add(BitmapFrame.Create(bmp));
				encoder.Save(memStream);
			});

			return memStream.ToArray();
		}

		public Geometry() : base(AssetType.Mesh) { }
	}
}