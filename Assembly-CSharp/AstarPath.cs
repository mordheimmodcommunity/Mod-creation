using Pathfinding;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using UnityEngine;

[HelpURL("http://arongranberg.com/astar/docs/class_astar_path.php")]
[AddComponentMenu("Pathfinding/Pathfinder")]
[ExecuteInEditMode]
public class AstarPath : MonoBehaviour
{
    public enum AstarDistribution
    {
        WebsiteDownload,
        AssetStore
    }

    public static readonly AstarDistribution Distribution = AstarDistribution.WebsiteDownload;

    public static readonly string Branch = "rvo_fix_Pro";

    public AstarData astarData;

    public static AstarPath active;

    public bool showNavGraphs = true;

    public bool showUnwalkableNodes = true;

    public GraphDebugMode debugMode;

    public float debugFloor;

    public float debugRoof = 20000f;

    public bool manualDebugFloorRoof;

    public bool showSearchTree;

    public float unwalkableNodeDebugSize = 0.3f;

    public PathLog logPathResults = PathLog.Normal;

    public float maxNearestNodeDistance = 100f;

    public bool scanOnStartup = true;

    public bool fullGetNearestSearch;

    public bool prioritizeGraphs;

    public float prioritizeGraphsLimit = 1f;

    public AstarColor colorSettings;

    [SerializeField]
    protected string[] tagNames;

    public Heuristic heuristic = Heuristic.Euclidean;

    public float heuristicScale = 1f;

    public ThreadCount threadCount;

    public float maxFrameTime = 1f;

    [Obsolete("Minimum area size is mostly obsolete since the limit has been raised significantly, and the edge cases are handled automatically")]
    public int minAreaSize;

    public bool batchGraphUpdates;

    public float graphUpdateBatchingInterval = 0.2f;

    [NonSerialized]
    public Path debugPath;

    private string inGameDebugPath;

    public static Action OnAwakeSettings;

    public static OnGraphDelegate OnGraphPreScan;

    public static OnGraphDelegate OnGraphPostScan;

    public static OnPathDelegate OnPathPreSearch;

    public static OnPathDelegate OnPathPostSearch;

    public static OnScanDelegate OnPreScan;

    public static OnScanDelegate OnPostScan;

    public static OnScanDelegate OnLatePostScan;

    public static OnScanDelegate OnGraphsUpdated;

    public static Action On65KOverflow;

    private static Action OnThreadSafeCallback;

    public Action OnDrawGizmosCallback;

    public Action OnUnloadGizmoMeshes;

    [Obsolete]
    public Action OnGraphsWillBeUpdated;

    [Obsolete]
    public Action OnGraphsWillBeUpdated2;

    private readonly GraphUpdateProcessor graphUpdates;

    private readonly WorkItemProcessor workItems;

    private PathProcessor pathProcessor;

    private bool graphUpdateRoutineRunning;

    private bool graphUpdatesWorkItemAdded;

    private float lastGraphUpdate = -9999f;

    private bool workItemsQueued;

    private readonly PathReturnQueue pathReturnQueue;

    public EuclideanEmbedding euclideanEmbedding = new EuclideanEmbedding();

    public bool showGraphs;

    private static readonly object safeUpdateLock = new object();

    private ushort nextFreePathID = 1;

    private static int waitForPathDepth = 0;

    public static Version Version => new Version(3, 8, 5);

    [Obsolete]
    public Type[] graphTypes => astarData.graphTypes;

    public NavGraph[] graphs
    {
        get
        {
            if (astarData == null)
            {
                astarData = new AstarData();
            }
            return astarData.graphs;
        }
        set
        {
            if (astarData == null)
            {
                astarData = new AstarData();
            }
            astarData.graphs = value;
        }
    }

    public float maxNearestNodeDistanceSqr => maxNearestNodeDistance * maxNearestNodeDistance;

    [Obsolete("This field has been renamed to 'batchGraphUpdates'")]
    public bool limitGraphUpdates
    {
        get
        {
            return batchGraphUpdates;
        }
        set
        {
            batchGraphUpdates = value;
        }
    }

    [Obsolete("This field has been renamed to 'graphUpdateBatchingInterval'")]
    public float maxGraphUpdateFreq
    {
        get
        {
            return graphUpdateBatchingInterval;
        }
        set
        {
            graphUpdateBatchingInterval = value;
        }
    }

