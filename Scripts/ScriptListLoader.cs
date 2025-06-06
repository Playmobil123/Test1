using System;
using System.Collections.Generic;
using HypeGame.Data;
using HypeGame.Loader;

public class ScriptEntry {
    public uint VirtualAddress;
    public uint off_scriptNodeRoot;
    public byte rawByte; // if valid
}

public static class ScriptListLoader {
    /// <summary>
    /// Parses a list of script pointers from the given virtual address and count.
    /// </summary>
    /// <param name="scriptsPointer">The virtual address of the script list.</param>
    /// <param name="scriptCount">The number of scripts in the list.</param>
    /// <param name="contextLabel">A label for logging context (e.g., "Behavior_Normal").</param>
    /// <returns>A list of ScriptEntry objects parsed from the list.</returns>
    public static List<ScriptEntry> ParseScriptList(uint scriptsPointer, int scriptCount, string contextLabel) {
        List<ScriptEntry> scripts = new List<ScriptEntry>();

        if (scriptsPointer == 0) {
            Console.WriteLine("Script list pointer is null");
            return scripts;
        }

        HypeGame.Data.BlockEntry block = FindBlockByVirtualAddress(scriptsPointer);
        if (block == null) {
            Console.WriteLine("No block found for script list pointer");
            return scripts;
        }

        uint offset = scriptsPointer - block.VirtualAddress;
        for (int i = 0; i < scriptCount; i++) {
            uint scriptOffset = offset + (uint)(i * 8);
            if (scriptOffset + 8 > block.Data.Length) {
                Console.WriteLine("Insufficient data for script " + i);
                break;
            }

            byte[] data = new byte[8];
            Array.Copy(block.Data, scriptOffset, data, 0, 8);
            uint off_scriptNodeRoot = BitConverter.ToUInt32(data, 0);
            byte rawByte = data[4];

            uint virtualAddress = block.VirtualAddress + scriptOffset;
            ScriptEntry entry = new ScriptEntry {
                VirtualAddress = virtualAddress,
                off_scriptNodeRoot = off_scriptNodeRoot,
                rawByte = rawByte
            };
            scripts.Add(entry);

            // Log the script information
            Console.WriteLine($"Script {i} [{contextLabel}] @ 0x{virtualAddress:X8}:");
            if (off_scriptNodeRoot >= 0x10000000) {
                HypeGame.Data.BlockEntry targetBlock = FindBlockByVirtualAddress(off_scriptNodeRoot);
                if (targetBlock != null) {
                    Console.WriteLine($"  Root → {targetBlock.BlockType} @ 0x{off_scriptNodeRoot:X8}");
                } else {
                    Console.WriteLine($"  Root → unknown @ 0x{off_scriptNodeRoot:X8}");
                }
            } else if (off_scriptNodeRoot == 0) {
                Console.WriteLine("  Root → null");
            } else {
                Console.WriteLine($"  Root → 0x{off_scriptNodeRoot:X8} (non-pointer)");
            }
        }

        return scripts;
    }

    /// <summary>
    /// Finds the block that contains the given virtual address.
    /// </summary>
    /// <param name="va">The virtual address to search for.</param>
    /// <returns>The HypeGame.Data.BlockEntry containing the virtual address, or null if not found.</returns>
    private static HypeGame.Data.BlockEntry FindBlockByVirtualAddress(uint va) {
        foreach (var block in FixSNALoader.LoadedBlocks) {
            if (va >= block.VirtualAddress && va < block.VirtualAddress + block.DecompressedSize) {
                return block;
            }
        }
        return null;
    }
}