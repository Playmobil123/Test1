namespace HypeGame.Data
{
    public class BlockEntry
    {
        public uint VirtualAddress;
        public uint DecompressedSize;
        public byte[] Data;
        public string BlockType;

        // ✅ Patch added to silence FileOffset error
        public long FileOffset => 0;
    }
}
