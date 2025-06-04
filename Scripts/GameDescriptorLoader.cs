using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

public static class GameDescriptorLoader {
    private static byte[] gamedscData;
    private static int gamedscPointer = 0;

    private static int nbWorlds = 0;
    private static List<WorldDescriptor> worldDescriptors = new();

    public static void Load(string relativePath) {
        if (string.IsNullOrEmpty(relativePath)) return;

        string fullPath = Path.Combine(AppContext.BaseDirectory, relativePath);
        if (!File.Exists(fullPath)) {
            Console.WriteLine($"Could not find: {fullPath}");
            return;
        }

        ParseDescriptor(fullPath);
        ScanWorldDescriptors();
    }

    private static void ParseDescriptor(string fileName) {
        gamedscData = File.ReadAllBytes(fileName);
        gamedscPointer = 0;
    }

    private static void ScanWorldDescriptors() {
        worldDescriptors.Clear();

        if (gamedscData == null || gamedscData.Length < 5) return;

        nbWorlds = gamedscData[0];
        Console.WriteLine($"Expecting {nbWorlds} worlds");

        int found = 0;
        int ptr = 1;

        while (found < nbWorlds && ptr + 0x18 < gamedscData.Length) {
            int nameStart = ptr + 2;
            int nameEnd = nameStart;

            while (nameEnd < gamedscData.Length && gamedscData[nameEnd] != 0) nameEnd++;

            if (nameEnd == nameStart || nameEnd >= gamedscData.Length) {
                ptr++;
                continue;
            }

            string name = Encoding.ASCII.GetString(gamedscData, nameStart, nameEnd - nameStart);

            if (string.IsNullOrWhiteSpace(name) || name.Length > 30 || name.Contains("\\") || name.Contains("/"))
            {
                ptr++;
                continue;
            }

            ushort id = BitConverter.ToUInt16(gamedscData, ptr + 0x14);
            byte index = gamedscData[ptr + 0x16];
            byte levelCount = gamedscData[ptr + 0x17];

            if (id == 0 || id > 10000 || levelCount > 120) {
                ptr++;
                continue;
            }

            var wd = new WorldDescriptor {
                StartOffset = ptr,
                Name = name,
                WorldId = id,
                WorldIndex = index,
                LevelCount = levelCount
            };

            Console.WriteLine($"World {found} found at 0x{ptr:X}, name={wd.Name}, ID={wd.WorldId}, Index={wd.WorldIndex}, Levels={wd.LevelCount}");

            worldDescriptors.Add(wd);
            found++;

            ptr = nameEnd + 1;
        }

        Console.WriteLine($"World count: {worldDescriptors.Count}");
        Console.WriteLine("-----------------------------------");
    }

    public static int GetWorldCount() => worldDescriptors.Count;
    public static IReadOnlyList<WorldDescriptor> GetWorldDescriptors() => worldDescriptors;
}

public class WorldDescriptor {
    public int StartOffset;
    public string Name;
    public ushort WorldId;
    public byte WorldIndex;
    public byte LevelCount;
}
