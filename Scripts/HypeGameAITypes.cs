using System.IO;

namespace HypeGame.AI {
    // Base class used for Behavior and Macro types
    public class BehaviorOrMacro {
    }

    // Represents a behavior in the AI model
    public class Behavior : BehaviorOrMacro {
    }

    // Represents a reflex in the AI model
    public class Reflex : BehaviorOrMacro {
    }



    // Represents a compiled script
    public class Script {
        public static Script Read(BinaryReader reader, uint offset, Macro macro, bool single) {
            return new Script(); // Placeholder logic
        }
    }

    // Game settings used to influence loading/parsing behavior
    public class GameSettings {
        public bool HasNames { get; set; }
        public Platform Platform { get; set; }
    }

    // Target platform enum (expand if needed)
    public enum Platform {
        PC
    }
}
