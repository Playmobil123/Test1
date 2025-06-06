using HypeGame.AI;

public class AIModel {
    public AIModelStruct Struct { get; set; } // Reference to AIModelStruct
    public string Name { get; set; }          // For macro.cs
    public Behavior[] Behaviors;              // Existing fields
    public Reflex[] Reflexes;
    public Macro[] Macros;
}