using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HypeGame.Data;
using HypeGame.Loader;
using HypeEngineClone;

public static class Bootloader
{
    public static IReadOnlyList<FixSnaBlock> InitializeFixBlocks(string fixCntPath, string fixSnaPath, uint snapshotPtr)
    {
        Console.WriteLine("Initializing...");

        FixCNTLoader.Load(fixCntPath);
        var entries = FixCNTLoader.Entries;
        Console.WriteLine($"fix.cnt: numEntries={entries.Count}");

        foreach (var entry in entries)
            Console.WriteLine($"Entry: {entry.FileName} (Block {entry.BlockIndex})");

        Console.WriteLine("fix.cnt loading complete.");
        Console.WriteLine("[FixSnaLoader] Loading SNA file...");

        FixSNALoader.LoadFixSnaFile(fixSnaPath);
        var blocks = FixSNALoader.LoadedBlocks;

        if (blocks.Count == 0)
        {
            Console.WriteLine("[FixSnaLoader] Falling back to FixBootSnaLoader...");
            blocks = FixBootSnaLoader.LoadBootSnaBlocks(fixSnaPath, snapshotPtr);
        }

        return blocks;
    }

    public static void BootFixSnaCore(string baseGameDataPath)
    {
        Console.WriteLine("🌀 Booting Fix.sna core...");

        string fixSna = Path.Combine(baseGameDataPath, "Fix.sna");
        string fixGpt = Path.Combine(baseGameDataPath, "Fix.gpt");
        string fixDlg = Path.Combine(baseGameDataPath, "Fix.dlg");
        string fixSda = Path.Combine(baseGameDataPath, "Fix.sda");
        string fixPtx = Path.Combine(baseGameDataPath, "Fix.ptx");

        FixMemory.OpenMemoryBlockFile(fixGpt);
        FixMemory.OpenMemoryBlockFile_DLG(fixDlg);
        FixMemory.OpenMemoryBlockFile_SDA(fixSda);

        FixSNALoader.LoadFixSnaFile(fixSna); // Just load file, no LoadSnaMemoryBlocks (remove invalid call)

        TextureManager.SetFixTexture(fixPtx);

        var snapshot = new FixSnaStateSnapshot();
        var block = FixSNALoader.LoadedBlocks.FirstOrDefault();

        if (block == null || block.DecompressedData == null || block.DecompressedData.Length < 40)
        {
            Console.WriteLine("[FixSnaViewer] Error: No valid snapshot block found.");
            return;
        }

        using var ms = new MemoryStream(block.DecompressedData);
        using var reader = new BinaryReader(ms);

        try
        {
            snapshot.FontCharCount = reader.ReadInt32();
            snapshot.CameraZDepth = reader.ReadInt32();
            snapshot.ScreenFadeTime = reader.ReadInt32();
            snapshot.SaveSlotIndex = reader.ReadInt32();
            snapshot.DialogSpeed = reader.ReadInt32();
            snapshot.GlobalRuntimeHash = reader.ReadInt32();
            snapshot.GaugeState = reader.ReadInt32();
            snapshot.InventoryStatus = reader.ReadInt32();
            snapshot.MatrixStackPointer = reader.ReadInt32();
            snapshot.StartupFlags = reader.ReadInt32();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[FixSnaViewer] Error while reading snapshot: {ex.Message}");
            return;
        }

        Console.WriteLine("[FixSnaViewer] Snapshot loaded.");
        snapshot.Print();

        Console.WriteLine("✅ Fix.sna boot complete.");
    }
}

public class FixSnaStateSnapshot
{
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

    public void Print()
    {
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

public static class FixMemory
{
    public static void OpenMemoryBlockFile(string path) => Console.WriteLine($"[FixMemory] Opened {path}");
    public static void OpenMemoryBlockFile_DLG(string path) => Console.WriteLine($"[FixMemory] Opened DLG {path}");
    public static void OpenMemoryBlockFile_SDA(string path) => Console.WriteLine($"[FixMemory] Opened SDA {path}");
}

public static class TextureManager
{
    public static void SetFixTexture(string path) => Console.WriteLine($"[TextureManager] Set texture binary: {path}");
}

public class FixSnaStateViewer
{
    public void ShowSnapshot(IReadOnlyList<FixSnaBlock> blocks, uint snapshotPtr)
    {
        Console.WriteLine($"[FixSnaStateViewer] Showing snapshot from pointer 0x{snapshotPtr:X8}...");
        Console.WriteLine($"[FixSnaStateViewer] Total blocks: {blocks.Count}");
        // Add whatever preview logic you want here
    }
}

class Program
{
    static void Main(string[] args)
    {
        string fixCntPath = "Gamedata/World/Levels/fix.cnt";
        string fixSnaPath = "Gamedata/World/Levels/fix.sna";
        uint snapshotPtr = 0x0;

        try
        {
            var blocks = Bootloader.InitializeFixBlocks(fixCntPath, fixSnaPath, snapshotPtr);

            if (blocks.Count == 0)
            {
                Console.WriteLine("An error occurred: No valid blocks were loaded.");
            }
            else
            {
                Console.WriteLine($"Successfully loaded {blocks.Count} blocks.");
                Console.WriteLine("Now parsing scene snapshot...");

                var viewer = new FixSnaStateViewer();
                viewer.ShowSnapshot(blocks, snapshotPtr);
            }

            Bootloader.BootFixSnaCore("Gamedata/World/Levels");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }

        Console.WriteLine("Done. Press any key to exit...");
        Console.ReadKey();
    }
}
