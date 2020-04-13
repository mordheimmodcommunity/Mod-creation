using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Pathfinding
{
    internal class PathProcessor
    {
        public readonly ThreadControlQueue queue;

        private readonly AstarPath astar;

        private readonly PathReturnQueue returnQueue;

        private readonly PathThreadInfo[] threadInfos;

        private readonly Thread[] threads;

        private IEnumerator threadCoroutine;

        private int nextNodeIndex = 1;

        private readonly Stack<int> nodeIndexPool = new Stack<int>();

        public int NumThreads => threadInfos.Length;

        public bool IsUsingMultithreading => threads != null;

        public event Action<Path> OnPathPreSearch;

        public event Action<Path> OnPathPostSearch;

        public PathProcessor(AstarPath astar, PathReturnQueue returnQueue, int processors, bool multithreaded)
        {
            this.astar = astar;
            this.returnQueue = returnQueue;
            if (processors < 0)
            {
                throw new ArgumentOutOfRangeException("processors");
            }
            if (!multithreaded && processors != 1)
            {
                throw new Exception("Only a single non-multithreaded processor is allowed");
            }
            queue = new ThreadControlQueue(processors);
            threadInfos = new PathThreadInfo[processors];
            for (int i = 0; i < processors; i++)
            {
                threadInfos[i] = new PathThreadInfo(i, astar, new PathHandler(i, processors));
            }
            if (multithreaded)
            {
                threads = new Thread[processors];
                for (int j = 0; j < processors; j++)
                {
                    int threadIndex = j;
                    Thread thread = new Thread((ThreadStart)delegate
                    {
                        CalculatePathsThreaded(threadInfos[threadIndex]);
                    })
                    {
                        Name = "Pathfinding Thread " + j,
                        IsBackground = true
                    };
                    threads[j] = thread;
                    thread.Start();
                }
            }
            else
            {
                threadCoroutine = CalculatePaths(threadInfos[0]);
            }
        }

        public void BlockUntilPathQueueBlocked()
        {
            queue.Block();
            if (!Application.isPlaying)
            {
                return;
            }
            while (!queue.AllReceiversBlocked)
            {
                if (IsUsingMultithreading)
                {
                    Thread.Sleep(1);
                }
                else
                {
                    TickNonMultithreaded();
                }
            }
        }

        public void TickNonMultithreaded()
        {
            if (threadCoroutine != null)
            {
                try
                {
                    threadCoroutine.MoveNext();
                }
                catch (Exception ex)
                {
                    threadCoroutine = null;
                    if (!(ex is ThreadControlQueue.QueueTerminationException))
                    {
                        Debug.LogException(ex);
                        Debug.LogError("Unhandled exception during pathfinding. Terminating.");
                        queue.TerminateReceivers();
                        try
                        {
                            queue.PopNoBlock(blockedBefore: false);
                        }
                        catch
                        {
                        }
                    }
                }
            }
        }

        public void JoinThreads()
        {
            if (threads == null)
            {
                return;
            }
            for (int i = 0; i < threads.Length; i++)
            {
                if (!threads[i].Join(50))
                {
                    Debug.LogError("Could not terminate pathfinding thread[" + i + "] in 50ms, trying Thread.Abort");
                    threads[i].Abort();
                }
            }
        }

        public void AbortThreads()
        {
            if (threads == null)
            {
                return;
            }
            for (int i = 0; i < threads.Length; i++)
            {
                if (threads[i] != null && threads[i].IsAlive)
                {
                    threads[i].Abort();
                }
            }
        }

        public int GetNewNodeIndex()
        {
            return (nodeIndexPool.Count <= 0) ? nextNodeIndex++ : nodeIndexPool.Pop();
        }

        public void InitializeNode(GraphNode node)
        {
            if (!queue.AllReceiversBlocked)
            {
                throw new Exception("Trying to initialize a node when it is not safe to initialize any nodes. Must be done during a graph update. See http://arongranberg.com/astar/docs/graph-updates.php#direct");
            }
            for (int i = 0; i < threadInfos.Length; i++)
            {
                threadInfos[i].runData.InitializeNode(node);
            }
        }

        public void DestroyNode(GraphNode node)
        {
            if (node.NodeIndex != -1)
            {
                nodeIndexPool.Push(node.NodeIndex);
                for (int i = 0; i < threadInfos.Length; i++)
                {
                    threadInfos[i].runData.DestroyNode(node);
                }
            }
        }

        private void CalculatePathsThreaded(PathThreadInfo threadInfo)
        {
            //Discarded unreachable code: IL_01d5
            try
            {
                PathHandler runData = threadInfo.runData;
                if (runData.nodes == null)
                {
                    throw new NullReferenceException("NodeRuns must be assigned to the threadInfo.runData.nodes field before threads are started\nthreadInfo is an argument to the thread functions");
                }
                long num = (long)(astar.maxFrameTime * 10000f);
                long num2 = DateTime.UtcNow.Ticks + num;
                while (true)
                {
                    Path path = queue.Pop();
                    num = (long)(astar.maxFrameTime * 10000f);
                    path.PrepareBase(runData);
                    path.AdvanceState(PathState.Processing);
                    if (this.OnPathPreSearch != null)
                    {
                        this.OnPathPreSearch(path);
                    }
                    long ticks = DateTime.UtcNow.Ticks;
                    long num3 = 0L;
                    path.Prepare();
                    if (!path.IsDone())
                    {
                        astar.debugPath = path;
                        path.Initialize();
                        while (!path.IsDone())
                        {
                            path.CalculateStep(num2);
                            path.searchIterations++;
                            if (path.IsDone())
                            {
                                break;
                            }
                            num3 += DateTime.UtcNow.Ticks - ticks;
                            Thread.Sleep(0);
                            ticks = DateTime.UtcNow.Ticks;
                            num2 = ticks + num;
                            if (queue.IsTerminating)
                            {
                                path.Error();
                            }
                        }
                        num3 += DateTime.UtcNow.Ticks - ticks;
                        path.duration = (float)num3 * 0.0001f;
                    }
                    path.Cleanup();
                    if (path.immediateCallback != null)
                    {
                        path.immediateCallback(path);
                    }
                    if (this.OnPathPostSearch != null)
                    {
                        this.OnPathPostSearch(path);
                    }
                    returnQueue.Enqueue(path);
                    path.AdvanceState(PathState.ReturnQueue);
                    if (DateTime.UtcNow.Ticks > num2)
                    {
                        Thread.Sleep(1);
                        num2 = DateTime.UtcNow.Ticks + num;
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex is ThreadAbortException || ex is ThreadControlQueue.QueueTerminationException)
                {
                    if (astar.logPathResults == PathLog.Heavy)
                    {
                        Debug.LogWarning("Shutting down pathfinding thread #" + threadInfo.threadIndex);
                    }
                    return;
                }
                Debug.LogException(ex);
                Debug.LogError("Unhandled exception during pathfinding. Terminating.");
                queue.TerminateReceivers();
            }
            Debug.LogError("Error : This part should never be reached.");
            queue.ReceiverTerminated();
        }

        private IEnumerator CalculatePaths(PathThreadInfo threadInfo)
        {
            int numPaths = 0;
            PathHandler runData = threadInfo.runData;
            if (runData.nodes == null)
            {
                throw new NullReferenceException("NodeRuns must be assigned to the threadInfo.runData.nodes field before threads are started\nthreadInfo is an argument to the thread functions");
            }
            long maxTicks2 = (long)(astar.maxFrameTime * 10000f);
            long targetTick = DateTime.UtcNow.Ticks + maxTicks2;
            while (true)
            {
                Path p = null;
                bool blockedBefore = false;
                while (p == null)
                {
                    try
                    {
                        p = queue.PopNoBlock(blockedBefore);
                        blockedBefore |= (p == null);
                    }
                    catch (ThreadControlQueue.QueueTerminationException)
                    {
                        yield break;
                    }
                    if (p == null)
                    {
                        yield return null;
                    }
                }
                maxTicks2 = (long)(astar.maxFrameTime * 10000f);
                p.PrepareBase(runData);
                p.AdvanceState(PathState.Processing);
                this.OnPathPreSearch?.Invoke(p);
                numPaths++;
                long startTicks = DateTime.UtcNow.Ticks;
                long totalTicks2 = 0L;
                p.Prepare();
                if (!p.IsDone())
                {
                    astar.debugPath = p;
                    p.Initialize();
                    while (!p.IsDone())
                    {
                        p.CalculateStep(targetTick);
                        p.searchIterations++;
                        if (p.IsDone())
                        {
                            break;
                        }
                        totalTicks2 += DateTime.UtcNow.Ticks - startTicks;
                        yield return null;
                        startTicks = DateTime.UtcNow.Ticks;
                        if (queue.IsTerminating)
                        {
                            p.Error();
                        }
                        targetTick = DateTime.UtcNow.Ticks + maxTicks2;
                    }
                    totalTicks2 += DateTime.UtcNow.Ticks - startTicks;
                    p.duration = (float)totalTicks2 * 0.0001f;
                }
                p.Cleanup();
                p.immediateCallback?.Invoke(p);
                this.OnPathPostSearch?.Invoke(p);
                returnQueue.Enqueue(p);
                p.AdvanceState(PathState.ReturnQueue);
                if (DateTime.UtcNow.Ticks > targetTick)
                {
                    yield return null;
                    targetTick = DateTime.UtcNow.Ticks + maxTicks2;
                    numPaths = 0;
                }
            }
        }
    }
}
