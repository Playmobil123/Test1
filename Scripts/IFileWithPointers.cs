using HypeGame.Data;
using HypeGame.Loader;

namespace HypeGame.Files {
    public interface IFileWithPointers {
        long GetPhysicalOffset(uint virtualOffset);
    }

    public class FixSnaFile : IFileWithPointers {
        public long GetPhysicalOffset(uint virtualOffset) {
            foreach (var block in FixSNALoader.LoadedBlocks) {
                if (virtualOffset >= block.VirtualAddress &&
                    virtualOffset < block.VirtualAddress + block.DecompressedSize) {

                    return block.FileOffset + (virtualOffset - block.VirtualAddress);
                }
            }
            return -1;
        }
    }
}
