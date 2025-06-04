using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using lzo.net;
using System.IO.Compression;

public class BlockEntry {
    public uint CompressionFlag { get; set; }
    public uint CompressedSize { get; set; }
    public uint CompressedChecksum { get; set; }
    public uint DecompressedSize { get; set; }
    public uint DecompressedChecksum { get; set; }
    public byte[] Data { get; set; }
    public uint VirtualAddress { get; set; }
    public long FileOffset { get; set; }
    public string BlockType { get; set; } = "UNKNOWN";
}

public class CPAFixSnaLoader {
    #region Global Variables
    public static List<BlockEntry> LoadedBlocks { get; private set; } = new();
    public static Dictionary<string, List<BlockEntry>> BlockTypeMap { get; private set; } = new();
    public static object FixFileLinkedListHead { get; set; }
    public static uint[] WritePatchTable { get; set; }
    public static string PathBaseGameData { get; set; } = "gameData/";
    public static uint MainInitStatus { get; set; }
    public static int FixSnaBootPointer { get; set; }
    public static int SaveSlotIndex { get; set; }
    public static int ScreenFadeTime { get; set; }
    public static short CameraZDepth { get; set; }
    private static uint BaseVirtualAddress { get; set; } = 0x10000000;
    private static bool EnableDebugLogging { get; set; } = true; // Enabled for troubleshooting
    private const uint MaxBlockSize = 100 * 1024 * 1024; // 100 MB limit
    #endregion

    #region Utility Methods
    private static uint Adler32(uint adler, byte[] data, int len) {
        const uint MOD_ADLER = 65521;
        uint a = adler & 0xFFFF;
        uint b = (adler >> 16) & 0xFFFF;

        if (data == null) return 1;

        for (int i = 0; i < len; i++) {
            a = (a + data[i]) % MOD_ADLER;
            b = (b + a) % MOD_ADLER;
        }

        return (b << 16) | a;
    }
    #endregion

    #region Decompression and Validation
    private static void DecompressAndValidate(BlockEntry entry, BinaryReader reader) {
        try {
            if (entry.CompressionFlag == 0) {
                // Uncompressed block
                entry.Data = reader.ReadBytes((int)entry.CompressedSize);
                if (entry.Data.Length != entry.CompressedSize) {
                    throw new InvalidDataException("Failed to read complete uncompressed data");
                }
                uint chk = Adler32(0, null, 0);
                chk = Adler32(chk, entry.Data, (int)entry.DecompressedSize);
                if (chk != entry.DecompressedChecksum) {
                    throw new InvalidDataException("Uncompressed data checksum mismatch");
                }
            } else {
                // Compressed block (using LZO)
                byte[] compressedData = reader.ReadBytes((int)entry.CompressedSize);
                if (compressedData.Length != entry.CompressedSize) {
                    throw new InvalidDataException("Failed to read complete compressed data");
                }
                uint chk = Adler32(0, null, 0);
                chk = Adler32(chk, compressedData, (int)entry.CompressedSize);
                if (chk != entry.CompressedChecksum) {
                    throw new InvalidDataException("Compressed data checksum mismatch");
                }

                // Decompress using LZO
                using (var compressedStream = new MemoryStream(compressedData))
                using (var lzo = new LzoStream(compressedStream, CompressionMode.Decompress))
                using (var decompressedStream = new MemoryStream()) {
                    lzo.CopyTo(decompressedStream);
                    entry.Data = decompressedStream.ToArray();
                }

                // Check decompressed size
                if (entry.Data.Length != entry.DecompressedSize) {
                    throw new InvalidDataException($"LZO decompression failed: expected {entry.DecompressedSize} bytes, got {entry.Data.Length} bytes");
                }

                // Validate decompressed checksum
                uint chkDecompressed = Adler32(0, null, 0);
                chkDecompressed = Adler32(chkDecompressed, entry.Data, (int)entry.DecompressedSize);
                if (chkDecompressed != entry.DecompressedChecksum) {
                    throw new InvalidDataException("Decompressed data checksum mismatch");
                }
            }
        } catch (Exception ex) {
            Console.WriteLine($"Error decompressing block {entry.BlockType} at offset {entry.FileOffset:X8}: {ex.Message}");
            throw;
        }
    }
    #endregion

