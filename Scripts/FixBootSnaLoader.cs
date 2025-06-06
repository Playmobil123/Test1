using System;
using System.Collections.Generic;
using System.IO;
using HypeGame.Loader;

namespace HypeEngineClone
{
    public static class FixBootSnaLoader
    {
        /// <summary>
        /// Backup block loader that skips snapshot filtering.
        /// </summary>
        public static List<FixSnaBlock> LoadBootSnaBlocks(string snaFilePath, uint snapshotPtr)
        {
            return LoadBootSnaBlocks(snaFilePath); // ignores snapshotPtr
        }

        public static List<FixSnaBlock> LoadBootSnaBlocks(string snaFilePath)
        {
            var blocks = new List<FixSnaBlock>();

            if (!File.Exists(snaFilePath))
            {
                Console.WriteLine($"[FixBootSnaLoader] File not found: {snaFilePath}");
                return blocks;
            }

            byte[] buffer = File.ReadAllBytes(snaFilePath);
            if (buffer.Length < 4)
            {
                Console.WriteLine("[FixBootSnaLoader] File too small.");
                return blocks;
            }

            uint rawBlockCount = BitConverter.ToUInt32(buffer, 0);
            if (rawBlockCount == 0 || rawBlockCount > 10000)
            {
                Console.WriteLine($"[FixBootSnaLoader] Unrealistic block count: {rawBlockCount}");
                return blocks;
            }

            int offset = 4;
            for (int i = 0; i < rawBlockCount; i++)
            {
                if (offset + 12 > buffer.Length)
                {
                    Console.WriteLine($"[FixBootSnaLoader] Block {i} out of bounds at offset {offset}");
                    break;
                }

                uint virtualAddr = BitConverter.ToUInt32(buffer, offset);
                uint fileOffset = BitConverter.ToUInt32(buffer, offset + 4);
                uint size = BitConverter.ToUInt32(buffer, offset + 8);

                if (fileOffset + size > buffer.Length || size == 0)
                {
                    Console.WriteLine($"[FixBootSnaLoader] Block {i} has invalid size/offset: Offset={fileOffset}, Size={size}");
                    break;
                }

                blocks.Add(new FixSnaBlock
                {
                    VirtualAddress = virtualAddr,
                    FileOffset = fileOffset,
                    CompressedSize = size
                });

                offset += 12;
            }

            return blocks;
        }
    }
}
