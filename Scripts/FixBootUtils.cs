using System;
using System.IO;
using HypeGame.Loader;

namespace HypeEngineClone {
    public static class FixBootUtils {
        /// <summary>
        /// Reads the g_FixSnaBootPointer from the first decompressed block of fix.sna.
        /// CPA logic skips 8 values, then reads the 9th int.
        /// </summary>
        public static uint GetSnapshotPointer(string snaPath) {
            FixSNALoader.LoadFixSnaFile(snaPath);

            var block = FixSNALoader.GetBlockDecompressed(0);
            if (block == null || block.Length < 40)
                throw new Exception("First Fix.sna block is too small or missing.");

            using var stream = new MemoryStream(block);
            using var reader = new BinaryReader(stream);

            for (int i = 0; i < 8; i++) reader.ReadInt32(); // Skip first 8 ints
            return reader.ReadUInt32(); // 9th = g_FixSnaBootPointer
        }
    }
}