    #region Block Loading
    private static BlockEntry ReadBlockEntry(BinaryReader reader, int blockIndex) {
        long offset = reader.BaseStream.Position;
        try {
            // Read raw metadata bytes
            byte[] metadataBytes = reader.ReadBytes(20);
            if (metadataBytes.Length != 20) {
                Console.WriteLine($"Warning: Insufficient data for block metadata at index {blockIndex}. Skipping.");
                return null;
            }

            // Debug output of raw metadata
            if (EnableDebugLogging) {
                Console.WriteLine($"Raw metadata for block {blockIndex}: {BitConverter.ToString(metadataBytes)}");
            }

            // Parse metadata fields in little-endian
            uint compressionFlag = BitConverter.ToUInt32(metadataBytes, 0);
            uint compressedSize = BitConverter.ToUInt32(metadataBytes, 4);
            uint compressedChecksum = BitConverter.ToUInt32(metadataBytes, 8);
            uint decompressedSize = BitConverter.ToUInt32(metadataBytes, 12);
            uint decompressedChecksum = BitConverter.ToUInt32(metadataBytes, 16);

            // Validate sizes
            if (compressedSize > MaxBlockSize || decompressedSize > MaxBlockSize) {
                Console.WriteLine($"Warning: Block {blockIndex} has invalid sizes: compressed={compressedSize}, decompressed={decompressedSize}. Skipping.");
                return null;
            }

            long remainingBytes = reader.BaseStream.Length - reader.BaseStream.Position;
            if (remainingBytes < compressedSize) {
                Console.WriteLine($"Warning: Not enough data for block {blockIndex}: expected {compressedSize} bytes, but only {remainingBytes} remaining. Skipping.");
                return null;
            }

            // Create block entry
            var entry = new BlockEntry {
                CompressionFlag = compressionFlag,
                CompressedSize = compressedSize,
                CompressedChecksum = compressedChecksum,
                DecompressedSize = decompressedSize,
                DecompressedChecksum = decompressedChecksum,
                FileOffset = offset,
                BlockType = InferBlockType(reader, blockIndex)
            };

            // Log block details
            Console.WriteLine($"Processing block {blockIndex} ({entry.BlockType}) at offset {offset:X8}:");
            Console.WriteLine($"- Compressed size: {entry.CompressedSize} bytes");
            Console.WriteLine($"- Uncompressed size: {entry.DecompressedSize} bytes");
            Console.WriteLine($"- Expected compressed checksum: {entry.CompressedChecksum:X8}");
            Console.WriteLine($"- Expected uncompressed checksum: {entry.DecompressedChecksum:X8}");

            DecompressAndValidate(entry, reader);
            if (EnableDebugLogging) {
                Console.WriteLine($"Loaded block {blockIndex} ({entry.BlockType}) at offset {offset:X8}: {entry.DecompressedSize} bytes");
            }
            return entry;
        } catch (Exception ex) {
            Console.WriteLine($"Error reading block {blockIndex} at offset {offset:X8}: {ex.Message}");
            return null;
        }
    }

    private static string InferBlockType(BinaryReader reader, int index) {
        long originalPosition = reader.BaseStream.Position;
        string inferredType = "UNKNOWN";
        if (reader.BaseStream.Position + 4 <= reader.BaseStream.Length) {
            byte[] peek = reader.ReadBytes(4);
            string tag = Encoding.ASCII.GetString(peek);
            switch (tag) {
                case "GPT ": inferredType = "GPT"; break;
                case "DLG ": inferredType = "DLG"; break;
                case "SDA ": inferredType = "SDA"; break;
                case "TEX ": inferredType = "TEX"; break;
                case "SCNE": inferredType = "SCNE"; break;
                default: inferredType = $"UNK{index}"; break;
            }
            reader.BaseStream.Seek(originalPosition, SeekOrigin.Begin);
        }
        return inferredType;
    }

    public static void LoadSnaMemoryBlocks(string filename, byte mode) {
        string adjustedPath = GetAdjustedPath(filename, mode);
        if (!File.Exists(adjustedPath)) {
            Console.WriteLine($"Missing file: {adjustedPath}");
            string fallbackPath = Path.Combine(PathBaseGameData, Path.GetFileName(adjustedPath));
            if (!File.Exists(fallbackPath)) {
                Console.WriteLine($"Fallback failed: {fallbackPath}");
                return;
            }
            adjustedPath = fallbackPath;
        }
        using var stream = File.OpenRead(adjustedPath);
        LoadSnaMemoryBlocksFromStream(stream, adjustedPath);
    }

    private static string GetAdjustedPath(string filename, byte mode) {
        string dir = Path.GetDirectoryName(filename);
        if (mode == 1) {
            return Path.Combine(dir, "Fix.sna");
        }
        string mode2Path = Path.Combine(dir, "Fix_mode2.sna");
        return File.Exists(mode2Path) ? mode2Path : Path.Combine(dir, "Fix.sna");
    }

    public static void LoadSnaMemoryBlocksFromStream(Stream stream, string sourceName) {
        LoadedBlocks.Clear();
        using var reader = new BinaryReader(stream);

        int index = 0;
        while (reader.BaseStream.Position < reader.BaseStream.Length) {
            var block = ReadBlockEntry(reader, index);
            if (block != null) {
                LoadedBlocks.Add(block);
            } else {
                Console.WriteLine($"Skipped invalid block at index {index}");
            }
            index++;
        }
        AssignVirtualAddresses();
        BuildBlockTypeMap();
        Console.WriteLine($"Loaded {LoadedBlocks.Count} blocks from {sourceName}");
        if (EnableDebugLogging) {
            foreach (var kvp in BlockTypeMap) {
                Console.WriteLine($"Block type {kvp.Key}: {kvp.Value.Count} blocks");
            }
        }
    }
    #endregion

