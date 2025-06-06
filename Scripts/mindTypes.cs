namespace HypeGame.Data
{
    public class Mind
    {
        public Intelligence intelligence;
    }

    public class Intelligence
    {
        public DsgMem dsgMem;
        public AIModel aiModel;
    }

    public class DsgMem
    {
        // Placeholder for dynamic state graph memory fields
        public int[] values;
    }

    public class AIModel
    {
        // Placeholder for AI behavior model reference
        public string name;
    }
}
