using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using HypeGame.Data;

public static class GameDescriptorLoader {
    public class WorldDescriptor {
        public string Name;
        public uint Offset;
        public uint Pointer;
        public uint Unknown;
    }

    public static List<WorldDescriptor> worldDescriptors = new();
    public static List<SceneDescriptor> SceneList = new();

    public static void Load(string path) {
        if (!File.Exists(path)) {
            Console.WriteLine($"gamedsc.bin not found: {path}");
            return;
        }

        Console.WriteLine($">>> Loading GameDescriptor: {path}");

        byte[] gamedscData = File.ReadAllBytes(path);
        int offset = 0;
        while (offset + 0x18 <= gamedscData.Length) {
            string name = Encoding.ASCII.GetString(gamedscData, offset, 0x10).TrimEnd('\0');
            uint ptr = BitConverter.ToUInt32(gamedscData, offset + 0x10);
            uint unknown = BitConverter.ToUInt32(gamedscData, offset + 0x14);

            var wd = new WorldDescriptor {
                Name = name,
                Offset = (uint)offset,
                Pointer = ptr,
                Unknown = unknown
            };
            worldDescriptors.Add(wd);

            Console.WriteLine($">>> World: {name} (ptr=0x{ptr:X8})");

            int remaining = gamedscData.Length - (offset + 0x18);
            int levelCount = remaining / 0x10;
            for (int i = 0; i < levelCount; i++) {
                int scenePtr = offset + 0x18 + (i * 0x10);
                if (scenePtr + 0x10 > gamedscData.Length) break;

                uint off_scene = BitConverter.ToUInt32(gamedscData, scenePtr + 0x00);
                uint off_gpt = BitConverter.ToUInt32(gamedscData, scenePtr + 0x04);
                uint off_patch = BitConverter.ToUInt32(gamedscData, scenePtr + 0x08);
                uint off_dial = BitConverter.ToUInt32(gamedscData, scenePtr + 0x0C);

                var sd = new SceneDescriptor {
                    Name = $"{name}_Level{i}",
                    Index = i,
                    off_scene = off_scene,
                    off_gpt = off_gpt,
                    off_patch = off_patch,
                    off_dial = off_dial
                };

                SceneList.Add(sd);
                Console.WriteLine($"  Scene {i}: SCNE=0x{off_scene:X8}, GPT=0x{off_gpt:X8}, PATCH=0x{off_patch:X8}, DLG=0x{off_dial:X8}");
            }

            offset += 0x18 + (levelCount * 0x10);
        }

        Console.WriteLine($">>> Loaded {SceneList.Count} scenes across {worldDescriptors.Count} worlds.");
    }
}
