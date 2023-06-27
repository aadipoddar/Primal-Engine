using System.Windows;
using System.Windows.Controls;

using PrimalEditor.ContentToolsAPIStructs;
using PrimalEditor.DllWrapper;
using PrimalEditor.Utilities.Controls;

namespace PrimalEditor.Content
{
    /// <summary>
    /// Interaction logic for PrimitiveMeshDialog.xaml
    /// </summary>
    public partial class PrimitiveMeshDialog : Window
    {
        private void OnPrimitiveType_ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) => UpdatePrimitive();

        private void OnSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) => UpdatePrimitive();

        private void OnScalarBox_ValueChanged(object sender, RoutedEventArgs e) => UpdatePrimitive();

        private float Value(ScalarBox scalarBox, float min)
        {
            float.TryParse(scalarBox.Value, out var result);
            return Math.Max(result, min);
        }

        private void UpdatePrimitive()
        {
            if (!IsInitialized) return;

            var primitiveType = (PrimitveMeshType)primalTypeComboBox.SelectedIndex;
            var info = new PrimitiveInitInfo() { Type = primitiveType };

            switch (primitiveType)
            {
                case PrimitveMeshType.Plane:
                    {
                        info.SegmentX = (int)xSliderPlane.Value;
                        info.SegmentZ = (int)zSliderPlane.Value;
                        info.Size.X = Value(widthScalarBoxPlane, 0.001f);
                        info.Size.Z = Value(lengthScalarBoxPlane, 0.001f);
                        break;
                    }
                case PrimitveMeshType.Cube:
                    break;
                case PrimitveMeshType.UVSphere:
                    break;
                case PrimitveMeshType.IcoSphere:
                    break;
                case PrimitveMeshType.Cylinder:
                    break;
                case PrimitveMeshType.Capsule:
                    break;
                default:
                    break;
            }

            var geometry = new Geometry();
            ContentToolsAPI.CreatePrimitiveMesh(geometry, info);
        }

        public PrimitiveMeshDialog()
        {
            InitializeComponent();
            Loaded += (s, e) => UpdatePrimitive();
        }
    }
}
