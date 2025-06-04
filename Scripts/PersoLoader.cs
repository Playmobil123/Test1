using System;

public class PersoStruct {
    public uint VirtualAddress;
    public uint off_3dData;
    public uint off_stdGame;
    public uint off_dynam;
    public uint off_brain;
    public uint off_camera;
    public uint off_collSet;
    public uint off_msWay;
    public uint off_msLight;
    public uint off_sectInfo;
}

public static class PersoLoader {
    private const uint PointerThreshold = 0x10000000;

    /// <summary>
    /// Parses a Perso struct at the given virtual address.
    /// </summary>
    /// <param name="persoPointer">The virtual address of the Perso struct.</param>
    /// <returns>A PersoStruct object containing the parsed fields.</returns>
    public static PersoStruct ParsePersoStruct(uint persoPointer) {
        // Find the block containing the persoPointer
        BlockEntry block = FindBlockByVirtualAddress(persoPointer);
        if (block == null) {
            Console.WriteLine($"No block found for Perso pointer 0x{persoPointer:X8}");
            return null;
        }

        // Calculate the offset within the block
        uint offsetInBlock = persoPointer - block.VirtualAddress;

        // Check if there are enough bytes left in the block
        if (offsetInBlock + 36 > block.Data.Length) {
            Console.WriteLine($"Insufficient data for Perso struct at VA=0x{persoPointer:X8}");
            return null;
        }

        // Read the 36 bytes for the Perso struct
        byte[] persoData = new byte[36];
        Array.Copy(block.Data, offsetInBlock, persoData, 0, 36);

        // Parse the fields
        uint off_3dData = BitConverter.ToUInt32(persoData, 0);
        uint off_stdGame = BitConverter.ToUInt32(persoData, 4);
        uint off_dynam = BitConverter.ToUInt32(persoData, 8);
        uint off_brain = BitConverter.ToUInt32(persoData, 12);
        uint off_camera = BitConverter.ToUInt32(persoData, 16);
        uint off_collSet = BitConverter.ToUInt32(persoData, 20);
        uint off_msWay = BitConverter.ToUInt32(persoData, 24);
        uint off_msLight = BitConverter.ToUInt32(persoData, 28);
        uint off_sectInfo = BitConverter.ToUInt32(persoData, 32);

        // Log the resolved pointers
        LogPointer("off_3dData", off_3dData);
        LogPointer("off_stdGame", off_stdGame);
        LogPointer("off_dynam", off_dynam);
        LogPointer("off_brain", off_brain);
        LogPointer("off_camera", off_camera);
        LogPointer("off_collSet", off_collSet);
        LogPointer("off_msWay", off_msWay);
        LogPointer("off_msLight", off_msLight);
        LogPointer("off_sectInfo", off_sectInfo);

        // Create and return PersoStruct
        return new PersoStruct {
            VirtualAddress = persoPointer,
            off_3dData = off_3dData,
            off_stdGame = off_stdGame,
            off_dynam = off_dynam,
            off_brain = off_brain,
            off_camera = off_camera,
            off_collSet = off_collSet,
            off_msWay = off_msWay,
            off_msLight = off_msLight,
            off_sectInfo = off_sectInfo
        };
    }

    /// <summary>
    /// Logs the resolved block type for a given pointer.
    /// </summary>
    /// <param name="fieldName">The name of the field (e.g., "off_brain").</param>
    /// <param name="pointer">The pointer value to resolve.</param>
    private static void LogPointer(string fieldName, uint pointer) {
        if (pointer >= PointerThreshold) {
            BlockEntry resolvedBlock = FindBlockByVirtualAddress(pointer);
            if (resolvedBlock != null) {
                Console.WriteLine($"{fieldName} → {resolvedBlock.BlockType} @ 0x{pointer:X8}");
            } else {
                Console.WriteLine($"{fieldName} → No matching block found for 0x{pointer:X8}");
            }
        } else {
            Console.WriteLine($"{fieldName} → 0x{pointer:X8} (non-pointer)");
        }
    }

    /// <summary>
    /// Finds the BlockEntry that contains the given virtual address.
    /// </summary>
    /// <param name="va">The virtual address to find.</param>
    /// <returns>The BlockEntry containing the virtual address, or null if not found.</returns>
    private static BlockEntry FindBlockByVirtualAddress(uint va) {
        foreach (var block in FixSNALoader.LoadedBlocks) {
            if (va >= block.VirtualAddress && va < block.VirtualAddress + block.DecompressedSize) {
                return block;
            }
        }
        return null;
    }
}