using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;

using PrimalEditor.Common;

namespace PrimalEditor.Content
{
	static class AssetRegistry
	{
		private static readonly Dictionary<string, AssetInfo> _assetDictionary = new();
		private static readonly ObservableCollection<AssetInfo> _assets = new();

		public static ReadOnlyObservableCollection<AssetInfo> Assets { get; } = new ReadOnlyObservableCollection<AssetInfo>(_assets);

		private static void RegisterAllAssets(string path)
		{
			Debug.Assert(Directory.Exists(path));
			foreach (string entry in Directory.GetFileSystemEntries(path))
			{
				if (ContentHelper.IsDirectory(entry))
				{
					RegisterAllAssets(entry);
				}
				else
				{
					RegisterAsset(entry);
				}
			}
		}

		private static void RegisterAsset(string file)
		{
			Debug.Assert(File.Exists(file));
			try
			{
				FileInfo fileInfo = new(file);
				if (!_assetDictionary.ContainsKey(file) || _assetDictionary[file].RegisterTime.IsOlder(fileInfo.LastWriteTime))
				{
					AssetInfo info = Asset.GetAssetInfo(file);
					Debug.Assert(info != null);
					info.RegisterTime = DateTime.Now;
					_assetDictionary[file] = info;

					Debug.Assert(_assetDictionary.ContainsKey(file));
					_assets.Add(_assetDictionary[file]);
				}
			}
			catch (Exception ex) { Debug.WriteLine(ex.Message); }
		}

		private static void UnregisterAsset(string file)
		{
			if (_assetDictionary.ContainsKey(file))
			{
				_assets.Remove(_assetDictionary[file]);
				_assetDictionary.Remove(file);
			}
		}

		private static void OnContentModified(object sender, ContentModifiedEventArgs e)
		{
			if (ContentHelper.IsDirectory(e.FullPath))
			{
				RegisterAllAssets(e.FullPath);
			}
			else if (File.Exists(e.FullPath))
			{
				RegisterAsset(e.FullPath);
			}

			_assets.Where(x => !File.Exists(x.FullPath)).ToList().ForEach(x => UnregisterAsset(x.FullPath));
		}

		public static void Reset(string contentFolder)
		{
			ContentWatcher.ContentModified -= OnContentModified;

			_assetDictionary.Clear();
			_assets.Clear();

			Debug.Assert(Directory.Exists(contentFolder));
			RegisterAllAssets(contentFolder);

			ContentWatcher.ContentModified += OnContentModified;
		}

		public static AssetInfo GetAssetInfo(string file) => _assetDictionary.ContainsKey(file) ? _assetDictionary[file] : null;

		public static AssetInfo GetAssetInfo(Guid guid) => _assets.FirstOrDefault(x => x.Guid == guid);
	}
}