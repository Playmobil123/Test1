using HypeGame.Loader;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using HypeEngineClone;

namespace HypeGame.Loader {
    public static class FixSNALoader {
        private static readonly List<FixSnaBlock> blocks = new();
        private static readonly Dictionary<(byte module, byte id), int> relocationDeltas = new();
        private static byte[] fileData;
        private const uint BaseInMemory = 0x10000000; // Example base address, adjust as needed

        public static void LoadFixSnaFile(string path) {
            Console.WriteLine("[FixSnaLoader] Loading SNA file...");
            blocks.Clear();
            relocationDeltas.Clear();
            fileData = File.ReadAllBytes(path);
            int offset = 0;
            int index = 0;

            while (offset + 14 <= fileData.Length) { // 4 + 1 + 1 + 4 + 4 = 14 bytes for header
                uint virtualAddress = BitConverter.ToUInt32(fileData, offset);
                byte module = fileData[offset + 4];
                byte id = fileData[offset + 5];
                uint compressedSize = BitConverter.ToUInt32(fileData, offset + 6);
                uint decompressedSize = BitConverter.ToUInt32(fileData, offset + 10);

                if (compressedSize == 0 || decompressedSize == 0 || offset + 14 + compressedSize > fileData.Length) {
                    Console.WriteLine($"[WARN] Block {index} at offset 0x{offset:X8} has invalid sizes: Compressed={compressedSize}, Decompressed={decompressedSize}");
                    break;
                }

                var rawData = new byte[compressedSize];
                Array.Copy(fileData, offset + 14, rawData, 0, compressedSize);

                byte[] decompressedData;
                if (compressedSize == decompressedSize) {
                    decompressedData = rawData;
                } else {
                    try {
                        using var ms = new MemoryStream(rawData);
                        using var z = new DeflateStream(ms, CompressionMode.Decompress);
                        using var outStream = new MemoryStream();
                        z.CopyTo(outStream);
                        decompressedData = outStream.ToArray();
                    } catch (Exception ex) {
                        Console.WriteLine($"[ERROR] Block {index} decompression failed: {ex.Message}");
                        break;
                    }
                }

                // Fix 1: Calculate and store relocation delta
                int relocationDelta = (int)(BaseInMemory + blocks.Count * 0x10000) - (int)virtualAddress; // Example actual address calculation
                relocationDeltas[(module, id)] = relocationDelta;

                var block = new FixSnaBlock {
                    Index = index,
                    Offset = offset,
                    CompressedSize = compressedSize,
                    DecompressedSize = decompressedSize,
                    RawData = rawData,
                    DecompressedData = decompressedData,
                    VirtualAddress = BaseInMemory + (uint)relocationDelta, // Fix 2: Set VirtualAddress
                    Module = module,
                    Id = id
                };

                blocks.Add(block);
                offset += 14 + (int)compressedSize;
                index++;
            }

            Console.WriteLine($"[FixSnaLoader] Loaded {blocks.Count} blocks from {path}");
        }

        public static byte[] GetBlockDecompressed(int index) {
            if (index < 0 || index >= blocks.Count) return null;
            return blocks[index].DecompressedData;
        }

        public static FixSnaBlock GetBlockMeta(int index) {
            if (index < 0 || index >= blocks.Count) return null;
            return blocks[index];
        }

        public static IReadOnlyList<FixSnaBlock> LoadedBlocks => blocks;

        public static IReadOnlyDictionary<(byte module, byte id), int> RelocationDeltas => relocationDeltas;
    }
}