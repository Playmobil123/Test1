using System.Collections.Generic;

namespace HypeEngineClone
{
    public class SnaRelocationTable
    {
        private readonly Dictionary<(byte mod, byte id), int> offsetTable = new();

        public void Add(byte module, byte block, int offset)
        {
            offsetTable[(module, block)] = offset;
        }

        public int Remap(int pointer)
        {
            byte mod = (byte)((pointer >> 8) & 0xFF);
            byte id  = (byte)(pointer & 0xFF);

            if (offsetTable.TryGetValue((mod, id), out int delta))
                return pointer + delta;

            return pointer;
        }
    }
}
