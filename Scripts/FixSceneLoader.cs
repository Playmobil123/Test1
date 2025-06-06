using System;
using HypeGame.Data;
using HypeGame.Loader;

public static class FixSceneLoader {
    public static void LoadSceneFromDescriptor(SceneDescriptor scene) {
        Console.WriteLine($">>> Loading scene: {scene.Name} (ID: {scene.Index})");

        LoadSceneBlock("SCNE", scene.off_scene);
        LoadSceneBlock("GPT", scene.off_gpt);
        LoadSceneBlock("GPT (Patch Table)", scene.off_patch);
        LoadSceneBlock("DLG", scene.off_dial);
    }

    private static void LoadSceneBlock(string name, uint pointer) {
        if (pointer == 0 || pointer == 0xFFFFFFFF) {
            Console.WriteLine($"[Skip] {name} block is null.");
            return;
        }

        HypeGame.Data.BlockEntry block = FindBlockByVirtualAddress(pointer);
        if (block != null) {
            Console.WriteLine($"[OK] {name} block at VA=0x{pointer:X8}, BlockType={block.BlockType}, Size={block.DecompressedSize} bytes");
        } else {
            Console.WriteLine($"[ERR] {name} block not found at VA=0x{pointer:X8}");
        }
    }

    private static HypeGame.Data.BlockEntry FindBlockByVirtualAddress(uint va) {
        foreach (var block in FixSNALoader.LoadedBlocks) {
            if (va >= block.VirtualAddress && va < block.VirtualAddress + block.DecompressedSize) {
                return block;
            }
        }
        return null;
    }
}