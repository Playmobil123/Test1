using System;

public class DsgVarInfoEntry {
    public DsgVarType type;
    public ushort number;
    public ushort additional;
    public ushort offset;
    public ushort initValueIndex;
}

public class DsgMem {
    public DsgVarValue[] variables;
    public DsgVarInfoEntry[] varInfos;
}

public static class DsgMemLoader {
    public static DsgMem LoadDsgMem(uint dsgVarPointer) {
        // Find the block containing the dsgVarPointer
        BlockEntry block = FindBlockByVirtualAddress(dsgVarPointer);
        if (block == null) {
            Console.WriteLine("No block found for DsgVar pointer");
            return null;
        }

        // Calculate offset within the block
        uint offset = dsgVarPointer - block.VirtualAddress;

        // Read dsgVarCount
        if (offset + 4 > block.Data.Length) {
            Console.WriteLine("Insufficient data for dsgVarCount");
            return null;
        }
        uint dsgVarCount = BitConverter.ToUInt32(block.Data, (int)offset);

        // Each DsgVarInfoEntry is 12 bytes: 4 (type) + 2*4 (ushorts)
        uint infoSize = 12;
        uint infoArrayOffset = offset + 4;
        if (infoArrayOffset + dsgVarCount * infoSize > block.Data.Length) {
            Console.WriteLine("Insufficient data for DsgVarInfoEntry array");
            return null;
        }

        // Parse DsgVarInfoEntry array
        DsgVarInfoEntry[] varInfos = new DsgVarInfoEntry[dsgVarCount];
        for (uint i = 0; i < dsgVarCount; i++) {
            uint entryOffset = infoArrayOffset + i * infoSize;
            byte[] entryData = new byte[infoSize];
            Array.Copy(block.Data, entryOffset, entryData, 0, infoSize);

            DsgVarInfoEntry entry = new DsgVarInfoEntry {
                type = (DsgVarType)BitConverter.ToUInt32(entryData, 0),
                number = BitConverter.ToUInt16(entryData, 4),
                additional = BitConverter.ToUInt16(entryData, 6),
                offset = BitConverter.ToUInt16(entryData, 8),
                initValueIndex = BitConverter.ToUInt16(entryData, 10)
            };
            varInfos[i] = entry;
        }

        // Create DsgMem instance
        DsgMem mem = new DsgMem {
            varInfos = varInfos,
            variables = new DsgVarValue[dsgVarCount]
        };

        // Populate variables
        for (int i = 0; i < dsgVarCount; i++) {
            DsgVarValue value = new DsgVarValue {
                type = varInfos[i].type,
                value = DsgVarUtil.GetDefaultValue(varInfos[i].type)
            };

            // Optional: Resolve initValueIndex if it's a pointer
            if (varInfos[i].initValueIndex >= 0x10000000) {
                // Assuming initValueIndex could be a pointer to a DsgVarValue
                // Uncomment and implement if pointer resolution is needed
                // value = DsgVarValue.Read(varInfos[i].initValueIndex);
            }

            mem.variables[i] = value;
        }

        // Log the result
        Console.WriteLine($"Loaded DsgMem with {dsgVarCount} variables:");
        for (int i = 0; i < dsgVarCount; i++) {
            string typeStr = mem.varInfos[i].type.ToString();
            string valueStr = mem.variables[i].value?.ToString() ?? "null";
            if (mem.varInfos[i].type == DsgVarType.Perso) {
                valueStr = mem.variables[i].value?.ToString() ?? "null";
            }
            Console.WriteLine($"  [{i}] → Type={typeStr}, Value={valueStr}");
        }

        return mem;
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