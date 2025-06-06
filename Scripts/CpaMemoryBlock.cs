namespace HypeEngineClone
{
    public class CpaMemoryBlock
    {
        public byte Module { get; set; }
        public byte ID { get; set; }

        public uint VirtualAddress { get; set; }

        public uint CompressedSize { get; set; }
        public uint DecompressedSize { get; set; }

        public byte[] CompressedData { get; set; }
        public byte[] DecompressedData { get; set; }  // optional for after decompression
    }
}
