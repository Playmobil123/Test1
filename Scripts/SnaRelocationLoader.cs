using System;
using System.Collections.Generic;

namespace HypeEngineClone
{
    public static class SnaRelocationLoader
    {
        public static void LoadFromTable(SnaRelocationTable table, byte[] patchOffsets)
        {
            if (patchOffsets.Length % 9 != 0)
                throw new ArgumentException("Relocation table is malformed (must be 9 bytes per entry)");

            int count = patchOffsets.Length / 9;
            for (int i = 0; i < count; i++)
            {
                byte module = patchOffsets[i * 9 + 0];
                byte block  = patchOffsets[i * 9 + 1];

                int offset = BitConverter.ToInt32(patchOffsets, i * 9 + 2); // signed delta
                table.Add(module, block, offset);

                Console.WriteLine($"[Relocation] Module {module}, Block {block} → delta {offset}");
            }
        }
    }
}
