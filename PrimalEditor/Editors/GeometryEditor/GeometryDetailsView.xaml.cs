﻿using System.Windows.Controls;

namespace PrimalEditor.Editors
{
	/// <summary>
	/// Interaction logic for GeometryDetailsView.xaml
	/// </summary>
	public partial class GeometryDetailsView : UserControl
	{
		public GeometryDetailsView()
		{
			InitializeComponent();
		}

		private void OnHighlight_CheckBox_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			var vm = DataContext as GeometryEditor;
			foreach (var m in vm.MeshRenderer.Meshes)
			{
				m.IsHighlighted = false;
			}

			var checkBox = sender as CheckBox;
			(checkBox.DataContext as MeshRendererVertexData).IsHighlighted = checkBox.IsChecked == true;
		}

		private void OnIsolate_CheckBox_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			var vm = DataContext as GeometryEditor;
			foreach (var m in vm.MeshRenderer.Meshes)
			{
				m.IsIsolated = false;
			}

			var checkBox = sender as CheckBox;
			var mesh = checkBox.DataContext as MeshRendererVertexData;
			mesh.IsIsolated = checkBox.IsChecked == true;

			if (Tag is GeometryView geometryView)
			{
				geometryView.SetGeometry(mesh.IsIsolated ? vm.MeshRenderer.Meshes.IndexOf(mesh) : -1);
			}
		}
	}
}
