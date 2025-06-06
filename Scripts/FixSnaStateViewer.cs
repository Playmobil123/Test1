using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HypeGame.Loader;

namespace HypeEngineClone {
    public class FixSnaStateSnapshot {
        public int FontCharCount;
        public int CameraZDepth;
        public int ScreenFadeTime;
        public int SaveSlotIndex;
        public int DialogSpeed;
        public int GlobalRuntimeHash;
        public int GaugeState;
        public int InventoryStatus;
        public int MatrixStackPointer;
        public int StartupFlags;

        public void Print() {
            Console.WriteLine("=== 📸 FIX.SNA STATE SNAPSHOT ===");
            Console.WriteLine($"Font Characters    : {FontCharCount}");
            Console.WriteLine($"Camera Z-Depth     : {CameraZDepth}");
            Console.WriteLine($"Screen Fade Time   : {ScreenFadeTime}");
            Console.WriteLine($"Save Slot Index    : {SaveSlotIndex}");
            Console.WriteLine($"Dialog Speed       : {DialogSpeed}");
            Console.WriteLine($"Runtime Hash       : 0x{GlobalRuntimeHash:X8}");
            Console.WriteLine($"Gauge State        : {GaugeState}");
            Console.WriteLine($"Inventory Status   : {InventoryStatus}");
            Console.WriteLine($"Matrix Stack Ptr   : {MatrixStackPointer}");
            Console.WriteLine($"Startup Flags      : 0x{StartupFlags:X}");
        }
    }

    public class FixSnaStateViewer {
        public FixSnaStateSnapshot Snapshot { get; private set; }

        public void ShowSnapshot(string snaPath, uint snapshotPtr) {
            FixSNALoader.LoadFixSnaFile(snaPath);
            ShowSnapshot(FixSNALoader.LoadedBlocks, snapshotPtr);
        }

        public void ShowSnapshot(IReadOnlyList<FixSnaBlock> blocks, uint snapshotPtr) {
            var block = blocks.FirstOrDefault(b => b.VirtualAddress == snapshotPtr);
            if (block == null) {
                Console.WriteLine($"[FixSnaViewer] Error: Block not found for VA=0x{snapshotPtr:X8}");
                return;
            }

            if (block.DecompressedData == null || block.DecompressedData.Length < 40) {
                Console.WriteLine($"[FixSnaViewer] Error: Invalid or incomplete block data.");
                return;
            }

            using var ms = new MemoryStream(block.DecompressedData);
            using var reader = new BinaryReader(ms);
            Snapshot = new FixSnaStateSnapshot();

            try {
                Snapshot.FontCharCount      = reader.ReadInt32();
                Snapshot.CameraZDepth       = reader.ReadInt32();
                Snapshot.ScreenFadeTime     = reader.ReadInt32();
                Snapshot.SaveSlotIndex      = reader.ReadInt32();
                Snapshot.DialogSpeed        = reader.ReadInt32();
                Snapshot.GlobalRuntimeHash  = reader.ReadInt32();
                Snapshot.GaugeState         = reader.ReadInt32();
                Snapshot.InventoryStatus    = reader.ReadInt32();
                Snapshot.MatrixStackPointer = reader.ReadInt32();
                Snapshot.StartupFlags       = reader.ReadInt32();
            } catch (Exception ex) {
                Console.WriteLine($"[FixSnaViewer] Error while reading snapshot: {ex.Message}");
            }

            Console.WriteLine("[FixSnaViewer] Snapshot loaded.");
            Snapshot.Print();
        }

        public static void DumpRawSnapshotData(IReadOnlyList<FixSnaBlock> blocks, uint snapshotPtr) {
            var block = blocks.FirstOrDefault(b => b.VirtualAddress == snapshotPtr);
            if (block == null) {
                Console.WriteLine($"[FixSnaViewer] Error: Block not found for VA=0x{snapshotPtr:X8}");
                return;
            }

            if (block.DecompressedData == null || block.DecompressedData.Length == 0) {
                Console.WriteLine($"[FixSnaViewer] Error: No decompressed data available.");
                return;
            }

            Console.WriteLine($"[FixSnaViewer] Raw Data Dump for VA=0x{snapshotPtr:X8} (Length={block.DecompressedData.Length})");
            for (int i = 0; i < block.DecompressedData.Length; i += 16) {
                var slice = block.DecompressedData.Skip(i).Take(16).ToArray();
                string hex = BitConverter.ToString(slice).Replace("-", " ");
                Console.WriteLine($"{snapshotPtr + (uint)i:X8}: {hex}");
            }
        }
    }
}
