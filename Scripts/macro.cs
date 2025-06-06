using System;
using System.Text;

namespace HypeGame.AI {
    public class Macro : BehaviorOrMacro {
        public string Name;
        public uint OffsetScript;
        public uint OffsetScript2;
        public Script Script;

        // Custom
        public int Index;
        public AIModel AIModel;

        public string ShortName {
            get {
                return GetShortName(AIModel, Index);
            }
        }

 public static class Utils {
    public static string ReadFixedString(BinaryReader reader, int length) {
        byte[] bytes = reader.ReadBytes(length);
        int nullIndex = Array.IndexOf(bytes, (byte)0);
        if (nullIndex >= 0) {
            return Encoding.ASCII.GetString(bytes, 0, nullIndex);
        } else {
            return Encoding.ASCII.GetString(bytes);
        }
    }
}

       public string GetShortName(AIModel model, int index) {
            string shortName = "";

            if (!string.IsNullOrEmpty(Name)) {
                shortName = Name;
                if (shortName.Contains("^CreateMacro:")) {
                    shortName = shortName.Substring(shortName.LastIndexOf("^CreateMacro:") + 13);
                }
                shortName = "[\"" + shortName + "\"]";
            }

            shortName = model.Name + ".Macro[" + index + "]" + shortName;
            return shortName;
        }

        public string NameSubstring {
            get {
                if (!string.IsNullOrEmpty(Name)) {
                    var shortName = Name;
                    if (shortName.Contains("^CreateMacro:")) {
                        shortName = shortName.Substring(shortName.LastIndexOf("^CreateMacro:") + 13);
                    }
                    return shortName;
                } else {
                    return "Macro_" + Index;
                }
            }
        }

        public void Read(BinaryReader reader, GameSettings settings) {
            if (settings.HasNames && settings.Platform == Platform.PC) {
                Name = Utils.ReadFixedString(reader, 0x100);

                int indexOf = Name.IndexOf("CreateMacro:", StringComparison.Ordinal);
                if (indexOf >= 0) {
                    Name = Name.Substring(indexOf + "CreateMacro:".Length);
                }
            }

            OffsetScript = reader.ReadUInt32();
            OffsetScript2 = reader.ReadUInt32();

            if (OffsetScript != 0) {
                long prevPos = reader.BaseStream.Position;
                reader.BaseStream.Seek(OffsetScript, SeekOrigin.Begin);
                Script = Script.Read(reader, OffsetScript, this, single: true);
                reader.BaseStream.Seek(prevPos, SeekOrigin.Begin);
            }
        }
    }
}
