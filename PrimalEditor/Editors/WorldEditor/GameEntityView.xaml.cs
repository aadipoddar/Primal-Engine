using System.Windows.Controls;
using System.Windows.Controls.Primitives;

using PrimalEditor.Components;
using PrimalEditor.GameProject;
using PrimalEditor.Utilities;

namespace PrimalEditor.Editors
{
    /// <summary>
    /// Interaction logic for GameEntityView.xaml
    /// </summary>
    public partial class GameEntityView : UserControl
    {
        private Action _undoAction;
        private string _propertyName;

        public static GameEntityView Instance { get; private set; }

        public GameEntityView()
        {
            InitializeComponent();
            DataContext = null;
            Instance = this;
            DataContextChanged += (_, _) =>
            {
                if (DataContext != null)
                {
                    (DataContext as MSEntity).PropertyChanged += (s, e) => _propertyName = e.PropertyName;
                }
            };
        }


        private Action GetRenameAction()
        {
            var vm = DataContext as MSEntity;
            var selection = vm.SelectedEntities.Select(entity => (entity, entity.Name)).ToList();
            return new Action(() =>
            {
                selection.ForEach(item => item.entity.Name = item.Name);
                (DataContext as MSEntity).Refresh();
            });
        }

        private Action GetIsEnabledAction()
        {
            var vm = DataContext as MSEntity;
            var selection = vm.SelectedEntities.Select(entity => (entity, entity.IsEnabled)).ToList();
            return new Action(() =>
            {
                selection.ForEach(item => item.entity.IsEnabled = item.IsEnabled);
                (DataContext as MSEntity).Refresh();
            });
        }

        private void OnName_TextBox_GotKeyboardFocus(object sender, System.Windows.Input.KeyboardFocusChangedEventArgs e)
        {
            _propertyName = string.Empty;
            _undoAction = GetRenameAction();
        }

        private void OnName_TextBox_LostKeyboardFocus(object sender, System.Windows.Input.KeyboardFocusChangedEventArgs e)
        {
            if (_propertyName == nameof(MSEntity.Name) && _undoAction != null)
            {
                var redoAction = GetRenameAction();

                Project.UndoRedo.Add(new UndoRedoAction(_undoAction, redoAction, "Rename Game Entity"));
                _propertyName = null;
            }
            _undoAction = null;
        }

        private void OnIsEnabled_CheckBox_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var undoAction = GetIsEnabledAction();
            var vm = DataContext as MSEntity;
            vm.IsEnabled = (sender as CheckBox).IsChecked == true;
            var redoAction = GetIsEnabledAction();
            Project.UndoRedo.Add(new UndoRedoAction(undoAction, redoAction,
                vm.IsEnabled == true ? "Enable Game Entity" : "Disable Game Entity"));
        }

        private void OnAddComponent_Button_PreviewMouse_LBD(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var menu = FindResource("addComponentMenu") as ContextMenu;
            var btn = sender as ToggleButton;
            btn.IsChecked = true;
            menu.Placement = PlacementMode.Bottom;
            menu.PlacementTarget = btn;
            menu.MinWidth = btn.ActualWidth;
            menu.IsOpen = true;
        }

        private void AddComponent(ComponentType componentType, object data)
        {
            var creationFunction = ComponentFactory.GetCreationFunction(componentType);
            var changedEntities = new List<(GameEntity entity, Component component)>();
            var vm = DataContext as MSEntity;

            foreach (var entity in vm.SelectedEntities)
            {
                var component = creationFunction(entity, data);
                if (entity.AddComponent(component))
                    changedEntities.Add((entity, component));
            }

            if (changedEntities.Any())
            {
                vm.Refresh();

                Project.UndoRedo.Add(new UndoRedoAction(
                                    () =>
                                    {
                                        changedEntities.ForEach(x => x.entity.RemoveComponent(x.component));
                                        (DataContext as MSEntity).Refresh();
                                    },
                                    () =>
                                    {
                                        changedEntities.ForEach(x => x.entity.AddComponent(x.component));
                                        (DataContext as MSEntity).Refresh();
                                    },
                                    $"Add {componentType} Component"
                                    ));
            }
        }

        private void OnAddScriptComponent(object sender, System.Windows.RoutedEventArgs e)
        {
            AddComponent(ComponentType.Script, (sender as MenuItem).Header.ToString());
        }
    }
}
