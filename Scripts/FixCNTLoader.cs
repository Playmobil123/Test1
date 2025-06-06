using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

public static class FixCNTLoader
{
    public class Entry
    {
        public string FileName;
        public int BlockIndex;
    }

    public static List<Entry> Entries = new();

    public static void Load(string relativePath)
    {
        string fullPath = Path.Combine(AppContext.BaseDirectory, relativePath);
        if (!File.Exists(fullPath))
        {
            Console.WriteLine($"Missing fix.cnt file: {fullPath}");
            return;
        }

        byte[] raw = File.ReadAllBytes(fullPath);
        if (raw.Length < 4)
        {
            Console.WriteLine("File too small");
            return;
        }

        uint sizeEncrypted = BitConverter.ToUInt32(raw, 0) ^ 0xAEEA0AE0;
        if (raw.Length != sizeEncrypted)
        {
            Console.WriteLine($"Warning: Expected size {sizeEncrypted}, but file size is {raw.Length}");
        }

        byte[] data = new byte[raw.Length - 4];
        byte[] xorKey = { 0xE0, 0x0A, 0xEA, 0xAE };
        for (int i = 0; i < data.Length; i++)
        {
            data[i] = (byte)(raw[i + 4] ^ xorKey[i % 4]);
        }

        using var ms = new MemoryStream(data);
        using var br = new BinaryReader(ms);

        try
        {
            ushort numEntries = br.ReadUInt16();
            ushort numShuffleIndices = br.ReadUInt16();
            Console.WriteLine($"fix.cnt: numEntries={numEntries}, numShuffleIndices={numShuffleIndices}");

            // Entry table is fixed: 8 bytes per entry
            long entriesStart = ms.Position;
            long stringTableStart = entriesStart + (numEntries * 8);

            Entries.Clear();

            for (int i = 0; i < numEntries; i++)
            {
                ms.Position = entriesStart + (i * 8);

                ushort nameOffset = br.ReadUInt16();
                ushort _flags = br.ReadUInt16(); // Ignored for now
                int blockIndex = br.ReadInt32();

                // String pointer is relative to the string table start
                ms.Position = stringTableStart + nameOffset;

                List<byte> nameBytes = new();
                byte b;
                while ((b = br.ReadByte()) != 0)
                    nameBytes.Add(b);

                string name = Encoding.ASCII.GetString(nameBytes.ToArray());

                bool isValid = !string.IsNullOrWhiteSpace(name);
                Entries.Add(new Entry
                {
                    FileName = isValid ? name : string.Empty,
                    BlockIndex = blockIndex
                });

                if (isValid)
                {
                    Console.WriteLine($"Entry {i}: {name} (Block {blockIndex})");
                }
            }

            Console.WriteLine($"Loaded {Entries.Count} total entries.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error while loading fix.cnt: {ex.Message}");
        }
    }
}
