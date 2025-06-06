using System;
using System.IO;

namespace HypeEngineClone
{
    public class SnaBinaryReader : IDisposable
    {
        private readonly FileStream stream;
        private readonly BinaryReader reader;

        public string FilePath { get; }

        public SnaRelocationTable Relocation { get; set; } = new();

        public SnaBinaryReader(string path)
        {
            FilePath = path;
            stream = new FileStream(path, FileMode.Open, FileAccess.Read);
            reader = new BinaryReader(stream);
        }

        public int ReadInt()
        {
            int value = reader.ReadInt32();
            return RemapPointer(value);
        }

        public uint ReadUInt()
        {
            uint value = reader.ReadUInt32();
            return (uint)RemapPointer((int)value);
        }

        public byte[] ReadBytes(int count)
        {
            return reader.ReadBytes(count);
        }

        public long Position => stream.Position;

        public void Seek(long offset, SeekOrigin origin = SeekOrigin.Begin)
        {
            stream.Seek(offset, origin);
        }

        public bool EndOfStream => stream.Position >= stream.Length;

        public void Dispose()
        {
            reader.Dispose();
            stream.Dispose();
        }

        /// <summary>
        /// Remaps a pointer using the loaded relocation table (simulating CPA pointer fixups).
        /// </summary>
        private int RemapPointer(int original)
        {
            return Relocation.Remap(original);
        }
    }
}
