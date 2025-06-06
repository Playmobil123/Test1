using System;
using HypeEngineClone;

public static class PatchTableInterpreter
{
    /// <summary>
    /// Reads patch table entries at the given offset in Fix.sna and loads them into the relocation table.
    /// </summary>
    public static void Apply(SnaBinaryReader reader, int patchTableOffset)
    {
        reader.Seek(patchTableOffset);

        Console.WriteLine($"[PatchTable] Reading patch entries at offset 0x{patchTableOffset:X8}");

        int maxEntries = 256; // safety cap
        for (int i = 0; i < maxEntries && !reader.EndOfStream; i++)
        {
            byte mod = reader.ReadBytes(1)[0];
            byte blk = reader.ReadBytes(1)[0];
            int delta = reader.ReadInt(); // already remapped

            if (mod == 0xFF && blk == 0xFF)
            {
                Console.WriteLine("[PatchTable] Reached terminator.");
                break;
            }

            reader.Relocation.Add(mod, blk, delta);
            Console.WriteLine($"[PatchTable] + Module={mod}, Block={blk}, Offset={delta}");
        }
    }
}