    public float lastScanTime
    {
        get;
        private set;
    }

    public PathHandler debugPathData
    {
        get
        {
            if (debugPath == null)
            {
                return null;
            }
            return debugPath.pathHandler;
        }
    }

    public bool isScanning
    {
        get;
        private set;
    }

    public int NumParallelThreads => pathProcessor.NumThreads;

    public bool IsUsingMultithreading => pathProcessor.IsUsingMultithreading;

    [Obsolete("Fixed grammar, use IsAnyGraphUpdateQueued instead")]
    public bool IsAnyGraphUpdatesQueued => IsAnyGraphUpdateQueued;

    public bool IsAnyGraphUpdateQueued => graphUpdates.IsAnyGraphUpdateQueued;

    public bool IsAnyGraphUpdateInProgress => graphUpdates.IsAnyGraphUpdateInProgress;

    public bool IsAnyWorkItemInProgress => workItems.workItemsInProgress;

    private AstarPath()
    {
        pathProcessor = new PathProcessor(this, pathReturnQueue, 0, multithreaded: true);
        pathReturnQueue = new PathReturnQueue(this);
        workItems = new WorkItemProcessor(this);
        graphUpdates = new GraphUpdateProcessor(this);
        graphUpdates.OnGraphsUpdated += delegate
        {
            if (OnGraphsUpdated != null)
            {
                OnGraphsUpdated(this);
            }
        };
    }

    public string[] GetTagNames()
    {
        if (tagNames == null || tagNames.Length != 32)
        {
            tagNames = new string[32];
            for (int i = 0; i < tagNames.Length; i++)
            {
                tagNames[i] = string.Empty + i;
            }
            tagNames[0] = "Basic Ground";
        }
        return tagNames;
    }

    public static string[] FindTagNames()
    {
        if (active != null)
        {
            return active.GetTagNames();
        }
        AstarPath astarPath = UnityEngine.Object.FindObjectOfType<AstarPath>();
        if (astarPath != null)
        {
            active = astarPath;
            return astarPath.GetTagNames();
        }
        return new string[1]
        {
            "There is no AstarPath component in the scene"
        };
    }

    internal ushort GetNextPathID()
    {
        if (nextFreePathID == 0)
        {
            nextFreePathID++;
            UnityEngine.Debug.Log("65K cleanup (this message is harmless, it just means you have searched a lot of paths)");
            if (On65KOverflow != null)
            {
                Action on65KOverflow = On65KOverflow;
                On65KOverflow = null;
                on65KOverflow();
            }
        }
        return nextFreePathID++;
    }

    private void RecalculateDebugLimits()
    {
        debugFloor = float.PositiveInfinity;
        debugRoof = float.NegativeInfinity;
        for (int i = 0; i < graphs.Length; i++)
        {
            if (graphs[i] != null && graphs[i].drawGizmos)
            {
                graphs[i].GetNodes(delegate (GraphNode node)
                {
                    if (!showSearchTree || debugPathData == null || NavGraph.InSearchTree(node, debugPath))
                    {
                        PathNode pathNode = (debugPathData == null) ? null : debugPathData.GetPathNode(node);
                        if (pathNode != null || debugMode == GraphDebugMode.Penalty)
                        {
                            switch (debugMode)
                            {
                                case GraphDebugMode.F:
                                    debugFloor = Mathf.Min(debugFloor, (float)(double)pathNode.F);
                                    debugRoof = Mathf.Max(debugRoof, (float)(double)pathNode.F);
                                    break;
                                case GraphDebugMode.G:
                                    debugFloor = Mathf.Min(debugFloor, (float)(double)pathNode.G);
                                    debugRoof = Mathf.Max(debugRoof, (float)(double)pathNode.G);
                                    break;
                                case GraphDebugMode.H:
                                    debugFloor = Mathf.Min(debugFloor, (float)(double)pathNode.H);
                                    debugRoof = Mathf.Max(debugRoof, (float)(double)pathNode.H);
                                    break;
                                case GraphDebugMode.Penalty:
                                    debugFloor = Mathf.Min(debugFloor, (float)(double)node.Penalty);
                                    debugRoof = Mathf.Max(debugRoof, (float)(double)node.Penalty);
                                    break;
                            }
                        }
                    }
                    return true;
                });
            }
        }
        if (float.IsInfinity(debugFloor))
        {
            debugFloor = 0f;
            debugRoof = 1f;
        }
        if (debugRoof - debugFloor < 1f)
        {
            debugRoof += 1f;
        }
    }

