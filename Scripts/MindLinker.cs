using System;
using HypeGame.Data;
using HypeGame.Loader;

public static class MindLinker
{
    public static void LinkMindToPerso(PersoStruct perso, object dsgMem, object aiModel)
    {
        Console.WriteLine($"[MindLinker] Linking mind to Perso at 0x{perso.VirtualAddress:X8}");

        if (dsgMem != null)
            Console.WriteLine($"[MindLinker] DsgMem type: {dsgMem.GetType().Name}");
        else
            Console.WriteLine("[MindLinker] DsgMem is null");

        if (aiModel != null)
            Console.WriteLine($"[MindLinker] AIModel type: {aiModel.GetType().Name}");
        else
            Console.WriteLine("[MindLinker] AIModel is null");

        // Extend here if you want to store links in the perso or perform logic
    }
}
