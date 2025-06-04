using System;
using System.IO;

class TestMain {
    static void Main(string[] args) {
        try {
            // Boot sequence
            Console.WriteLine("Initializing...");
            string basePath = Path.Combine(AppContext.BaseDirectory, "Gamedata", "World", "Levels");

            GameDescriptorLoader.Load(Path.Combine(AppContext.BaseDirectory, "Gamedata", "gamedsc.bin"));
            FixCNTLoader.Load(Path.Combine(basePath, "fix.cnt"));
            CPAFixSnaLoader.LoadSnaMemoryBlocks(Path.Combine(basePath, "fix.sna"), 1);

            Console.WriteLine("Boot complete.");

            // Load menu.rp
            Console.WriteLine("\nLoading menu.rp...");
            var rp = RPFileLoader.LoadRP("menu.rp");
            var scene = SceneGraphLoader.ParseSceneGraph(rp.SceneBlock);

            // Get spawner list pointer (assume first field in SCNE)
            uint spawnerListPtr = BitConverter.ToUInt32(scene.SceneBlock.Data, 0x00);
            SpawnerListLoader.ParseSpawnerList(spawnerListPtr);

            Console.WriteLine("\nParsing spawners...");
            int index = 0;
            foreach (var spawner in SpawnerListLoader.SpawnerList) {
                Console.WriteLine($"\nSpawner {index++} @ VA=0x{spawner.VirtualAddress:X8}:");

                var perso = PersoLoader.ParsePersoStruct(spawner.TargetPointer);
                if (perso == null) {
                    Console.WriteLine("  → Perso not found or invalid");
                    continue;
                }

                var stdGame = StandardGameLoader.ParseStandardGameStruct(perso.off_stdGame);
                Console.WriteLine($"  → Name: {stdGame.FamilyIndex}/{stdGame.ModelIndex}/{stdGame.InstanceIndex}");

                var brain = BrainLoader.ParseBrainStruct(perso.off_brain);
                if (brain == null) {
                    Console.WriteLine("  → Brain not found.");
                    continue;
                }

                var mind = MindLoader.ParseMindStruct(brain.off_mind);
                if (mind == null) {
                    Console.WriteLine("  → Mind not found.");
                    continue;
                }

                var aiModel = AIModelLoader.ParseAIModelStruct(mind.off_AI_model);
                if (aiModel == null) {
                    Console.WriteLine("  → AIModel not found.");
                    continue;
                }

                MindLinker.LinkMindAndIntelligence(mind, aiModel);

                Console.WriteLine($"  → Behaviors: {aiModel.behaviorCount}, Reflexes: {aiModel.reflexCount}, Macros: {aiModel.macroCount}");
            }

        } catch (Exception ex) {
            Console.WriteLine("An error occurred:");
            Console.WriteLine(ex.Message);
            Console.WriteLine(ex.StackTrace);
        }

        Console.WriteLine("\nDone. Press any key to exit...");
        Console.ReadKey();
    }
}
