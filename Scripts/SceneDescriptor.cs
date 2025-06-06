using System;

namespace HypeGame.Data {
    public class SceneDescriptor {
        public string FileName { get; set; }
        public string DisplayName { get; set; }

        // Fields required by FixSceneLoader.cs and SceneGraphLoader.cs
        public string Name { get; set; }
        public int Index { get; set; }

        public uint off_scene { get; set; }
        public uint off_gpt { get; set; }
        public uint off_patch { get; set; }
        public uint off_dial { get; set; }
    }
}
