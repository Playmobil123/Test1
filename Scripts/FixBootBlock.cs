namespace HypeEngineClone
{
    public class FixBootBlock
    {
        public int Index { get; set; }
        public int CompressedSize { get; set; }
        public int DecompressedSize { get; set; }
        public int OffsetToData { get; set; }
        public byte[] CompressedData { get; set; }
    }
}
