using System;

public class StandardGameStruct {
    public uint VirtualAddress;
    public int FamilyIndex;
    public int ModelIndex;
    public int InstanceIndex;
}

public static class StandardGameLoader {
    /// <summary>
    /// Parses a StandardGame struct at the given virtual address.
    /// </summary>
    /// <param name="stdGamePointer">The virtual address of the StandardGame struct.</param>
    /// <returns>A StandardGameStruct object containing the parsed fields, or null if parsing fails.</returns>
    public static StandardGameStruct ParseStandardGame(uint stdGamePointer) {
        // Find the block containing the stdGamePointer
        BlockEntry block = null;
        foreach (var b in FixSNALoader.LoadedBlocks) {
            if (stdGamePointer >= b.VirtualAddress && stdGamePointer < b.VirtualAddress + b.DecompressedSize) {
                block = b;
                break;
            }
        }

        if (block == null) {
            Console.WriteLine($"No block found for StandardGame pointer 0x{stdGamePointer:X8}");
            return null;
        }

        // Calculate the offset within the block
        uint offset = stdGamePointer - block.VirtualAddress;

        // Check if there are at least 12 bytes from the offset
        if (offset + 12 > block.Data.Length) {
            Console.WriteLine($"Insufficient data for StandardGame struct at VA=0x{stdGamePointer:X8}");
            return null;
        }

        // Read the three integers
        int objectTypeFamily = BitConverter.ToInt32(block.Data, (int)offset + 0);
        int objectTypeModel = BitConverter.ToInt32(block.Data, (int)offset + 4);
        int objectTypeInstance = BitConverter.ToInt32(block.Data, (int)offset + 8);

        // Create and populate StandardGameStruct
        var stdGame = new StandardGameStruct {
            VirtualAddress = stdGamePointer,
            FamilyIndex = objectTypeFamily,
            ModelIndex = objectTypeModel,
            InstanceIndex = objectTypeInstance
        };

        // Log the result
        Console.WriteLine($"StandardGame @ 0x{stdGamePointer:X8}: Family={objectTypeFamily}, Model={objectTypeModel}, Instance={objectTypeInstance}");

        return stdGame;
    }
}