using Pathfinding.Util;
using System;

namespace Pathfinding
{
    internal class PathReturnQueue
    {
        private LockFreeStack pathReturnStack = new LockFreeStack();

        private Path pathReturnPop;

        private object pathsClaimedSilentlyBy;

        public PathReturnQueue(object pathsClaimedSilentlyBy)
        {
            this.pathsClaimedSilentlyBy = pathsClaimedSilentlyBy;
        }

        public void Enqueue(Path path)
        {
            pathReturnStack.Push(path);
        }

        public void ReturnPaths(bool timeSlice)
        {
            Path next = pathReturnStack.PopAll();
            if (pathReturnPop == null)
            {
                pathReturnPop = next;
            }
            else
            {
                Path next2 = pathReturnPop;
                while (next2.next != null)
                {
                    next2 = next2.next;
                }
                next2.next = next;
            }
            long num = (!timeSlice) ? 0 : (DateTime.UtcNow.Ticks + 10000);
            int num2 = 0;
            while (pathReturnPop != null)
            {
                Path path = pathReturnPop;
                pathReturnPop = pathReturnPop.next;
                path.next = null;
                path.ReturnPath();
                path.AdvanceState(PathState.Returned);
                path.Release(pathsClaimedSilentlyBy, silent: true);
                num2++;
                if (num2 > 5 && timeSlice)
                {
                    num2 = 0;
                    if (DateTime.UtcNow.Ticks >= num)
                    {
                        break;
                    }
                }
            }
        }
    }
}
