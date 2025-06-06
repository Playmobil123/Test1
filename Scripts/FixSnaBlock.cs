using HypeGame.Data;

namespace HypeGame.Loader
{
    public class FixSnaBlock : BlockEntry
    {
        public int Index { get; set; }
        public int Offset { get; set; }

        public new uint FileOffset { get; set; }
        public uint VirtualAddress { get; set; }   // Used by memory parsers
        public byte Module { get; set; }           // Module ID (e.g., AI, Graph)
        public byte Id { get; set; }               // Local block ID within module

        public byte[] RawData { get; set; }        // Original compressed block data
        public byte[] DecompressedData { get; set; }  // Fully decoded

        // ✅ These fields are required by FixBootSnaLoader
        public uint CompressedSize { get; set; }
        public uint DecompressedSize { get; set; }
        public uint XorKey { get; set; }
        public int OffsetToData { get; set; }
        public byte[] CompressedData { get; set; }

        public FixSnaBlock()
        {
            RawData = Array.Empty<byte>();
            DecompressedData = Array.Empty<byte>();
            CompressedData = Array.Empty<byte>();
        }

        public override string ToString()
        {
            return $"[FixSnaBlock] Index={Index}, Offset=0x{Offset:X8}, VA=0x{VirtualAddress:X8}, Module={Module}, ID={Id}, Size={RawData?.Length ?? 0}";
        }
    }
}
