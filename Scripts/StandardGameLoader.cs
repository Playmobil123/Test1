using System;
using HypeGame.Data;
using HypeGame.Loader;

public static class StandardGameLoader {
    public static StandardGameStruct ParseStandardGameStruct(uint ptr) {
        HypeGame.Data.BlockEntry block = FindBlockByVirtualAddress(ptr);
        if (block == null) return default;

        int offset = (int)(ptr - block.VirtualAddress);
        if (offset + 8 > block.Data.Length) return default;

        return new StandardGameStruct {
            FamilyIndex = block.Data[offset],
            ModelIndex = block.Data[offset + 1],
            InstanceIndex = block.Data[offset + 2],
            InstanceFlags = block.Data[offset + 3],
            Additional = BitConverter.ToUInt32(block.Data, offset + 4)
        };
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

public struct StandardGameStruct {
    public byte FamilyIndex;
    public byte ModelIndex;
    public byte InstanceIndex;
    public byte InstanceFlags;
    public uint Additional;
}