    private void OnDrawGizmos()
    {
        if (isScanning)
        {
            return;
        }
        if (active == null)
        {
            active = this;
        }
        else if (active != this)
        {
            return;
        }
        if (graphs == null || workItems.workItemsInProgress)
        {
            return;
        }
        if (showNavGraphs && !manualDebugFloorRoof)
        {
            RecalculateDebugLimits();
        }
        for (int i = 0; i < graphs.Length; i++)
        {
            if (graphs[i] != null && graphs[i].drawGizmos)
            {
                graphs[i].OnDrawGizmos(showNavGraphs);
            }
        }
        if (showNavGraphs)
        {
            euclideanEmbedding.OnDrawGizmos();
            if (showUnwalkableNodes)
            {
                Gizmos.color = AstarColor.UnwalkableNode;
                GraphNodeDelegateCancelable del = DrawUnwalkableNode;
                for (int j = 0; j < graphs.Length; j++)
                {
                    if (graphs[j] != null && graphs[j].drawGizmos)
                    {
                        graphs[j].GetNodes(del);
                    }
                }
            }
        }
        if (OnDrawGizmosCallback != null)
        {
            OnDrawGizmosCallback();
        }
    }

    private bool DrawUnwalkableNode(GraphNode node)
    {
        if (!node.Walkable)
        {
            Gizmos.DrawCube((Vector3)node.position, Vector3.one * unwalkableNodeDebugSize);
        }
        return true;
    }

    private void OnGUI()
    {
        if (logPathResults == PathLog.InGame && inGameDebugPath != string.Empty)
        {
            GUI.Label(new Rect(5f, 5f, 400f, 600f), inGameDebugPath);
        }
    }

    internal void Log(string s)
    {
        if (object.ReferenceEquals(active, null))
        {
            UnityEngine.Debug.Log("No AstarPath object was found : " + s);
        }
        else if (active.logPathResults != 0 && active.logPathResults != PathLog.OnlyErrors)
        {
            UnityEngine.Debug.Log(s);
        }
    }

    private void LogPathResults(Path p)
    {
        if (logPathResults != 0 && (logPathResults != PathLog.OnlyErrors || p.error))
        {
            string message = p.DebugString(logPathResults);
            if (logPathResults == PathLog.InGame)
            {
                inGameDebugPath = message;
            }
            else
            {
                UnityEngine.Debug.Log(message);
            }
        }
    }

    private void Update()
    {
        if (Application.isPlaying)
        {
            if (!isScanning)
            {
                PerformBlockingActions();
            }
            pathProcessor.TickNonMultithreaded();
            pathReturnQueue.ReturnPaths(timeSlice: true);
        }
    }

    private void PerformBlockingActions(bool force = false, bool unblockOnComplete = true)
    {
        if (!pathProcessor.queue.AllReceiversBlocked)
        {
            return;
        }
        pathReturnQueue.ReturnPaths(timeSlice: false);
        if (OnThreadSafeCallback != null)
        {
            Action onThreadSafeCallback = OnThreadSafeCallback;
            OnThreadSafeCallback = null;
            onThreadSafeCallback();
        }
        if (!pathProcessor.queue.AllReceiversBlocked || !workItems.ProcessWorkItems(force))
        {
            return;
        }
        workItemsQueued = false;
        if (unblockOnComplete)
        {
            if (euclideanEmbedding.dirty)
            {
                euclideanEmbedding.RecalculateCosts();
            }
            pathProcessor.queue.Unblock();
        }
    }

    [Obsolete("This method has been moved. Use the method on the context object that can be sent with work item delegates instead")]
    public void QueueWorkItemFloodFill()
    {
        throw new Exception("This method has been moved. Use the method on the context object that can be sent with work item delegates instead");
    }

