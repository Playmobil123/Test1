using System;
using System.Collections.Generic;

public static class SceneGraphLoader {
    private const uint PointerThreshold = 0x10000000;
    private const int HeaderSize = 0x20; // 32 bytes for header parsing

    public static void ParseSceneBlock(SceneDescriptor descriptor) {
        // Validate input
        if (descriptor.SceneBlock == null) {
            Console.WriteLine("SceneBlock is null");
            return;
        }

        byte[] data = descriptor.SceneBlock.Data;
        if (data == null || data.Length < HeaderSize) {
            Console.WriteLine("SceneBlock data is too small");
            return;
        }

        // Log root SCNE block info
        Console.WriteLine($"Root SCNE block found: VA=0x{descriptor.SceneBlock.VirtualAddress:X8}, size={data.Length}");

        // Parse header fields (first 32 bytes, in 4-byte increments)
        for (int offset = 0; offset < HeaderSize; offset += 4) {
            if (offset + 4 > data.Length) break;

            uint value = BitConverter.ToUInt32(data, offset);
            if (value >= PointerThreshold) {
                // Treat as pointer and attempt to resolve
                BlockEntry resolvedBlock = ResolvePointer(value);
                if (resolvedBlock != null) {
                    Console.WriteLine($"→ Field at offset 0x{offset:X2}: VA=0x{value:X8} → BlockType={resolvedBlock.BlockType}");
                } else {
                    Console.WriteLine($"→ Field at offset 0x{offset:X2}: VA=0x{value:X8} → No matching block found");
                }
            } else {
                // Treat as non-pointer value
                Console.WriteLine($"→ Field at offset 0x{offset:X2}: 0x{value:X8} → treated as non-pointer");
            }
        }
    }

    private static BlockEntry ResolvePointer(uint pointer) {
        foreach (var block in FixSNALoader.LoadedBlocks) {
            if (pointer >= block.VirtualAddress && pointer < block.VirtualAddress + block.DecompressedSize) {
                return block;
            }
        }
        return null;
    }
}