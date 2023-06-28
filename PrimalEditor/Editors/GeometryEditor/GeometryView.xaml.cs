using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Media3D;

namespace PrimalEditor.Editors
{
    /// <summary>
    /// Interaction logic for GeometryView.xaml
    /// </summary>
    public partial class GeometryView : UserControl
    {
        private Point _clickedPositon;
        private bool _captureLeft;
        private bool _captureRight;

        public void SetGeometry(int index = -1)
        {
            if (!(DataContext is MeshRenderer vm)) return;

            if (vm.Meshes.Any() && viewport.Children.Count == 2)
            {
                viewport.Children.RemoveAt(1);
            }

            var meshIndex = 0;
            var modelGroup = new Model3DGroup();
            foreach (var mesh in vm.Meshes)
            {
                if (index != -1 && meshIndex != index)
                {
                    ++meshIndex;
                    continue;
                }

                var mesh3D = new MeshGeometry3D()
                {
                    Positions = mesh.Positions,
                    Normals = mesh.Normals,
                    TriangleIndices = mesh.Indices,
                    TextureCoordinates = mesh.UVs
                };

                var diffuse = new DiffuseMaterial(mesh.Diffuse);
                var specular = new SpecularMaterial(mesh.Specular, 50);
                var matGroup = new MaterialGroup();
                matGroup.Children.Add(diffuse);
                matGroup.Children.Add(specular);

                var model = new GeometryModel3D(mesh3D, matGroup);
                modelGroup.Children.Add(model);

                var binding = new Binding(nameof(mesh.Diffuse)) { Source = mesh };
                BindingOperations.SetBinding(diffuse, DiffuseMaterial.BrushProperty, binding);

                if (meshIndex == index) break;
            }

            var visul = new ModelVisual3D() { Content = modelGroup };
            viewport.Children.Add(visul);
        }

        public GeometryView()
        {
            InitializeComponent();
            DataContextChanged += (s, e) => SetGeometry();
        }

        private void OnGrid_Mouse_LBD(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            _clickedPositon = e.GetPosition(this);
            _captureLeft = true;
            Mouse.Capture(sender as UIElement);
        }

        private void OnGrid_Mouse_Move(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (_captureLeft && !_captureRight) return;

            var pos = e.GetPosition(this);
            var d = pos - _clickedPositon;

            if (_captureLeft && !_captureRight)
            {
                // MoveCamera(d.X, d.Y, 0);
            }
            else if (!_captureLeft && _captureRight)
            {
                var vm = DataContext as MeshRenderer;
                var cp = vm.CameraPosition;
                var yOffset = d.Y * 0.001 * Math.Sqrt(cp.X * cp.X + cp.Z * cp.Z);
                vm.CameraTarget = new Point3D(vm.CameraTarget.X, vm.CameraTarget.Y + yOffset, vm.CameraTarget.Z);
            }
        }

        private void OnGrid_Mouse_LBU(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            _captureLeft = false;
            if (!_captureRight) Mouse.Capture(null);
        }

        private void OnGrid_Mouse_Wheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {

        }

        private void OnGrid_Mouse_RBD(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {

        }

        private void OnGrid_Mouse_RBU(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {

        }
    }
}
