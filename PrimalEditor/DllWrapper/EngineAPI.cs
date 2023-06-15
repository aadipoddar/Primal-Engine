using System.Numerics;
using System.Runtime.InteropServices;

using PrimalEditor.Components;
using PrimalEditor.EngineAPIStructs;
using PrimalEditor.GameProject;
using PrimalEditor.Utilities;

namespace PrimalEditor.EngineAPIStructs
{
    [StructLayout(LayoutKind.Sequential)]
    class TransformComponent
    {
        public Vector3 Position;
        public Vector3 Rotation;
        public Vector3 Scale = new Vector3(1, 1, 1);
    }

    [StructLayout(LayoutKind.Sequential)]
    class ScriptComponent
    {
        public IntPtr ScriptCreator;
    }

    [StructLayout(LayoutKind.Sequential)]
    class GameEntityDescriptor
    {
        public TransformComponent Transform = new TransformComponent();
        public ScriptComponent Script = new ScriptComponent();
    }
}


namespace PrimalEditor.DllWrapper
{
    static class EngineAPI
    {
        private const string _engineDLL = "EngineDLL.dll";

        [DllImport(_engineDLL, CharSet = CharSet.Ansi)]
        public static extern int LoadGameCodeDLL(string dllPath);

        [DllImport(_engineDLL)]
        public static extern int UnloadGameCodeDLL();

        [DllImport(_engineDLL)]
        public static extern IntPtr GetScriptCreator(string name);

        [DllImport(_engineDLL)]
        [return: MarshalAs(UnmanagedType.SafeArray)]
        public static extern string[] GetScriptNames();

        [DllImport(_engineDLL)]
        public static extern int CreateRenderSurface(IntPtr host, int width, int height);

        [DllImport(_engineDLL)]
        public static extern int RemoveRenderSurface(int surfaceId);

        [DllImport(_engineDLL)]
        public static extern IntPtr GetWindowHandle(int surfaceId);


        internal static class EntityAPI
        {
            [DllImport(_engineDLL)]
            private static extern int CreateGameEntity(GameEntityDescriptor desc);
            public static int CreateGameEntity(GameEntity entity)
            {
                GameEntityDescriptor desc = new GameEntityDescriptor();

                // Transform component
                {
                    var c = entity.GetComponent<Transform>();
                    desc.Transform.Position = c.Position;
                    desc.Transform.Rotation = c.Rotation;
                    desc.Transform.Scale = c.Scale;
                }

                // Script component
                {
                    // Note: Here we also check if the cuurent project is not null, so we can tell whether the game code DLL has been loaded or not.
                    // This way, creation of entities with a script component is deferred until the DLL has been loaded.
                    var c = entity.GetComponent<Script>();
                    if (c != null && Project.Current != null)
                    {
                        if (Project.Current.AvailableScripts.Contains(c.Name))
                        {
                            desc.Script.ScriptCreator = GetScriptCreator(c.Name);
                        }
                        else
                        {
                            Logger.Log(MessageType.Error, $"Unable to Find Script with Name {c.Name}. Game Entity will be created without Script Component!");
                        }
                    }
                }

                return CreateGameEntity(desc);
            }

            [DllImport(_engineDLL)]
            private static extern void RemoveGameEntity(int id);
            public static void RemoveGameEntity(GameEntity entity)
            {
                RemoveGameEntity(entity.EntityId);
            }
        }
    }
}
