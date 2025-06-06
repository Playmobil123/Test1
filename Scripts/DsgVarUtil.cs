public static class DsgVarUtil {
    public static object GetDefaultValue(DsgVarType type) {
        switch (type) {
            case DsgVarType.Boolean: return false;
            case DsgVarType.Byte:
            case DsgVarType.UByte: return (byte)0;
            case DsgVarType.Short: return (short)0;
            case DsgVarType.UShort: return (ushort)0;
            case DsgVarType.Int: return 0;
            case DsgVarType.UInt: return 0u;
            case DsgVarType.Float: return 0f;
            case DsgVarType.Vector: return new float[] { 0f, 0f, 0f };
            default: return null;
        }
    }
}
