using System;
using HypeGame.Data;
using HypeGame.Loader; // ✅ Needed for FixSNALoader

public class MindStruct {
    public uint VirtualAddress;
    public uint off_AI_model;
    public uint off_intelligence_normal;
    public uint off_intelligence_reflex;
    public uint off_dsgMem;
    public uint off_name;
    public byte byte0, byte1, byte2, byte3;
}

public static class MindLoader {
    private const uint PointerThreshold = 0x10000000;

    /// <summary>
    /// Parses a Mind struct at the given virtual address.
    /// </summary>
    public static MindStruct ParseMindStruct(uint mindPointer) {
        if (mindPointer == 0) {
            Console.WriteLine("Mind pointer is null (0x00000000)");
            return null;
        }

        HypeGame.Data.BlockEntry block = FindBlockByVirtualAddress(mindPointer);
        if (block == null) {
            Console.WriteLine($"No block found for Mind pointer 0x{mindPointer:X8}");
            return null;
        }

        uint offset = mindPointer - block.VirtualAddress;
        if (offset + 0x18 > block.Data.Length) {
            Console.WriteLine($"Insufficient data for Mind struct at VA=0x{mindPointer:X8}");
            return null;
        }

        uint off_AI_model = BitConverter.ToUInt32(block.Data, (int)offset + 0x00);
        uint off_intelligence_normal = BitConverter.ToUInt32(block.Data, (int)offset + 0x04);
        uint off_intelligence_reflex = BitConverter.ToUInt32(block.Data, (int)offset + 0x08);
        uint off_dsgMem = BitConverter.ToUInt32(block.Data, (int)offset + 0x0C);
        uint off_name = BitConverter.ToUInt32(block.Data, (int)offset + 0x10);
        byte byte0 = block.Data[offset + 0x14];
        byte byte1 = block.Data[offset + 0x15];
        byte byte2 = block.Data[offset + 0x16];
        byte byte3 = block.Data[offset + 0x17];

        var mind = new MindStruct {
            VirtualAddress = mindPointer,
            off_AI_model = off_AI_model,
            off_intelligence_normal = off_intelligence_normal,
            off_intelligence_reflex = off_intelligence_reflex,
            off_dsgMem = off_dsgMem,
            off_name = off_name,
            byte0 = byte0,
            byte1 = byte1,
            byte2 = byte2,
            byte3 = byte3
        };

        Console.WriteLine($"Mind @ 0x{mindPointer:X8}:");
        LogPointer("off_AI_model", off_AI_model);
        LogPointer("off_intelligence_normal", off_intelligence_normal);
        LogPointer("off_intelligence_reflex", off_intelligence_reflex);
        LogPointer("off_dsgMem", off_dsgMem);
        LogPointer("off_name", off_name);
        Console.WriteLine($"  byte0 = 0x{byte0:X2}, byte1 = 0x{byte1:X2}, byte2 = 0x{byte2:X2}, byte3 = 0x{byte3:X2}");

        return mind;
    }

    private static void LogPointer(string fieldName, uint pointer) {
        if (pointer >= PointerThreshold) {
            HypeGame.Data.BlockEntry resolvedBlock = FindBlockByVirtualAddress(pointer);
            if (resolvedBlock != null) {
                Console.WriteLine($"  {fieldName} → {resolvedBlock.BlockType} @ 0x{pointer:X8}");
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
