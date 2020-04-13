namespace Pathfinding
{
    public struct Progress
    {
        public readonly float progress;

        public readonly string description;

        public Progress(float p, string d)
        {
            progress = p;
            description = d;
        }

        public override string ToString()
        {
            return progress.ToString("0.0") + " " + description;
        }
    }
}
