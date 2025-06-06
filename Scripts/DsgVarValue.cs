namespace HypeGame.AI {
public class DsgVarValue {
    public bool BoolValue;
    public byte ByteValue;
    public short ShortValue;
    public int IntValue;
    public float FloatValue;
    public float[] VectorValue; // usually length 3
    public object ObjectRef;

    public void SetDefault(DsgVarType type) {
        switch (type) {
            case DsgVarType.Boolean: BoolValue = false; break;
            case DsgVarType.Byte: ByteValue = 0; break;
            case DsgVarType.Short: ShortValue = 0; break;
            case DsgVarType.Int: IntValue = 0; break;
            case DsgVarType.Float: FloatValue = 0f; break;
            case DsgVarType.Vector: VectorValue = new float[3]; break;
            default: ObjectRef = null; break;
        }
    }
}

}