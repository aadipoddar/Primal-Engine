using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

using PrimalEditor.GameProject;

namespace PrimalEditor.Content
{
	/// <summary>
	/// Interaction logic for ContentBrowser.xaml
	/// </summary>
	public partial class ContentBrowserView : UserControl
	{
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
				
			}
		}
	}
}
