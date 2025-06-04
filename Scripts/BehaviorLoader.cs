using System;
using System.Collections.Generic;

public class BehaviorStruct {
    public uint VirtualAddress;
    public uint off_name;
    public uint off_scripts;
    public uint scriptCount;
    public uint off_schedule;
    public uint flags;
    public byte byte0;
    public byte byte1;
    public byte byte2;
    public byte byte3;
}

public static class BehaviorLoader {
    /// <summary>
    /// Parses a list of behaviors (normal or reflex) from the given pointer and count.
    /// </summary>
    /// <param name="behaviorListPointer">The virtual address of the behavior list.</param>
    /// <param name="behaviorCount">The number of behaviors in the list.</param>
    /// <param name="behaviorTypeLabel">The type label for logging ("Normal" or "Reflex").</param>
    /// <returns>A list of BehaviorStruct objects parsed from the list.</returns>
    public static List<BehaviorStruct> ParseBehaviorList(uint behaviorListPointer, int behaviorCount, string behaviorTypeLabel) {
        var behaviors = new List<BehaviorStruct>();

        // Check if the pointer is null
        if (behaviorListPointer == 0) {
            Console.WriteLine("Behavior list pointer is null");
            return behaviors;
        }

        // Find the block containing the behavior list
        BlockEntry block = FindBlockByVirtualAddress(behaviorListPointer);
        if (block == null) {
            Console.WriteLine($"No block found for behavior list pointer 0x{behaviorListPointer:X8}");
            return behaviors;
        }

        // Calculate offset and validate data size
        uint offset = behaviorListPointer - block.VirtualAddress;
        if (offset + (uint)(behaviorCount * 0x1C) > block.Data.Length) {
            Console.WriteLine("Insufficient data for behavior list");
            return behaviors;
        }

        // Parse each behavior entry
        for (int i = 0; i < behaviorCount; i++) {
            uint behaviorOffset = offset + (uint)(i * 0x1C);
            byte[] data = new byte[0x1C];
            Array.Copy(block.Data, behaviorOffset, data, 0, 0x1C);

            // Extract fields from the 0x1C-byte entry
            uint off_name = BitConverter.ToUInt32(data, 0);
            uint off_scripts = BitConverter.ToUInt32(data, 4);
            uint scriptCount = BitConverter.ToUInt32(data, 8);
            uint off_schedule = BitConverter.ToUInt32(data, 12);
            uint flags = BitConverter.ToUInt32(data, 16);
            byte byte0 = data[20];
            byte byte1 = data[21];
            byte byte2 = data[22];
            byte byte3 = data[23];

            // Calculate the virtual address of this behavior
            uint virtualAddress = block.VirtualAddress + behaviorOffset;

            // Create and populate the BehaviorStruct
            var behavior = new BehaviorStruct {
                VirtualAddress = virtualAddress,
                off_name = off_name,
                off_scripts = off_scripts,
                scriptCount = scriptCount,
                off_schedule = off_schedule,
                flags = flags,
                byte0 = byte0,
                byte1 = byte1,
                byte2 = byte2,
                byte3 = byte3
            };

            behaviors.Add(behavior);

            // Log the behavior information
            Console.WriteLine($"Behavior {i} ({behaviorTypeLabel}) @ 0x{virtualAddress:X8}:");
            Console.Write($"  scripts[{scriptCount}] → ");
            LogPointer(off_scripts);
            Console.Write("  namePtr → ");
            LogPointer(off_name);
        }

        return behaviors;
    }

    /// <summary>
    /// Logs a pointer, resolving it to a block type if possible.
    /// </summary>
    private static void LogPointer(uint pointer) {
        if (pointer >= 0x10000000) {
            BlockEntry block = FindBlockByVirtualAddress(pointer);
            if (block != null) {
                Console.WriteLine($"{block.BlockType} @ 0x{pointer:X8}");
            } else {
                Console.WriteLine("No matching block found");
            }
        } else {
            Console.WriteLine($"0x{pointer:X8}");
        }
    }

    /// <summary>
    /// Finds a block by its virtual address in the loaded blocks.
    /// </summary>
    private static BlockEntry FindBlockByVirtualAddress(uint va) {
        foreach (var block in FixSNALoader.LoadedBlocks) {
            if (va >= block.VirtualAddress && va < block.VirtualAddress + block.DecompressedSize) {
                return block;
            }
        }
        return null;
    }
}