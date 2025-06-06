using System;
using System.IO;

namespace HypeGame.Data {
    public class Pointer {
        public uint Offset { get; set; }
        public IFileWithPointers File { get; set; }

        public Pointer(uint offset, IFileWithPointers file) {
            Offset = offset;
            File = file;
        }

        public void DoAt(ref BinaryReader reader, Action action) {
            long originalPosition = reader.BaseStream.Position;
            long newPos = File.GetPhysicalOffset(Offset);

            if (newPos >= 0 && newPos < reader.BaseStream.Length) {
                reader.BaseStream.Seek(newPos, SeekOrigin.Begin);
                action();
                reader.BaseStream.Seek(originalPosition, SeekOrigin.Begin);
            } else {
                Console.WriteLine($"Pointer.DoAt: Invalid seek to 0x{newPos:X8} for offset 0x{Offset:X8}");
            }
        }

        public override string ToString() {
            return $"Pointer(Offset=0x{Offset:X8})";
        }
    }

    // Interface for any file that supports virtual-to-physical offset mapping
    public interface IFileWithPointers {
        long GetPhysicalOffset(uint virtualOffset);
    }
}
