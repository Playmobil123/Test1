// ✅ BrainLoader.cs (fully fixed)
using System;
using HypeGame.Data;
using HypeGame.Loader;

public class BrainStruct {
    public uint VirtualAddress;
    public uint off_name;
    public uint off_script;
    public uint off_transitions;
    public uint transitionCount;
    public uint off_constants;
    public uint constantCount;
}

public static class BrainLoader {
    private const uint PointerThreshold = 0x10000000;

    public static BrainStruct ParseBrainStruct(uint brainPointer) {
        if (brainPointer == 0) {
            Console.WriteLine("Brain pointer is null (0x00000000)");
            return null;
        }

        HypeGame.Data.BlockEntry block = FindBlockByVirtualAddress(brainPointer);
        if (block == null) {
            Console.WriteLine($"No block found for Brain pointer 0x{brainPointer:X8}");
            return null;
        }

        uint offset = brainPointer - block.VirtualAddress;
        if (offset + 0x1C > block.Data.Length) {
            Console.WriteLine($"Insufficient data for Brain struct at VA=0x{brainPointer:X8}");
            return null;
        }

        var brain = new BrainStruct {
            VirtualAddress = brainPointer,
            off_name = BitConverter.ToUInt32(block.Data, (int)offset + 0x00),
            off_script = BitConverter.ToUInt32(block.Data, (int)offset + 0x04),
            off_transitions = BitConverter.ToUInt32(block.Data, (int)offset + 0x08),
            transitionCount = BitConverter.ToUInt32(block.Data, (int)offset + 0x0C),
            off_constants = BitConverter.ToUInt32(block.Data, (int)offset + 0x10),
            constantCount = BitConverter.ToUInt32(block.Data, (int)offset + 0x14),
        };

        Console.WriteLine($"Brain @ 0x{brainPointer:X8}:");
        LogPointer("off_name", brain.off_name);
        LogPointer("off_script", brain.off_script);
        LogPointer("off_transitions", brain.off_transitions);
        Console.WriteLine($"  transitionCount = {brain.transitionCount}");
        LogPointer("off_constants", brain.off_constants);
        Console.WriteLine($"  constantCount = {brain.constantCount}");

        return brain;
    }

    private static void LogPointer(string fieldName, uint pointer) {
        if (pointer >= PointerThreshold) {
            HypeGame.Data.BlockEntry block = FindBlockByVirtualAddress(pointer);
            if (block != null) {
                Console.WriteLine($"  {fieldName} → {block.BlockType} @ 0x{pointer:X8}");
            } else {
                Console.WriteLine($"  {fieldName} → No matching block found for 0x{pointer:X8}");
            }
        } else {
            Console.WriteLine($"  {fieldName} → 0x{pointer:X8} (non-pointer)");
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