    [Obsolete("This method has been moved. Use the method on the context object that can be sent with work item delegates instead")]
    public void EnsureValidFloodFill()
    {
        throw new Exception("This method has been moved. Use the method on the context object that can be sent with work item delegates instead");
    }

    public void AddWorkItem(AstarWorkItem itm)
    {
        workItems.AddWorkItem(itm);
        if (!workItemsQueued)
        {
            workItemsQueued = true;
            if (!isScanning)
            {
                InterruptPathfinding();
            }
        }
    }

    public void QueueGraphUpdates()
    {
        if (!graphUpdatesWorkItemAdded)
        {
            graphUpdatesWorkItemAdded = true;
            AstarWorkItem workItem = graphUpdates.GetWorkItem();
            AddWorkItem(new AstarWorkItem(delegate
            {
                graphUpdatesWorkItemAdded = false;
                lastGraphUpdate = Time.realtimeSinceStartup;
                debugPath = null;
                workItem.init();
            }, workItem.update));
        }
    }

    private IEnumerator DelayedGraphUpdate()
    {
        graphUpdateRoutineRunning = true;
        yield return new WaitForSeconds(graphUpdateBatchingInterval - (Time.realtimeSinceStartup - lastGraphUpdate));
        QueueGraphUpdates();
        graphUpdateRoutineRunning = false;
    }

    public void UpdateGraphs(Bounds bounds, float t)
    {
        UpdateGraphs(new GraphUpdateObject(bounds), t);
    }

    public void UpdateGraphs(GraphUpdateObject ob, float t)
    {
        StartCoroutine(UpdateGraphsInteral(ob, t));
    }

    private IEnumerator UpdateGraphsInteral(GraphUpdateObject ob, float t)
    {
        yield return new WaitForSeconds(t);
        UpdateGraphs(ob);
    }

    public void UpdateGraphs(Bounds bounds)
    {
        UpdateGraphs(new GraphUpdateObject(bounds));
    }

    public void UpdateGraphs(GraphUpdateObject ob)
    {
        graphUpdates.UpdateGraphs(ob);
        if (batchGraphUpdates && Time.realtimeSinceStartup - lastGraphUpdate < graphUpdateBatchingInterval)
        {
            if (!graphUpdateRoutineRunning)
            {
                StartCoroutine(DelayedGraphUpdate());
            }
        }
        else
        {
            QueueGraphUpdates();
        }
    }

    public void FlushGraphUpdates()
    {
        if (IsAnyGraphUpdateQueued)
        {
            QueueGraphUpdates();
            FlushWorkItems();
        }
    }

    public void FlushWorkItems()
    {
        FlushWorkItemsInternal(unblockOnComplete: true);
    }

    [Obsolete("Use FlushWorkItems() instead or use FlushWorkItemsInternal if you really need to")]
    public void FlushWorkItems(bool unblockOnComplete, bool block)
    {
        BlockUntilPathQueueBlocked();
        PerformBlockingActions(block, unblockOnComplete);
    }

    internal void FlushWorkItemsInternal(bool unblockOnComplete)
    {
        BlockUntilPathQueueBlocked();
        PerformBlockingActions(force: true, unblockOnComplete);
    }

    public void FlushThreadSafeCallbacks()
    {
        if (OnThreadSafeCallback != null)
        {
            BlockUntilPathQueueBlocked();
            PerformBlockingActions();
        }
    }

    public static int CalculateThreadCount(ThreadCount count)
    {
        if (count == ThreadCount.AutomaticLowLoad || count == ThreadCount.AutomaticHighLoad)
        {
            int num = Mathf.Max(1, SystemInfo.processorCount);
            int num2 = SystemInfo.systemMemorySize;
            if (num2 <= 0)
            {
                UnityEngine.Debug.LogError("Machine reporting that is has <= 0 bytes of RAM. This is definitely not true, assuming 1 GiB");
                num2 = 1024;
            }
            if (num <= 1)
            {
                return 0;
            }
            if (num2 <= 512)
            {
                return 0;
            }
            if (count == ThreadCount.AutomaticHighLoad)
            {
                if (num2 <= 1024)
                {
                    num = Math.Min(num, 2);
                }
            }
            else
            {
                num /= 2;
                num = Mathf.Max(1, num);
                if (num2 <= 1024)
                {
                    num = Math.Min(num, 2);
                }
                num = Math.Min(num, 6);
            }
            return num;
        }
        return (int)count;
    }

