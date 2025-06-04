public class AIModel {
    public uint offset_behaviors;
    public uint offset_reflexes;
    public uint offset_macros;
    public ushort behaviorCount;
    public ushort reflexCount;
    public ushort macroCount;

    // Optional: references to resolved data
    public Behavior[] Behaviors;
    public Reflex[] Reflexes;
    public Macro[] Macros;
}
