using System;
using System.IO;
using System.Text;

public static class Utils {
    public static string ReadFixedString(BinaryReader reader, int length) {
        byte[] bytes = reader.ReadBytes(length);
        int nullIndex = Array.IndexOf(bytes, (byte)0);
        if (nullIndex >= 0) {
            return Encoding.ASCII.GetString(bytes, 0, nullIndex);
        } else {
            return Encoding.ASCII.GetString(bytes);
        }
    }
}