    private void Awake()
    {
        active = this;
        if (UnityEngine.Object.FindObjectsOfType(typeof(AstarPath)).Length > 1)
        {
            UnityEngine.Debug.LogError("You should NOT have more than one AstarPath component in the scene at any time.\nThis can cause serious errors since the AstarPath component builds around a singleton pattern.");
        }
        base.useGUILayout = false;
        if (Application.isPlaying)
        {
            if (OnAwakeSettings != null)
            {
                OnAwakeSettings();
            }
            GraphModifier.FindAllModifiers();
            RelevantGraphSurface.FindAllGraphSurfaces();
            InitializePathProcessor();
            InitializeProfiler();
            SetUpReferences();
            InitializeAstarData();
            FlushWorkItems();
            euclideanEmbedding.dirty = true;
            if (scanOnStartup && (!astarData.cacheStartup || astarData.file_cachedStartup == null))
            {
                Scan();
            }
        }
    }

    private void InitializePathProcessor()
    {
        int num = CalculateThreadCount(threadCount);
        int processors = Mathf.Max(num, 1);
        bool flag = num > 0;
        pathProcessor = new PathProcessor(this, pathReturnQueue, processors, flag);
        pathProcessor.OnPathPreSearch += delegate (Path path)
        {
            OnPathPreSearch?.Invoke(path);
        };
        pathProcessor.OnPathPostSearch += delegate (Path path)
        {
            LogPathResults(path);
            OnPathPostSearch?.Invoke(path);
        };
        if (flag)
        {
            graphUpdates.EnableMultithreading();
        }
    }

    internal void VerifyIntegrity()
    {
        if (active != this)
        {
            throw new Exception("Singleton pattern broken. Make sure you only have one AstarPath object in the scene");
        }
        if (astarData == null)
        {
            throw new NullReferenceException("AstarData is null... Astar not set up correctly?");
        }
        if (astarData.graphs == null)
        {
            astarData.graphs = new NavGraph[0];
        }
    }

    public void SetUpReferences()
    {
        active = this;
        if (astarData == null)
        {
            astarData = new AstarData();
        }
        if (colorSettings == null)
        {
            colorSettings = new AstarColor();
        }
        colorSettings.OnEnable();
    }

    private void InitializeProfiler()
    {
    }

    private void InitializeAstarData()
    {
        astarData.FindGraphTypes();
        astarData.Awake();
        astarData.UpdateShortcuts();
        for (int i = 0; i < astarData.graphs.Length; i++)
        {
            if (astarData.graphs[i] != null)
            {
                astarData.graphs[i].Awake();
            }
        }
    }

    private void OnDisable()
    {
        if (OnUnloadGizmoMeshes != null)
        {
            OnUnloadGizmoMeshes();
        }
    }

    private void OnDestroy()
    {
        if (!Application.isPlaying)
        {
            return;
        }
        if (logPathResults == PathLog.Heavy)
        {
            UnityEngine.Debug.Log("+++ AstarPath Component Destroyed - Cleaning Up Pathfinding Data +++");
        }
        if (!(active != this))
        {
            BlockUntilPathQueueBlocked();
            euclideanEmbedding.dirty = false;
            FlushWorkItemsInternal(unblockOnComplete: false);
            pathProcessor.queue.TerminateReceivers();
            if (logPathResults == PathLog.Heavy)
            {
                UnityEngine.Debug.Log("Processing Possible Work Items");
            }
            graphUpdates.DisableMultithreading();
            pathProcessor.JoinThreads();
            if (logPathResults == PathLog.Heavy)
            {
                UnityEngine.Debug.Log("Returning Paths");
            }
            pathReturnQueue.ReturnPaths(timeSlice: false);
            if (logPathResults == PathLog.Heavy)
            {
                UnityEngine.Debug.Log("Destroying Graphs");
            }
            astarData.OnDestroy();
            if (logPathResults == PathLog.Heavy)
            {
                UnityEngine.Debug.Log("Cleaning up variables");
            }
            OnDrawGizmosCallback = null;
            OnAwakeSettings = null;
            OnGraphPreScan = null;
            OnGraphPostScan = null;
            OnPathPreSearch = null;
            OnPathPostSearch = null;
            OnPreScan = null;
            OnPostScan = null;
            OnLatePostScan = null;
            On65KOverflow = null;
            OnGraphsUpdated = null;
            OnThreadSafeCallback = null;
            active = null;
        }
    }

