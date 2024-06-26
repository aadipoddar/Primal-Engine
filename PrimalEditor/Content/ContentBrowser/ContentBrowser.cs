﻿using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;

using PrimalEditor.Common;
using PrimalEditor.GameProject;
using PrimalEditor.Utilities;

namespace PrimalEditor.Content
{
	sealed class ContentInfo
	{
		public static int IconWidth => 90;
		public byte[] Icon { get; }
		public byte[] IconSmall { get; }

		public string FullPath { get; }
		public string FileName => Path.GetFileNameWithoutExtension(FullPath);
		public bool IsDirectory { get; }
		public DateTime DateModified { get; }
		public long? Size { get; }

		public ContentInfo(string fullPath, byte[] icon = null, byte[] smallIcon = null, DateTime? lastModified = null)
		{
			Debug.Assert(File.Exists(fullPath) || Directory.Exists(fullPath));
			var info = new FileInfo(fullPath);
			IsDirectory = ContentHelper.IsDirectory(fullPath);
			DateModified = lastModified ?? info.LastWriteTime;
			Size = IsDirectory ? (long?)null : info.Length;
			Icon = icon;
			IconSmall = smallIcon ?? icon;
			FullPath = fullPath;
		}
	}

	class ContentBrowser : ViewModelBase, IDisposable
	{
		private readonly DelayEventTimer _refreshTimer = new DelayEventTimer(TimeSpan.FromMilliseconds(250));

		public string ContentFolder { get; }

		private readonly ObservableCollection<ContentInfo> _folderContent = new ObservableCollection<ContentInfo>();
		public ReadOnlyObservableCollection<ContentInfo> FolderContent { get; }

		private string _selectedFolder;
		public string SelectedFolder
		{
			get { return _selectedFolder; }
			set
			{
				if (_selectedFolder != value)
				{
					_selectedFolder = value;
					if (!string.IsNullOrEmpty(_selectedFolder))
					{
						_ = GetFolderContent();
					}
					OnPropertyChanged(nameof(SelectedFolder));
				}
			}
		}

		private void OnContentModified(object sender, ContentModifiedEventArgs e)
		{
			if (Path.GetDirectoryName(e.FullPath) != SelectedFolder) return;
			_refreshTimer.Trigger();
		}

		private void Refresh(object sender, DelayEventTimerArgs e)
		{
			_ = GetFolderContent();
		}

		private async Task GetFolderContent()
		{
			var folderContent = new List<ContentInfo>();
			await Task.Run(() =>
			{
				folderContent = GetFolderContent(SelectedFolder);
			});

			_folderContent.Clear();
			folderContent.ForEach(x => _folderContent.Add(x));
		}

		private static List<ContentInfo> GetFolderContent(string path)
		{
			Debug.Assert(!string.IsNullOrEmpty(path));
			var folderContent = new List<ContentInfo>();

			try
			{
				// Get Sub-Folder
				foreach (var dir in Directory.GetDirectories(path))
				{
					folderContent.Add(new ContentInfo(dir));
				}

				// Get Files
				foreach (var file in Directory.GetFiles(path, $"*{Asset.AssetFileExtension}"))
				{
					var fileInfo = new FileInfo(file);

					folderContent.Add(ContentInfoCache.Add(file));
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message);
			}
			return folderContent;
		}

		public void Dispose()
		{
			ContentWatcher.ContentModified -= OnContentModified;
			ContentInfoCache.Save();
		}

		public ContentBrowser(Project project)
		{
			Debug.Assert(project != null);
			var contentFolder = project.ContentPath;
			Debug.Assert(!string.IsNullOrEmpty(contentFolder.Trim()));
			contentFolder = Path.TrimEndingDirectorySeparator(contentFolder);
			ContentFolder = contentFolder;
			SelectedFolder = contentFolder;
			FolderContent = new ReadOnlyObservableCollection<ContentInfo>(_folderContent);

			ContentWatcher.ContentModified += OnContentModified;
			_refreshTimer.Triggered += Refresh;
		}
	}
}
