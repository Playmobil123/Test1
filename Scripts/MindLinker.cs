using System;

public static class MindLinker {
    /// <summary>
    /// Links the Mind and Intelligence by setting the dsgMem and aiModel fields.
    /// </summary>
    /// <param name="mind">The Mind instance to link.</param>
    /// <param name="aiModel">The AIModel instance to use for linking.</param>
    public static void LinkMindAndIntelligence(Mind mind, AIModel aiModel) {
        if (mind == null || aiModel == null) {
            Console.WriteLine("Error: Mind or AIModel is null.");
            return;
        }

        if (mind.intelligence == null) {
            Console.WriteLine("Error: Mind.intelligence is null.");
            return;
        }

        if (aiModel.off_dsgVar == 0) {
            Console.WriteLine("Error: aiModel.off_dsgVar is null or invalid.");
            return;
        }

        // Load DsgMem and link it to Mind.intelligence
        DsgMem dsgMem = DsgMemLoader.LoadDsgMem(aiModel.off_dsgVar);
        if (dsgMem == null) {
            Console.WriteLine("Error: Failed to load DsgMem.");
            return;
        }

        mind.intelligence.dsgMem = dsgMem;
        mind.intelligence.aiModel = aiModel;

        // Debug logging
        int varCount = dsgMem.variables.Length;
        Console.WriteLine($"DsgMem loaded with {varCount} vars.");

        if (varCount > 0) {
            DsgVarValue firstVar = dsgMem.variables[0];
            string typeStr = firstVar.type.ToString();
            string valueStr = firstVar.value != null ? firstVar.value.ToString() : "null";
            Console.WriteLine($"Example: [0] = {typeStr} = {valueStr}");
        }
    }
}