    public void FloodFill(GraphNode seed)
    {
        graphUpdates.FloodFill(seed);
    }

    public void FloodFill(GraphNode seed, uint area)
    {
        graphUpdates.FloodFill(seed, area);
    }

    [ContextMenu("Flood Fill Graphs")]
    public void FloodFill()
    {
        graphUpdates.FloodFill();
        workItems.OnFloodFill();
    }

    internal int GetNewNodeIndex()
    {
        return pathProcessor.GetNewNodeIndex();
    }

    internal void InitializeNode(GraphNode node)
    {
        pathProcessor.InitializeNode(node);
    }

    internal void DestroyNode(GraphNode node)
    {
        pathProcessor.DestroyNode(node);
    }

    public void BlockUntilPathQueueBlocked()
    {
        pathProcessor.BlockUntilPathQueueBlocked();
    }

    public void Scan()
    {
        foreach (Progress item in ScanAsync())
        {
        }
    }

    [Obsolete("ScanLoop is now named ScanAsync and is an IEnumerable<Progress>. Use foreach to iterate over the progress insead")]
    public void ScanLoop(OnScanStatus statusCallback)
    {
        foreach (Progress item in ScanAsync())
        {
            statusCallback(item);
        }
    }

    public IEnumerable<Progress> ScanAsync()
    {
        if (graphs == null)
        {
            yield break;
        }
        isScanning = true;
        euclideanEmbedding.dirty = false;
        VerifyIntegrity();
        BlockUntilPathQueueBlocked();
        pathReturnQueue.ReturnPaths(timeSlice: false);
        BlockUntilPathQueueBlocked();
        if (!Application.isPlaying)
        {
            GraphModifier.FindAllModifiers();
            RelevantGraphSurface.FindAllGraphSurfaces();
        }
        RelevantGraphSurface.UpdateAllPositions();
        astarData.UpdateShortcuts();
        yield return new Progress(0.05f, "Pre processing graphs");
        if (OnPreScan != null)
        {
            OnPreScan(this);
        }
        GraphModifier.TriggerEvent(GraphModifier.EventType.PreScan);
        Stopwatch watch = Stopwatch.StartNew();
        for (int j = 0; j < graphs.Length; j++)
        {
            if (graphs[j] != null)
            {
                graphs[j].GetNodes(delegate (GraphNode node)
                {
                    node.Destroy();
                    return true;
                });
            }
        }
        for (int i = 0; i < graphs.Length; i++)
        {
            if (graphs[i] != null)
            {
                float minp = Mathf.Lerp(0.1f, 0.8f, (float)i / (float)graphs.Length);
                float maxp = Mathf.Lerp(0.1f, 0.8f, ((float)i + 0.95f) / (float)graphs.Length);
                string progressDescriptionPrefix = "Scanning graph " + (i + 1) + " of " + graphs.Length + " - ";
                foreach (Progress progress in ScanGraph(graphs[i]))
                {
                    yield return new Progress(Mathf.Lerp(minp, maxp, progress.progress), progressDescriptionPrefix + progress.description);
                }
            }
        }
        yield return new Progress(0.8f, "Post processing graphs");
        if (OnPostScan != null)
        {
            OnPostScan(this);
        }
        GraphModifier.TriggerEvent(GraphModifier.EventType.PostScan);
        try
        {
            FlushWorkItemsInternal(unblockOnComplete: false);
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogException(e);
        }
        yield return new Progress(0.9f, "Computing areas");
        FloodFill();
        VerifyIntegrity();
        yield return new Progress(0.95f, "Late post processing");
        isScanning = false;
        if (OnLatePostScan != null)
        {
            OnLatePostScan(this);
        }
        GraphModifier.TriggerEvent(GraphModifier.EventType.LatePostScan);
        euclideanEmbedding.dirty = true;
        euclideanEmbedding.RecalculatePivots();
        PerformBlockingActions(force: true);
        watch.Stop();
        lastScanTime = (float)watch.Elapsed.TotalSeconds;
        GC.Collect();
        Log("Scanning - Process took " + (lastScanTime * 1000f).ToString("0") + " ms to complete");
    }

