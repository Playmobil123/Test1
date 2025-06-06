using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HypeGame.Data;

public static class FixCNTSceneIndexer {
    public static List<SceneDescriptor> SceneList = new();

    public static void PopulateSceneListFromFixCNT() {
        SceneList.Clear();

        string levelsPath = Path.Combine(AppContext.BaseDirectory, "Gamedata", "World", "Levels");

        foreach (var entry in FixCNTLoader.Entries) {
            if (entry == null || string.IsNullOrWhiteSpace(entry.FileName))
                continue;

            string fn = entry.FileName.ToLowerInvariant();

            if (fn.EndsWith(".lvl") || fn.EndsWith(".bin") || fn.EndsWith(".rul")) {
                string baseName = Path.GetFileNameWithoutExtension(fn);
                string cntPath = Path.Combine(levelsPath, baseName + ".cnt");
                string snaPath = Path.Combine(levelsPath, baseName + ".sna");

                bool hasCnt = File.Exists(cntPath);
                bool hasSna = File.Exists(snaPath);

                if (hasCnt && hasSna) {
                    SceneList.Add(new SceneDescriptor {
                        FileName = entry.FileName,
                        DisplayName = baseName
                    });

                    Console.WriteLine($"✔ Found scene: {baseName} (.cnt + .sna OK)");
                } else {
                    Console.WriteLine($"⚠ Skipped {baseName}: Missing {(hasCnt ? ".sna" : hasSna ? ".cnt" : ".cnt + .sna")}");
                }
            }
        }

        Console.WriteLine($"Total valid scenes found: {SceneList.Count}");
    }
}
