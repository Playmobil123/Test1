using System;
using HypeEngineClone;

public static class CPABootloader
{
    public static void BootFixSnaCore(string fixSnaPath)
    {
        Console.WriteLine($"[BOOT] Loading Fix.sna: {fixSnaPath}");

        using var reader = new SnaBinaryReader(fixSnaPath);

        Console.WriteLine("[BOOT] Reading patch + runtime header values...");

        int value1 = reader.ReadInt(); // DAT_00728704 + 0x2264
        int value2 = reader.ReadInt(); // DAT_0071aaa4
        int patchPtr = reader.ReadInt(); // CPA_g_pWritePatchTable

        Console.WriteLine($" - PatchTablePtr = 0x{patchPtr:X8}");

        int value4 = reader.ReadInt(); // DAT_00606960
        int value5 = reader.ReadInt(); // DAT_007f31f0
        byte[] buf4 = reader.ReadBytes(4); // DAT_00606964
        int value6 = reader.ReadInt();
        int value7 = reader.ReadInt();
        int value8 = reader.ReadInt();
        byte[] buf5 = reader.ReadBytes(0x90); // FadeScrollState

        Console.WriteLine("[BOOT] Reading matrix stack...");
        for (int i = 0; i < 256; i++) // total matrix stack count unknown, using 256 for now
        {
            int matrixVal = reader.ReadInt();
            // Store to fake runtime if needed
        }

        byte[] buf6 = reader.ReadBytes(4);   // HIE_g_lNbMatrixInStack
        byte[] buf7 = reader.ReadBytes(0xC); // DAT_00727f30
        byte[] buf8 = reader.ReadBytes(0x20); // DAT_0072aa00

        Console.WriteLine("[BOOT] Reading font + UI state...");
        byte[] buf9 = reader.ReadBytes(0x10); // DAT_00762990
        int fontCharCount = reader.ReadInt();
        int fontTexHandle = reader.ReadInt();
        int fontCount = reader.ReadInt();

        Console.WriteLine("[BOOT] FontCharCount = " + fontCharCount);

        int fadeTime = reader.ReadInt();
        int saveSlot = reader.ReadInt();

        Console.WriteLine($"[BOOT] FadeTime = {fadeTime}, SaveSlot = {saveSlot}");

        // You could interpret WritePatchTable now
        PatchTableInterpreter.Apply(reader, patchPtr);

        int fixSnaBootPtr = reader.ReadInt();
        Console.WriteLine($"[BOOT] FixSnaBootPtr = 0x{fixSnaBootPtr:X8}");

        Console.WriteLine("[BOOT] CPA Boot FixSna Core complete!");
    }
}
