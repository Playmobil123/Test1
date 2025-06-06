using System;
using System.Collections.Generic;
using HypeGame.Data;
using HypeGame.Loader;

public class SpawnerInfo {
    public uint VirtualAddress;
    public uint TargetPointer;
}

public static class SpawnerListLoader {
    private const uint PointerThreshold = 0x10000000;

    // ✅ Now accessible from TestMain.cs
    public static List<SpawnerInfo> SpawnerList { get; private set; } = new();

    public static void ParseSpawnerList(uint startPointer) {
        SpawnerList.Clear();
        ParseStructureList(startPointer, 0x20, new List<int> { 0x00, 0x04 });
    }

    public static void ParseStructureList(uint startPointer, int structureSize, List<int> pointerOffsets) {
        if (startPointer == 0x00000000) {
            Console.WriteLine("Start pointer is null (0x00000000). No spawner list to parse.");
            return;
        }

        HypeGame.Data.BlockEntry block = FindBlockByVirtualAddress(startPointer);
        if (block == null) {
            Console.WriteLine($"No block found for start pointer 0x{startPointer:X8}");
            return;
        }

        uint offsetInBlock = startPointer - block.VirtualAddress;
        int spawnerIndex = 0;

        while (offsetInBlock + (uint)structureSize <= (uint)block.Data.Length) {
            uint checkValue = BitConverter.ToUInt32(block.Data, (int)offsetInBlock);
            if (checkValue == 0x00000000) break;

            uint spawnerVA = block.VirtualAddress + offsetInBlock;
            uint targetPtr = BitConverter.ToUInt32(block.Data, (int)(offsetInBlock + 4));

            Console.WriteLine($"Spawner {spawnerIndex} @ VA=0x{spawnerVA:X8}:");

            foreach (int ptrOffset in pointerOffsets) {
                if (offsetInBlock + (uint)ptrOffset + 4 > (uint)block.Data.Length) continue;
                uint ptrValue = BitConverter.ToUInt32(block.Data, (int)(offsetInBlock + (uint)ptrOffset));
                if (ptrValue >= PointerThreshold) {
                    HypeGame.Data.BlockEntry resolvedBlock = FindBlockByVirtualAddress(ptrValue);
                    Console.WriteLine($"  → Target pointer: 0x{ptrValue:X8} → {(resolvedBlock != null ? $"BlockType={resolvedBlock.BlockType}" : "No matching block")}");
                } else if (ptrValue == 0x00000000) {
                    Console.WriteLine($"  → Target pointer: 0x00000000 (null)");
                }
            }

            SpawnerList.Add(new SpawnerInfo {
                VirtualAddress = spawnerVA,
                TargetPointer = targetPtr
            });

            offsetInBlock += (uint)structureSize;
            spawnerIndex++;
        }

        if (spawnerIndex == 0) {
            Console.WriteLine("No spawners found in the list.");
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
