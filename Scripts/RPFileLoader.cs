using System;
using System.Collections.Generic;
using System.IO;

public class SceneDescriptor {
    public BlockEntry SceneBlock;
    public BlockEntry ScriptBlock;
    public BlockEntry PatchTableBlock;
    public BlockEntry DialogBlock;
}

public static class RPFileLoader {
    public static SceneDescriptor Load(string path) {
        using var stream = File.OpenRead(path);
        using var reader = new BinaryReader(stream);

        // Read the number of block references
        uint numReferences = reader.ReadUInt32();
        if (numReferences < 4) {
            Console.WriteLine($"Warning: Expected at least 4 block references, got {numReferences}. Using available references.");
        }

        // Read the virtual addresses (up to 4, or fewer if numReferences is less)
        uint[] virtualAddresses = new uint[Math.Min(numReferences, 4)];
        for (int i = 0; i < virtualAddresses.Length; i++) {
            virtualAddresses[i] = reader.ReadUInt32();
        }

        // Define expected block types in order (PTCH corrected to GPT)
        string[] expectedTypes = { "SCNE", "GPT", "GPT", "DLG" };
        BlockEntry[] blocks = new BlockEntry[4];

        // Map virtual addresses to blocks
        for (int i = 0; i < virtualAddresses.Length; i++) {
            uint va = virtualAddresses[i];
            BlockEntry block = FindBlockByVirtualAddress(va);
            if (block == null) {
                throw new Exception($"No block found for virtual address 0x{va:X8}");
            }
            if (block.BlockType != expectedTypes[i]) {
                Console.WriteLine($"Warning: Block type mismatch at position {i}: expected {expectedTypes[i]}, got {block.BlockType}");
            }
            blocks[i] = block;
        }

        // Handle cases where fewer than 4 references are provided
        for (int i = virtualAddresses.Length; i < 4; i++) {
            blocks[i] = null; // Null for missing blocks
        }

        // Create and return SceneDescriptor
        var descriptor = new SceneDescriptor {
            SceneBlock = blocks[0],
            ScriptBlock = blocks[1],
            PatchTableBlock = blocks[2],
            DialogBlock = blocks[3]
        };

        // Debug logging
        Console.WriteLine($"Loaded .rp: {Path.GetFileName(path)}");
        for (int i = 0; i < 4; i++) {
            Console.WriteLine($"  {expectedTypes[i]} → {blocks[i]?.BlockType ?? "null"} @ {blocks[i]?.VirtualAddress:X8}");
        }

        return descriptor;
    }

    private static BlockEntry FindBlockByVirtualAddress(uint va) {
        foreach (var block in FixSNALoader.LoadedBlocks) {
            if (va >= block.VirtualAddress && va < block.VirtualAddress + block.DecompressedSize) {
                return block;
            }
        }
        return null;
    }
}