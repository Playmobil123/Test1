using System;
using System.Collections.Generic;
using HypeGame.Loader;

namespace HypeEngineClone
{
    public static class SnaParser
    {
        public static IReadOnlyList<FixSnaBlock> LoadedBlocks = new List<FixSnaBlock>();

        public static long GetBlockOffset(int index)
        {
            if (index < 0 || index >= LoadedBlocks.Count)
            {
                Console.WriteLine($"[SnaParser] Invalid block index: {index}");
                return 0;
            }

            return LoadedBlocks[index].FileOffset;
        }
    }
}
