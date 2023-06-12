using System.Numerics;
using System.Runtime.InteropServices;

using PrimalEditor.Components;
using PrimalEditor.EngineAPIStructs;


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
    class GameEntityDescriptor
    {
        public TransformComponent Transform = new TransformComponent();
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
