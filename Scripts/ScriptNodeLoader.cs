using System;
using System.Collections.Generic;

public static class ScriptNodeLoader {
    /// <summary>
    /// Parses and logs the script node tree starting from the given root pointer.
    /// </summary>
    /// <param name="rootPointer">The virtual address of the root script node.</param>
    /// <param name="contextLabel">A label for logging context.</param>
    public static void ParseScriptNodeTree(uint rootPointer, string contextLabel) {
        if (rootPointer == 0) {
            Console.WriteLine("Root pointer is null");
            return;
        }

        // Find the block containing the root pointer
        BlockEntry block = FindBlockByVirtualAddress(rootPointer);
        if (block == null) {
            Console.WriteLine("No block found for root pointer");
            return;
        }

        // Start parsing from the root
        ParseNodeRecursive(block, rootPointer - block.VirtualAddress, 0, contextLabel);
    }

    /// <summary>
    /// Recursively parses and logs script nodes starting from the given offset in the block.
    /// </summary>
    /// <param name="block">The memory block containing the nodes.</param>
    /// <param name="offset">The offset within the block to start parsing.</param>
    /// <param name="currentIndent">The current indentation level.</param>
    /// <param name="contextLabel">A label for logging context.</param>
    /// <returns>The next offset after parsing the current node and its subtree.</returns>
    private static uint ParseNodeRecursive(BlockEntry block, uint offset, int currentIndent, string contextLabel) {
        while (offset + 8 <= block.Data.Length) {
            // Read the 8-byte node
            byte[] nodeData = new byte[8];
            Array.Copy(block.Data, offset, nodeData, 0, 8);

            uint param = BitConverter.ToUInt32(nodeData, 0);
            byte a = nodeData[4];
            byte b = nodeData[5];
            byte indent = nodeData[6];
            byte type = nodeData[7];

            // Check if we've moved to a lower indent level (end of subtree)
            if (indent < currentIndent) {
                return offset; // Return to parent level
            }

            // Log the node
            string indentStr = new string(' ', indent * 2);
            string typeStr = GetNodeTypeString(type);
            string paramStr = (param >= 0x10000000) ? ResolvePointer(param) : $"{param}";

            Console.WriteLine($"{indentStr}[{indent}] type={typeStr} (0x{type:X2}), param={paramStr}");

            // Move to the next node
            uint nextOffset = offset + 8;

            // If this node has children (next node has higher indent), recurse
            if (nextOffset + 8 <= block.Data.Length) {
                byte nextIndent = block.Data[nextOffset + 6];
                if (nextIndent > indent) {
                    nextOffset = ParseNodeRecursive(block, nextOffset, indent + 1, contextLabel);
                }
            }

            // Update offset for the next sibling
            offset = nextOffset;
        }

        return offset;
    }

    /// <summary>
    /// Resolves a pointer to a block type and address string.
    /// </summary>
    /// <param name="pointer">The pointer to resolve.</param>
    /// <returns>A string representing the block type and address, or "unknown" if not found.</returns>
    private static string ResolvePointer(uint pointer) {
        BlockEntry block = FindBlockByVirtualAddress(pointer);
        if (block != null) {
            return $"{block.BlockType} @ 0x{pointer:X8}";
        }
        return $"unknown @ 0x{pointer:X8}";
    }

    /// <summary>
    /// Gets a string representation of the node type.
    /// </summary>
    /// <param name="type">The node type byte.</param>
    /// <returns>A string describing the node type.</returns>
    private static string GetNodeTypeString(byte type) {
        switch (type) {
            case 0x23: return "Procedure";
            case 0x12: return "Condition";
            case 0x20: return "Operator";
            case 0x01: return "Constant";
            default: return $"Unknown (0x{type:X2})";
        }
    }

    /// <summary>
    /// Finds the block containing the given virtual address.
    /// </summary>
    /// <param name="va">The virtual address to search for.</param>
    /// <returns>The BlockEntry containing the address, or null if not found.</returns>
    private static BlockEntry FindBlockByVirtualAddress(uint va) {
        foreach (var block in FixSNALoader.LoadedBlocks) {
            if (va >= block.VirtualAddress && va < block.VirtualAddress + block.DecompressedSize) {
                return block;
            }
        }
        return null;
    }
}
