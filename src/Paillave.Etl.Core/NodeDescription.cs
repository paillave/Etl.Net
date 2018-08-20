namespace Paillave.Etl
{
    public class NodeDescription
    {
        public NodeDescription(string name, bool isTarget, bool isSource)
        {
            this.Name = name;
            this.IsTarget = isTarget;
            this.IsSource = isSource;
        }
        public string Name { get; }
        public bool IsTarget { get; }
        public bool IsSource { get; }
    }
}