    #region Pointer Remapping
    private static void RemapPointers() {
        var pointerMap = new Dictionary<uint, uint>();
        foreach (var block in LoadedBlocks) {
            if (block.Data.Length >= 4) {
                uint potentialBase = BitConverter.ToUInt32(block.Data, 0);
                if (potentialBase >= BaseVirtualAddress) {
                    pointerMap[potentialBase] = block.VirtualAddress;
                }
            }
        }

        foreach (var block in LoadedBlocks) {
            for (int i = 0; i < block.Data.Length - 3; i += 4) {
                uint pointer = BitConverter.ToUInt32(block.Data, i);
                if (pointerMap.ContainsKey(pointer)) {
                    uint newPointer = pointerMap[pointer];
                    byte[] newPointerBytes = BitConverter.GetBytes(newPointer);
                    Array.Copy(newPointerBytes, 0, block.Data, i, 4);
                    if (EnableDebugLogging) {
                        Console.WriteLine($"Remapped pointer at {block.VirtualAddress + (uint)i:X8} from {pointer:X8} to {newPointer:X8}");
                    }
                }
            }
        }
    }
    #endregion

    #region Virtual Address Assignment
    private static void AssignVirtualAddresses() {
        uint currentAddress = BaseVirtualAddress;
        foreach (var block in LoadedBlocks) {
            block.VirtualAddress = currentAddress;
            currentAddress += block.DecompressedSize;
            if (EnableDebugLogging) {
                Console.WriteLine($"Assigned VA {block.VirtualAddress:X8} to block {block.BlockType}");
            }
        }
    }
    #endregion

    #region Block Type Mapping
    private static void BuildBlockTypeMap() {
        BlockTypeMap.Clear();
        foreach (var block in LoadedBlocks) {
            if (!BlockTypeMap.ContainsKey(block.BlockType)) {
                BlockTypeMap[block.BlockType] = new List<BlockEntry>();
            }
            BlockTypeMap[block.BlockType].Add(block);
        }
    }
    #endregion

    #region Main Boot Function
    public static void BootFixSnaCore() {
        var initialLinkedListHead = FixFileLinkedListHead;
        string fixSnaPath = Path.Combine(PathBaseGameData, "Fix.sna");

        LoadSnaMemoryBlocks(fixSnaPath, 1);
        LoadSnaMemoryBlocks(fixSnaPath, 2);

        using var fs = File.OpenRead(fixSnaPath);
        using var reader = new BinaryReader(fs);
        long headerSize = LoadedBlocks.Count * 20;
        reader.BaseStream.Seek(headerSize, SeekOrigin.Begin);

        const int patchTableEntryCount = 2198;
        WritePatchTable = new uint[patchTableEntryCount];
        var patchValues = new uint[patchTableEntryCount];
        for (int i = 0; i < patchTableEntryCount; i++) {
            WritePatchTable[i] = reader.ReadUInt32();
            patchValues[i] = reader.ReadUInt32();
        }

        ApplyWritePatchChain(WritePatchTable, patchValues, 4, patchTableEntryCount);
        RemapPointers();

        SaveSlotIndex = reader.ReadInt32();
        ScreenFadeTime = reader.ReadInt32();
        CameraZDepth = reader.ReadInt16();
        FixSnaBootPointer = reader.ReadInt32();

        FixFileLinkedListHead = initialLinkedListHead;
        Console.WriteLine("BootFixSnaCore completed");
    }

    private static void ApplyWritePatchChain(uint[] patchTable, uint[] patchValues, int elementSize, int count) {
        for (int i = 0; i < count; i++) {
            uint offset = patchTable[i];
            uint value = patchValues[i];
            bool applied = false;
            foreach (var block in LoadedBlocks) {
                if (offset >= block.VirtualAddress && offset < block.VirtualAddress + block.DecompressedSize) {
                    int localOffset = (int)(offset - block.VirtualAddress);
                    if (localOffset + elementSize <= block.Data.Length) {
                        Array.Copy(BitConverter.GetBytes(value), 0, block.Data, localOffset, elementSize);
                        if (EnableDebugLogging) {
                            Console.WriteLine($"Applied patch at {offset:X8} with value {value:X8}");
                        }
                        applied = true;
                    }
                    break;
                }
            }
            if (!applied) {
                Console.WriteLine($"Failed to apply patch at {offset:X8}: offset not found in any block");
            }
        }
    }
    #endregion
}