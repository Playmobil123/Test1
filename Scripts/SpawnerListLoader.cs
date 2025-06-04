using System;
using System.Collections.Generic;

public static class SpawnerListLoader {
    private const uint PointerThreshold = 0x10000000;

    /// <summary>
    /// Parses the spawner list starting from the given virtual address pointer.
    /// Assumes a structure size of 0x20 bytes and checks pointer fields at offsets 0x00 and 0x04.
    /// </summary>
    /// <param name="startPointer">The virtual address where the spawner list starts, typically from SceneBlock.Data at offset 0x00.</param>
    public static void ParseSpawnerList(uint startPointer) {
        ParseStructureList(startPointer, 0x20, new List<int> { 0x00, 0x04 });
    }

    /// <summary>
    /// General method to parse a list of structures starting from a given virtual address.
    /// </summary>
    /// <param name="startPointer">The virtual address where the structure list starts.</param>
    /// <param name="structureSize">The size of each structure in bytes (default 0x20 for spawners).</param>
    /// <param name="pointerOffsets">The offsets within each structure to check for pointers (e.g., 0x00, 0x04).</param>
    public static void ParseStructureList(uint startPointer, int structureSize, List<int> pointerOffsets) {
        // Validate input
        if (startPointer == 0x00000000) {
            Console.WriteLine("Start pointer is null (0x00000000). No spawner list to parse.");
            return;
        }

        // Locate the block containing the start pointer
        BlockEntry block = FindBlockByVirtualAddress(startPointer);
        if (block == null) {
            Console.WriteLine($"No block found for start pointer 0x{startPointer:X8}");
            return;
        }

        // Calculate the offset within the block
        uint offsetInBlock = startPointer - block.VirtualAddress;

        // Parse the spawner list
        int spawnerIndex = 0;
        while (offsetInBlock + (uint)structureSize <= (uint)block.Data.Length) {
            // Check for end of list (null pointer at offset 0x00)
            uint checkValue = BitConverter.ToUInt32(block.Data, (int)offsetInBlock);
            if (checkValue == 0x00000000) {
                break;
            }

            // Calculate the virtual address of the spawner
            uint spawnerVA = block.VirtualAddress + offsetInBlock;
            Console.WriteLine($"Spawner {spawnerIndex} @ VA=0x{spawnerVA:X8}:");

            // Check specified pointer fields
            foreach (int ptrOffset in pointerOffsets) {
                if (offsetInBlock + (uint)ptrOffset + 4 > (uint)block.Data.Length) continue;
                uint ptrValue = BitConverter.ToUInt32(block.Data, (int)(offsetInBlock + (uint)ptrOffset));
                if (ptrValue >= PointerThreshold) {
                    BlockEntry resolvedBlock = FindBlockByVirtualAddress(ptrValue);
                    if (resolvedBlock != null) {
                        Console.WriteLine($"  → Target pointer: 0x{ptrValue:X8} → BlockType={resolvedBlock.BlockType}");
                    } else {
                        Console.WriteLine($"  → Target pointer: 0x{ptrValue:X8} → No matching block found");
                    }
                } else if (ptrValue == 0x00000000) {
                    Console.WriteLine($"  → Target pointer: 0x00000000 (null)");
                }
            }

            // Move to the next structure
            offsetInBlock += (uint)structureSize;
            spawnerIndex++;
        }

        if (spawnerIndex == 0) {
            Console.WriteLine("No spawners found in the list.");
        }
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