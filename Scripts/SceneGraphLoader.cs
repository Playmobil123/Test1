using System;
using HypeGame.Data;
using HypeGame.Loader;

public static class SceneGraphLoader {
    public static void ParseSceneGraph(SceneDescriptor descriptor) {
        HypeGame.Data.BlockEntry sceneBlock = ResolvePointer(descriptor.off_scene);
        if (sceneBlock == null) {
            Console.WriteLine("Scene block not found for offset " + descriptor.off_scene.ToString("X8"));
            return;
        }

        byte[] data = sceneBlock.Data;

        Console.WriteLine("Scene graph data found. Virtual Address: 0x" + sceneBlock.VirtualAddress.ToString("X8"));
        // TODO: Parse structure if needed
    }

    private static HypeGame.Data.BlockEntry ResolvePointer(uint pointer) {
        foreach (var block in FixSNALoader.LoadedBlocks) {
            if (pointer >= block.VirtualAddress &&
                pointer < block.VirtualAddress + block.DecompressedSize) {
                return block;
            }
        }
        return null;
    }
}
