using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

using PrimalEditor.GameProject;

namespace PrimalEditor.Content
{
	/// <summary>
	/// Interaction logic for ContentBrowser.xaml
	/// </summary>
	public partial class ContentBrowserView : UserControl
	{
		private string _sortProperty = nameof(ContentInfo.FileName);
		private ListSortDirection _sortDirection;

		public ContentBrowserView()
		{
			DataContext = null;
			InitializeComponent();
			Loaded += OnContentBrowserLoaded;
		}

		private void OnContentBrowserLoaded(object sender, RoutedEventArgs e)
		{
			Loaded -= OnContentBrowserLoaded;
			if (Application.Current?.MainWindow != null)
			{
				Application.Current.MainWindow.DataContextChanged += OnPropertyChanged;
			}

			OnPropertyChanged(null, new DependencyPropertyChangedEventArgs(DataContextProperty, null, Project.Current));
			folderListView.AddHandler(Thumb.DragDeltaEvent, new DragDeltaEventHandler(Thumb_DragDelta), true);
			folderListView.Items.SortDescriptions.Add(new SortDescription(_sortProperty, _sortDirection));
		}

		private void Thumb_DragDelta(object sender, DragDeltaEventArgs e)
		{
			if (e.OriginalSource is Thumb thumb &&
					thumb.TemplatedParent is GridViewColumnHeader header)
			{
				if (header.Column.ActualWidth < 50)
				{
					header.Column.Width = 50;
				}
				else if (header.Column.ActualWidth > 250)
				{
					header.Column.Width = 250;
				}
			}
		}

		private void OnPropertyChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			(DataContext as ContentBrowser)?.Dispose();
			DataContext = null;
			if (e.NewValue is Project project)
			{
				Debug.Assert(e.NewValue == Project.Current);
				var contentBrowser = new ContentBrowser(project);
				contentBrowser.PropertyChanged += OnSelectedFolderChanged;
				DataContext = contentBrowser;
			}
		}

		private void OnSelectedFolderChanged(object sender, PropertyChangedEventArgs e)
		{
			var vm = sender as ContentBrowser;
			if (e.PropertyName == nameof(vm.SelectedFolder) && !string.IsNullOrEmpty(vm.SelectedFolder))
			{
				GeneratePathStackButtons();
			}
		}

		private void GeneratePathStackButtons()
		{
			var vm = DataContext as ContentBrowser;
			var path = Directory.GetParent(Path.TrimEndingDirectorySeparator(vm.SelectedFolder)).FullName;
			var contentPath = Path.TrimEndingDirectorySeparator(vm.ContentFolder);

			pathStack.Children.RemoveRange(1, pathStack.Children.Count - 1);
			if (vm.SelectedFolder == vm.ContentFolder) return;

			string[] paths = new string[3];
			string[] labels = new string[3];

			int i;
			for (i = 0; i < 3; ++i)
			{
				paths[i] = path;
				labels[i] = path[(path.LastIndexOf(Path.DirectorySeparatorChar) + 1)..];
				if (path == contentPath) break;
				path = path.Substring(0, path.LastIndexOf(Path.DirectorySeparatorChar));
			}

			if (i == 3) i = 2;
			for (; i >= 0; --i)
			{
				Button btn = new()
				{
					DataContext = paths[i],
					Content = new TextBlock() { Text = labels[i], TextTrimming = TextTrimming.CharacterEllipsis }
				};
				pathStack.Children.Add(btn);
				if (i > 0) pathStack.Children.Add(new System.Windows.Shapes.Path());
			}
		}

		private void OnPathStack_Button_Click(object sender, RoutedEventArgs e)
		{
			var vm = DataContext as ContentBrowser;
			vm.SelectedFolder = (sender as Button).DataContext as string;
		}

		private void OnGridViewColumnHeader_Click(object sender, RoutedEventArgs e)
		{
			var column = sender as GridViewColumnHeader;
			var sortBy = column.Tag.ToString();

			folderListView.Items.SortDescriptions.Clear();
			var newDir = ListSortDirection.Ascending;
			if (_sortProperty == sortBy && _sortDirection == newDir)
			{
				newDir = ListSortDirection.Descending;
			}

			_sortDirection = newDir;
			_sortProperty = sortBy;

			folderListView.Items.SortDescriptions.Add(new SortDescription(sortBy, newDir));
		}

		private void OnContent_Item_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			var info = (sender as FrameworkElement).DataContext as ContentInfo;
			ExecutreSelection(info);
		}

		private void OnContent_Item_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
		{
			if (e.Key == Key.Enter)
			{
				var info = (sender as FrameworkElement).DataContext as ContentInfo;
				ExecutreSelection(info);
			}
		}

		private void ExecutreSelection(ContentInfo info)
		{
			if (info == null) return;

			if (info.IsDirectory)
			{
				var vm = DataContext as ContentBrowser;
				vm.SelectedFolder = info.FullPath;
			}
		}
	}
}
