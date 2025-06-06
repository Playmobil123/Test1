using System;
using HypeGame.Data;
using HypeGame.Loader;

public class AIModelStruct {
    public uint VirtualAddress;
    public uint off_familyName;
    public uint behaviorCount;
    public uint off_behaviors_normal;
    public uint reflexCount;
    public uint off_behaviors_reflex;
    public uint macroCount;
    public uint off_macros;
    public uint off_dsgVar;
    public uint flags;
}

public static class AIModelLoader {
    private const uint PointerThreshold = 0x10000000;

    /// <summary>
    /// Parses an AIModel struct at the given virtual address.
    /// </summary>
    /// <param name="aiModelPointer">The virtual address of the AIModel struct.</param>
    /// <returns>An AIModelStruct object containing the parsed fields, or null if parsing fails.</returns>
    public static AIModelStruct ParseAIModelStruct(uint aiModelPointer) {
        if (aiModelPointer == 0) {
            Console.WriteLine("AIModel pointer is null (0x00000000)");
            return null;
        }

        // Find the block containing the aiModelPointer
        HypeGame.Data.BlockEntry block = FindBlockByVirtualAddress(aiModelPointer);
        if (block == null) {
            Console.WriteLine($"No block found for AIModel pointer 0x{aiModelPointer:X8}");
            return null;
        }

        // Calculate the offset within the block
        uint offset = aiModelPointer - block.VirtualAddress;

        // Check if there are enough bytes for the AIModel struct (36 bytes to include all fields)
        if (offset + 36 > block.Data.Length) {
            Console.WriteLine($"Insufficient data for AIModel struct at VA=0x{aiModelPointer:X8}");
            return null;
        }

        // Read the 36 bytes
        byte[] structData = new byte[36];
        Array.Copy(block.Data, offset, structData, 0, 36);

        // Parse the fields (little-endian)
        uint off_familyName = BitConverter.ToUInt32(structData, 0);
        uint behaviorCount = BitConverter.ToUInt32(structData, 4);
        uint off_behaviors_normal = BitConverter.ToUInt32(structData, 8);
        uint reflexCount = BitConverter.ToUInt32(structData, 12);
        uint off_behaviors_reflex = BitConverter.ToUInt32(structData, 16);
        uint macroCount = BitConverter.ToUInt32(structData, 20);
        uint off_macros = BitConverter.ToUInt32(structData, 24);
        uint off_dsgVar = BitConverter.ToUInt32(structData, 28);
        uint flags = BitConverter.ToUInt32(structData, 32);

        // Create AIModelStruct instance
        var aiModel = new AIModelStruct {
            VirtualAddress = aiModelPointer,
            off_familyName = off_familyName,
            behaviorCount = behaviorCount,
            off_behaviors_normal = off_behaviors_normal,
            reflexCount = reflexCount,
            off_behaviors_reflex = off_behaviors_reflex,
            macroCount = macroCount,
            off_macros = off_macros,
            off_dsgVar = off_dsgVar,
            flags = flags
        };

        // Log the struct details
        Console.WriteLine($"AIModel @ 0x{aiModelPointer:X8}:");
        Console.Write("  Family Name → ");
        LogPointer(off_familyName);
        Console.Write($"  Normal Behaviors: {behaviorCount} → ");
        LogPointer(off_behaviors_normal);
        Console.Write($"  Reflex Behaviors: {reflexCount} → ");
        LogPointer(off_behaviors_reflex);
        Console.Write($"  Macros: {macroCount} → ");
        LogPointer(off_macros);
        Console.Write("  DsgVar Table → ");
        LogPointer(off_dsgVar);
        Console.WriteLine($"  Flags: 0x{flags:X8}");

        return aiModel;
    }

    /// <summary>
    /// Logs the resolved block type for a given pointer.
    /// </summary>
    /// <param name="pointer">The pointer value to resolve.</param>
    private static void LogPointer(uint pointer) {
        if (pointer >= PointerThreshold) {
            HypeGame.Data.BlockEntry block = FindBlockByVirtualAddress(pointer);
            if (block != null) {
                Console.WriteLine($"{block.BlockType} @ 0x{pointer:X8}");
            } else {
                Console.WriteLine($"No matching block found for 0x{pointer:X8}");
            }
        } else {
            Console.WriteLine($"0x{pointer:X8} (non-pointer)");
        }
    }

    /// <summary>
    /// Finds the HypeGame.Data.BlockEntry containing the specified virtual address.
    /// </summary>
    /// <param name="va">The virtual address to search for.</param>
    /// <returns>The HypeGame.Data.BlockEntry if found, otherwise null.</returns>
    private static HypeGame.Data.BlockEntry FindBlockByVirtualAddress(uint va) {
        foreach (var block in FixSNALoader.LoadedBlocks) {
            if (va >= block.VirtualAddress && va < block.VirtualAddress + block.DecompressedSize) {
                return block;
            }
        }
        return null;
    }
}