    private IEnumerable<Progress> ScanGraph(NavGraph graph)
    {
        if (OnGraphPreScan != null)
        {
            yield return new Progress(0f, "Pre processing");
            OnGraphPreScan(graph);
        }
        yield return new Progress(0f, string.Empty);
        foreach (Progress p in graph.ScanInternal())
        {
            yield return new Progress(Mathf.Lerp(0f, 0.95f, p.progress), p.description);
        }
        yield return new Progress(0.95f, "Assigning graph indices");
        graph.GetNodes(delegate (GraphNode node)
        {
            node.GraphIndex = base.graph.graphIndex;
            return true;
        });
        if (OnGraphPostScan != null)
        {
            yield return new Progress(0.99f, "Post processing");
            OnGraphPostScan(graph);
        }
    }

    public static void WaitForPath(Path p)
    {
        if (active == null)
        {
            throw new Exception("Pathfinding is not correctly initialized in this scene (yet?). AstarPath.active is null.\nDo not call this function in Awake");
        }
        if (p == null)
        {
            throw new ArgumentNullException("Path must not be null");
        }
        if (active.pathProcessor.queue.IsTerminating)
        {
            return;
        }
        if (p.GetState() == PathState.Created)
        {
            throw new Exception("The specified path has not been started yet.");
        }
        waitForPathDepth++;
        if (waitForPathDepth == 5)
        {
            UnityEngine.Debug.LogError("You are calling the WaitForPath function recursively (maybe from a path callback). Please don't do this.");
        }
        if (p.GetState() < PathState.ReturnQueue)
        {
            if (active.IsUsingMultithreading)
            {
                while (p.GetState() < PathState.ReturnQueue)
                {
                    if (active.pathProcessor.queue.IsTerminating)
                    {
                        waitForPathDepth--;
                        throw new Exception("Pathfinding Threads seems to have crashed.");
                    }
                    Thread.Sleep(1);
                    active.PerformBlockingActions();
                }
            }
            else
            {
                while (p.GetState() < PathState.ReturnQueue)
                {
                    if (active.pathProcessor.queue.IsEmpty && p.GetState() != PathState.Processing)
                    {
                        waitForPathDepth--;
                        throw new Exception("Critical error. Path Queue is empty but the path state is '" + p.GetState() + "'");
                    }
                    active.pathProcessor.TickNonMultithreaded();
                    active.PerformBlockingActions();
                }
            }
        }
        active.pathReturnQueue.ReturnPaths(timeSlice: false);
        waitForPathDepth--;
    }

    [Obsolete("The threadSafe parameter has been deprecated")]
    public static void RegisterSafeUpdate(Action callback, bool threadSafe)
    {
        RegisterSafeUpdate(callback);
    }

    public static void RegisterSafeUpdate(Action callback)
    {
        if (callback != null && Application.isPlaying)
        {
            if (active.pathProcessor.queue.AllReceiversBlocked)
            {
                active.pathProcessor.queue.Lock();
                try
                {
                    if (active.pathProcessor.queue.AllReceiversBlocked)
                    {
                        callback();
                        return;
                    }
                }
                finally
                {
                    active.pathProcessor.queue.Unlock();
                }
            }
            lock (safeUpdateLock)
            {
                OnThreadSafeCallback = (Action)Delegate.Combine(OnThreadSafeCallback, callback);
            }
            active.pathProcessor.queue.Block();
        }
    }

    private void InterruptPathfinding()
    {
        pathProcessor.queue.Block();
    }

