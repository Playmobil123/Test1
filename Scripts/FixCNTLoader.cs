using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

public static class FixCNTLoader {
    public class Entry {
        public string FileName;
    }

    public static List<Entry> Entries = new();

    public static void Load(string relativePath) {
        string fullPath = Path.Combine(AppContext.BaseDirectory, relativePath);
        if (!File.Exists(fullPath)) {
            Console.WriteLine($"Missing fix.cnt file: {fullPath}");
            return;
        }

        byte[] raw = File.ReadAllBytes(fullPath);
        if (raw.Length < 4) {
            Console.WriteLine("File too small");
            return;
        }

        // Decrypt the first 4 bytes to get the total size
        uint sizeEncrypted = BitConverter.ToUInt32(raw, 0) ^ 0xaeea0ae0;
        if (raw.Length != sizeEncrypted) {
            Console.WriteLine($"Warning: Expected size {sizeEncrypted}, but file size is {raw.Length}");
        }

        // Decrypt the remaining data (starting at offset 4)
        byte[] data = new byte[raw.Length - 4];
        byte[] xorKey = { 0xE0, 0x0A, 0xEA, 0xAE };
        for (int i = 0; i < data.Length; i++) {
            data[i] = (byte)(raw[i + 4] ^ xorKey[i % 4]);
        }

        using var ms = new MemoryStream(data);
        using var br = new BinaryReader(ms);

        try {
            // Read the number of entries and shuffle indices
            ushort numEntries = br.ReadUInt16();
            ushort numShuffleIndices = br.ReadUInt16();
            Console.WriteLine($"fix.cnt: numEntries={numEntries}, numShuffleIndices={numShuffleIndices}");

            Entries.Clear();
            for (int i = 0; i < numEntries; i++) {
                byte length = br.ReadByte();
                byte[] nameBytes = br.ReadBytes(length);
                string name = Encoding.ASCII.GetString(nameBytes);
                bool isValidName = !string.IsNullOrWhiteSpace(name) && name.All(c => c >= 32 && c <= 126);
                Entries.Add(new Entry {
                    FileName = isValidName ? name : string.Empty
                });
                if (isValidName) {
                    Console.WriteLine($"Entry {Entries.Count}: {name}");
                }
            }
            Console.WriteLine($"Loaded {Entries.Count} total entries.");
        } catch (Exception ex) {
            Console.WriteLine($"Error while loading fix.cnt: {ex.Message}");
        }
    }
}