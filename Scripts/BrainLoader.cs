using System;

public class BrainStruct {
    public uint VirtualAddress;
    public uint off_mind;
    public uint unknown1;
    public uint unknown2;
}

public static class BrainLoader {
    private const uint PointerThreshold = 0x10000000;

    /// <summary>
    /// Parses a Brain struct at the given virtual address.
    /// </summary>
    /// <param name="brainPointer">The virtual address of the Brain struct.</param>
    /// <returns>A BrainStruct object containing the parsed fields, or null if parsing fails.</returns>
    public static BrainStruct ParseBrainStruct(uint brainPointer) {
        if (brainPointer == 0) {
            Console.WriteLine("Brain pointer is null (0x00000000)");
            return null;
        }

        // Find the block containing the brainPointer
        BlockEntry block = FindBlockByVirtualAddress(brainPointer);
        if (block == null) {
            Console.WriteLine($"No block found for Brain pointer 0x{brainPointer:X8}");
            return null;
        }

        // Calculate the offset within the block
        uint offset = brainPointer - block.VirtualAddress;

        // Check if there are at least 12 bytes from the offset
        if (offset + 12 > block.Data.Length) {
            Console.WriteLine($"Insufficient data for Brain struct at VA=0x{brainPointer:X8}");
            return null;
        }

        // Read the three uint32 fields
        uint off_mind = BitConverter.ToUInt32(block.Data, (int)offset + 0);
        uint unknown1 = BitConverter.ToUInt32(block.Data, (int)offset + 4);
        uint unknown2 = BitConverter.ToUInt32(block.Data, (int)offset + 8);

        // Create BrainStruct
        var brain = new BrainStruct {
            VirtualAddress = brainPointer,
            off_mind = off_mind,
            unknown1 = unknown1,
            unknown2 = unknown2
        };

        // Log the fields
        Console.WriteLine($"Brain @ 0x{brainPointer:X8}:");
        if (off_mind >= PointerThreshold) {
            BlockEntry resolvedBlock = FindBlockByVirtualAddress(off_mind);
            if (resolvedBlock != null) {
                Console.WriteLine($"  off_mind → {resolvedBlock.BlockType} @ 0x{off_mind:X8}");
            } else {
                Console.WriteLine($"  off_mind → No matching block found for 0x{off_mind:X8}");
            }
        } else {
            Console.WriteLine($"  off_mind → 0x{off_mind:X8} (non-pointer)");
        }
        Console.WriteLine($"  unknown1 = 0x{unknown1:X8}");
        Console.WriteLine($"  unknown2 = 0x{unknown2:X8}");

        return brain;
    }

    /// <summary>
    /// Finds the BlockEntry that contains the given virtual address.
    /// </summary>
    /// <param name="va">The virtual address to find.</param>
    /// <returns>The BlockEntry containing the virtual address, or null if not found.</returns>
    private static BlockEntry FindBlockByVirtualAddress(uint va) {
        foreach (var block in FixSNALoader.LoadedBlocks) {
            if (va >= block.VirtualAddress && va < block.VirtualAddress + block.DecompressedSize) {
                return block;
            }
        }
        return null;
    }
}