    public static void StartPath(Path p, bool pushToFront = false)
    {
        AstarPath astarPath = active;
        if (object.ReferenceEquals(astarPath, null))
        {
            UnityEngine.Debug.LogError("There is no AstarPath object in the scene or it has not been initialized yet");
            return;
        }
        if (p.GetState() != 0)
        {
            throw new Exception("The path has an invalid state. Expected " + PathState.Created + " found " + p.GetState() + "\nMake sure you are not requesting the same path twice");
        }
        if (astarPath.pathProcessor.queue.IsTerminating)
        {
            p.Error();
            return;
        }
        if (astarPath.graphs == null || astarPath.graphs.Length == 0)
        {
            UnityEngine.Debug.LogError("There are no graphs in the scene");
            p.Error();
            UnityEngine.Debug.LogError(p.errorLog);
            return;
        }
        p.Claim(astarPath);
        p.AdvanceState(PathState.PathQueue);
        if (pushToFront)
        {
            astarPath.pathProcessor.queue.PushFront(p);
        }
        else
        {
            astarPath.pathProcessor.queue.Push(p);
        }
    }

    private void OnApplicationQuit()
    {
        OnDestroy();
        pathProcessor.AbortThreads();
    }

    public NNInfo GetNearest(Vector3 position)
    {
        return GetNearest(position, NNConstraint.None);
    }

    public NNInfo GetNearest(Vector3 position, NNConstraint constraint)
    {
        return GetNearest(position, constraint, null);
    }

    public NNInfo GetNearest(Vector3 position, NNConstraint constraint, GraphNode hint)
    {
        NavGraph[] graphs = this.graphs;
        float num = float.PositiveInfinity;
        NNInfoInternal internalInfo = default(NNInfoInternal);
        int num2 = -1;
        if (graphs != null)
        {
            for (int i = 0; i < graphs.Length; i++)
            {
                NavGraph navGraph = graphs[i];
                if (navGraph == null || !constraint.SuitableGraph(i, navGraph))
                {
                    continue;
                }
                NNInfoInternal nNInfoInternal = (!fullGetNearestSearch) ? navGraph.GetNearest(position, constraint) : navGraph.GetNearestForce(position, constraint);
                GraphNode node = nNInfoInternal.node;
                if (node != null)
                {
                    float magnitude = (nNInfoInternal.clampedPosition - position).magnitude;
                    if (prioritizeGraphs && magnitude < prioritizeGraphsLimit)
                    {
                        num = magnitude;
                        internalInfo = nNInfoInternal;
                        num2 = i;
                        break;
                    }
                    if (magnitude < num)
                    {
                        num = magnitude;
                        internalInfo = nNInfoInternal;
                        num2 = i;
                    }
                }
            }
        }
        if (num2 == -1)
        {
            return default(NNInfo);
        }
        if (internalInfo.constrainedNode != null)
        {
            internalInfo.node = internalInfo.constrainedNode;
            internalInfo.clampedPosition = internalInfo.constClampedPosition;
        }
        if (!fullGetNearestSearch && internalInfo.node != null && !constraint.Suitable(internalInfo.node))
        {
            NNInfoInternal nearestForce = graphs[num2].GetNearestForce(position, constraint);
            if (nearestForce.node != null)
            {
                internalInfo = nearestForce;
            }
        }
        if (!constraint.Suitable(internalInfo.node) || (constraint.constrainDistance && (internalInfo.clampedPosition - position).sqrMagnitude > maxNearestNodeDistanceSqr))
        {
            return default(NNInfo);
        }
        return new NNInfo(internalInfo);
    }

    public GraphNode GetNearest(Ray ray)
    {
        if (graphs == null)
        {
            return null;
        }
        float minDist = float.PositiveInfinity;
        GraphNode nearestNode = null;
        Vector3 lineDirection = ray.direction;
        Vector3 lineOrigin = ray.origin;
        for (int i = 0; i < graphs.Length; i++)
        {
            NavGraph navGraph = graphs[i];
            navGraph.GetNodes(delegate (GraphNode node)
            {
                Vector3 vector = (Vector3)node.position;
                Vector3 a = lineOrigin + Vector3.Dot(vector - lineOrigin, lineDirection) * lineDirection;
                float num = Mathf.Abs(a.x - vector.x);
                num *= num;
                if (num > minDist)
                {
                    return true;
                }
                num = Mathf.Abs(a.z - vector.z);
                num *= num;
                if (num > minDist)
                {
                    return true;
                }
                float sqrMagnitude = (a - vector).sqrMagnitude;
                if (sqrMagnitude < minDist)
                {
                    minDist = sqrMagnitude;
                    nearestNode = node;
                }
                return true;
            });
        }
        return nearestNode;
    }
}
