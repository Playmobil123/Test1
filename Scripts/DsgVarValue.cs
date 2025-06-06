public class DsgVarValue {
    public DsgVarType type;
    public object value;

    public void SetDefault(DsgVarType type) {
        this.type = type;
        value = DsgVarUtil.GetDefaultValue(type);
    }
}
