using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;

namespace PrimalEditor.Editors
{
    /// <summary>
    /// Interaction logic for GeometryView.xaml
    /// </summary>
    public partial class GeometryView : UserControl
    {
        private static readonly GeometryView _geometryView = new GeometryView() { Background = (Brush)Application.Current.FindResource("Editor.Window.GrayBrush4") };
        private Point _clickedPosition;
        private bool _capturedLeft;
        private bool _capturedRight;

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

        private void OnGrid_Mouse_LBD(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            _clickedPosition = e.GetPosition(this);
            _capturedLeft = true;
            Mouse.Capture(sender as UIElement);
        }

        private void OnGrid_Mouse_Move(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (!_capturedLeft && !_capturedRight) return;

            Point pos = e.GetPosition(this);
            Vector d = pos - _clickedPosition;

            if (_capturedLeft && !_capturedRight)
            {
                MoveCamera(d.X, d.Y, 0);
            }
            else if (!_capturedLeft && _capturedRight)
            {
                MeshRenderer vm = DataContext as MeshRenderer;
                Point3D cp = vm.CameraPosition;
                double yOffset = d.Y * 0.001 * Math.Sqrt(cp.X * cp.X + cp.Z * cp.Z);
                vm.CameraTarget = new Point3D(vm.CameraTarget.X, vm.CameraTarget.Y + yOffset, vm.CameraTarget.Z);
            }

            _clickedPosition = pos;
        }

        private void OnGrid_Mouse_LBU(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            _capturedLeft = false;
            if (!_capturedRight) Mouse.Capture(null);
        }

        private void OnGrid_Mouse_Wheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            MoveCamera(0, 0, Math.Sign(e.Delta));
        }

        private void OnGrid_Mouse_RBD(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            _clickedPosition = e.GetPosition(this);
            _capturedRight = true;
            Mouse.Capture(sender as UIElement);
        }

        private void OnGrid_Mouse_RBU(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            _capturedRight = false;
            if (!_capturedLeft) Mouse.Capture(null);
        }

        private void MoveCamera(double dx, double dy, int dz)
        {
            MeshRenderer vm = DataContext as MeshRenderer;
            Vector3D v = new(vm.CameraPosition.X, vm.CameraPosition.Y, vm.CameraPosition.Z);

            double r = v.Length;
            double theta = Math.Acos(v.Y / r);
            double phi = Math.Atan2(-v.Z, v.X);

            theta -= dy * 0.01;
            phi -= dx * 0.01;
            r *= 1.0 - 0.1 * dz; // dx is either +1 or -1

            theta = Math.Clamp(theta, 0.0001, Math.PI - 0.0001);

            v.X = r * Math.Sin(theta) * Math.Cos(phi);
            v.Y = r * Math.Cos(theta);
            v.Z = -r * Math.Sin(theta) * Math.Sin(phi);

            vm.CameraPosition = new Point3D(v.X, v.Y, v.Z);
        }

        internal static BitmapSource RenderToBitmap(MeshRenderer mesh, int width, int height)
        {
            var bmp = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Default);

            _geometryView.DataContext = mesh;
            _geometryView.Width = width;
            _geometryView.Height = height;
            _geometryView.Measure(new Size(width, height));
            _geometryView.Arrange(new Rect(0, 0, width, height));
            _geometryView.UpdateLayout();

            bmp.Render(_geometryView);
            return bmp;
        }

        public GeometryView()
        {
            InitializeComponent();
            DataContextChanged += (s, e) => SetGeometry();
        }
    }
}
