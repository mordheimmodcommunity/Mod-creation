namespace Pathfinding
{
    public struct PathThreadInfo
    {
        public readonly int threadIndex;

        public readonly AstarPath astar;

        public readonly PathHandler runData;

        public PathThreadInfo(int index, AstarPath astar, PathHandler runData)
        {
            threadIndex = index;
            this.astar = astar;
            this.runData = runData;
        }
    }
}
