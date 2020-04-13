using Flowmap;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("Flowmaps/Simulators/Shallow Water")]
public class ShallowWaterSimulator : FlowSimulator
{
    public enum OutputTexture
    {
        Flowmap,
        HeightAndFluid,
        Foam
    }

    public int updateTextureDelayCPU = 10;

    public float timestep = 0.4f;

    public float evaporationRate = 0.001f;

    public float gravity = 1f;

    public float velocityScale = 1f;

    public float fluidAddMultiplier = 0.01f;

    public float fluidRemoveMultiplier = 0.01f;

    public float fluidForceMultiplier = 0.01f;

    public float initialFluidAmount;

    public FluidDepth fluidDepth;

    public float outputAccumulationRate = 0.02f;

    private int outputFilterSize = 1;

    public float outputFilterStrength = 1f;

    public bool simulateFoam;

    public float foamVelocityScale = 1f;

    public bool simulateFirstFluidHit;

    public float firstFluidHitTimeMax = 30f;

    public Material[] assignFlowmapToMaterials;

    public bool assignFlowmap;

    public string assignedFlowmapName = "_FlowmapTex";

    public bool assignHeightAndFluid;

    public string assignedHeightAndFluidName = "_HeightFluidTex";

    public bool assignUVScaleTransform;

    public string assignUVCoordsName = "_FlowmapUV";

    public bool writeHeightAndFluid;

    public bool writeFoamSeparately;

    public bool writeFluidDepthInAlpha;

    private RenderTexture heightFluidRT;

    private RenderTexture heightPreviewRT;

    private RenderTexture fluidPreviewRT;

    private RenderTexture fluidAddRT;

    private RenderTexture fluidRemoveRT;

    private RenderTexture fluidForceRT;

    private RenderTexture heightmapFieldsRT;

    private RenderTexture outflowRT;

    private RenderTexture bufferRT1;

    private RenderTexture velocityRT;

    private RenderTexture velocityAccumulatedRT;

    private Material simulationMaterial;

    private Camera fieldRenderCamera;

    private bool initializedGpu;

    [SerializeField]
    [HideInInspector]
    private SimulationData[][] simulationDataCpu;

    private Texture2D heightFluidCpu;

    private Texture2D velocityAccumulatedCpu;

    private bool initializedCpu;

    private Material SimulationMaterial
    {
        get
        {
            if (!simulationMaterial)
            {
                simulationMaterial = new Material(Shader.Find("Hidden/ShallowWaterFlowmapSimulator"));
                simulationMaterial.hideFlags = HideFlags.HideAndDontSave;
            }
            return simulationMaterial;
        }
    }

    public Texture HeightFluidTexture
    {
        get
        {
            if (FlowmapGenerator.SimulationPath == SimulationPath.CPU)
            {
                return heightFluidCpu;
            }
            return heightFluidRT;
        }
    }

    public Texture VelocityAccumulatedTexture
    {
        get
        {
            if (FlowmapGenerator.SimulationPath == SimulationPath.CPU && (bool)velocityAccumulatedCpu)
            {
                return velocityAccumulatedCpu;
            }
            if ((bool)velocityAccumulatedRT)
            {
                return velocityAccumulatedRT;
            }
            return null;
        }
    }

    public event VoidEvent OnRenderTextureReset;

    public event VoidEvent OnMaxStepsReached;

    public override void Init()
    {
        base.Init();
        Cleanup();
        switch (FlowmapGenerator.SimulationPath)
        {
            case SimulationPath.GPU:
                {
                    heightFluidRT = new RenderTexture(resolutionX, resolutionY, 0, FlowmapGenerator.GetTwoChannelRTFormat, RenderTextureReadWrite.Linear);
                    heightFluidRT.hideFlags = HideFlags.HideAndDontSave;
                    heightFluidRT.name = "HeightFluid";
                    heightFluidRT.Create();
                    fluidAddRT = new RenderTexture(resolutionX, resolutionY, 0, FlowmapGenerator.GetSingleChannelRTFormat, RenderTextureReadWrite.Linear);
                    fluidAddRT.hideFlags = HideFlags.HideAndDontSave;
                    fluidAddRT.name = "FluidAdd";
                    fluidAddRT.Create();
                    fluidRemoveRT = new RenderTexture(resolutionX, resolutionY, 0, FlowmapGenerator.GetSingleChannelRTFormat, RenderTextureReadWrite.Linear);
                    fluidRemoveRT.hideFlags = HideFlags.HideAndDontSave;
                    fluidRemoveRT.name = "FluidRemove";
                    fluidRemoveRT.Create();
                    fluidForceRT = new RenderTexture(resolutionX, resolutionY, 0, FlowmapGenerator.GetFourChannelRTFormat, RenderTextureReadWrite.Linear);
                    fluidForceRT.hideFlags = HideFlags.HideAndDontSave;
                    fluidForceRT.name = "FluidForce";
                    fluidForceRT.Create();
                    outflowRT = new RenderTexture(resolutionX, resolutionY, 0, FlowmapGenerator.GetFourChannelRTFormat, RenderTextureReadWrite.Linear);
                    outflowRT.hideFlags = HideFlags.HideAndDontSave;
                    outflowRT.name = "Outflow";
                    outflowRT.Create();
                    velocityRT = new RenderTexture(resolutionX, resolutionY, 0, FlowmapGenerator.GetTwoChannelRTFormat, RenderTextureReadWrite.Linear);
                    velocityRT.hideFlags = HideFlags.HideAndDontSave;
                    velocityRT.name = "Velocity";
                    velocityRT.Create();
                    velocityAccumulatedRT = new RenderTexture(resolutionX, resolutionY, 0, FlowmapGenerator.GetFourChannelRTFormat, RenderTextureReadWrite.Linear);
                    velocityAccumulatedRT.hideFlags = HideFlags.HideAndDontSave;
                    velocityAccumulatedRT.name = "VelocityAccumulated";
                    velocityAccumulatedRT.Create();
                    bufferRT1 = new RenderTexture(resolutionX, resolutionY, 0, FlowmapGenerator.GetFourChannelRTFormat, RenderTextureReadWrite.Linear);
                    bufferRT1.hideFlags = HideFlags.HideAndDontSave;
                    bufferRT1.name = "Buffer1";
                    bufferRT1.Create();
                    fieldRenderCamera = new GameObject("Field Renderer", typeof(Camera)).GetComponent<Camera>();
                    fieldRenderCamera.gameObject.hideFlags = HideFlags.HideAndDontSave;
                    fieldRenderCamera.orthographic = true;
                    Camera camera = fieldRenderCamera;
                    Vector2 dimensions = base.Generator.Dimensions;
                    float x = dimensions.x;
                    Vector2 dimensions2 = base.Generator.Dimensions;
                    camera.orthographicSize = Mathf.Max(x, dimensions2.y) * 0.5f;
                    fieldRenderCamera.renderingPath = RenderingPath.Forward;
                    fieldRenderCamera.cullingMask = 1 << FlowmapGenerator.GpuRenderLayer.value;
                    fieldRenderCamera.clearFlags = CameraClearFlags.Color;
                    fieldRenderCamera.backgroundColor = Color.black;
                    fieldRenderCamera.enabled = false;
                    ResetGPUData();
                    initializedGpu = true;
                    break;
                }
            case SimulationPath.CPU:
                {
                    ResetCpuData();
                    BakeFieldsCpu();
                    for (int i = 0; i < resolutionX; i++)
                    {
                        for (int j = 0; j < resolutionY; j++)
                        {
                            simulationDataCpu[i][j].fluid = ((fluidDepth != 0) ? initialFluidAmount : ((1f - Mathf.Ceil(simulationDataCpu[i][j].height)) * initialFluidAmount));
                        }
                    }
                    initializedCpu = true;
                    break;
                }
        }
        if (base.Generator.Heightmap is FlowRenderHeightmap)
        {
            (base.Generator.Heightmap as FlowRenderHeightmap).UpdateHeightmap();
        }
    }

    private void DestroyProperly(UnityEngine.Object obj)
    {
        if (Application.isEditor || !Application.isPlaying)
        {
            UnityEngine.Object.DestroyImmediate(obj);
        }
        else if (Application.isPlaying && !Application.isEditor)
        {
            UnityEngine.Object.Destroy(obj);
        }
    }

    private void Cleanup()
    {
        RenderTexture.active = null;
        if ((bool)heightFluidRT)
        {
            DestroyProperly(heightFluidRT);
        }
        if ((bool)fluidAddRT)
        {
            DestroyProperly(fluidAddRT);
        }
        if ((bool)fluidRemoveRT)
        {
            DestroyProperly(fluidRemoveRT);
        }
        if ((bool)fluidForceRT)
        {
            DestroyProperly(fluidForceRT);
        }
        if ((bool)outflowRT)
        {
            DestroyProperly(outflowRT);
        }
        if ((bool)velocityRT)
        {
            DestroyProperly(velocityRT);
        }
        if ((bool)velocityAccumulatedRT)
        {
            DestroyProperly(velocityAccumulatedRT);
        }
        if ((bool)bufferRT1)
        {
            DestroyProperly(bufferRT1);
        }
        if ((bool)fieldRenderCamera)
        {
            DestroyProperly(fieldRenderCamera.gameObject);
        }
        if ((bool)simulationMaterial)
        {
            DestroyProperly(simulationMaterial);
        }
        initializedGpu = false;
        simulationDataCpu = null;
        if ((bool)heightFluidCpu)
        {
            DestroyProperly(heightFluidCpu);
        }
        if ((bool)velocityAccumulatedCpu)
        {
            DestroyProperly(heightFluidCpu);
        }
        initializedCpu = false;
    }

    private void OnDestroy()
    {
        Cleanup();
    }

    public override void Reset()
    {
        Init();
        base.Reset();
        AssignToMaterials();
    }

    private void ResetGPUData()
    {
        SimulationMaterial.SetColor("_Color", new Color(0.5f, 0.5f, 0f, 1f));
        Graphics.Blit(null, velocityRT, SimulationMaterial, 4);
        Graphics.Blit(null, velocityAccumulatedRT, SimulationMaterial, 4);
        Graphics.Blit(null, fluidForceRT, SimulationMaterial, 4);
        SimulationMaterial.SetColor("_Color", Color.black);
        Graphics.Blit(null, fluidAddRT, SimulationMaterial, 4);
        Graphics.Blit(null, fluidRemoveRT, SimulationMaterial, 4);
        Graphics.Blit(null, bufferRT1, SimulationMaterial, 4);
        SimulationMaterial.SetColor("_Color", new Color(0f, 0f, 0f, 0f));
        Graphics.Blit(null, outflowRT, SimulationMaterial, 4);
        if ((bool)base.Generator.Heightmap)
        {
            Graphics.Blit(base.Generator.Heightmap.HeightmapTexture, heightFluidRT, SimulationMaterial, 6);
        }
        else
        {
            SimulationMaterial.SetColor("_Color", new Color(0f, 0f, 0f, 0f));
            Graphics.Blit(null, heightFluidRT, SimulationMaterial, 4);
        }
        if (initialFluidAmount > 0f)
        {
            SimulationMaterial.SetFloat("_DeepWater", (fluidDepth == FluidDepth.DeepWater) ? 1 : 0);
            SimulationMaterial.SetFloat("_FluidAmount", initialFluidAmount);
            Graphics.Blit(heightFluidRT, bufferRT1, SimulationMaterial, 14);
            Graphics.Blit(bufferRT1, heightFluidRT);
        }
    }

    private void ResetCpuData()
    {
        if (simulationDataCpu == null)
        {
            simulationDataCpu = new SimulationData[resolutionX][];
        }
        for (int i = 0; i < resolutionX; i++)
        {
            if (simulationDataCpu[i] == null)
            {
                simulationDataCpu[i] = new SimulationData[resolutionY];
            }
            for (int j = 0; j < resolutionY; j++)
            {
                simulationDataCpu[i][j] = default(SimulationData);
                simulationDataCpu[i][j].velocity = new Vector3(0.5f, 0.5f, 0f);
                simulationDataCpu[i][j].velocityAccumulated = new Vector3(0.5f, 0.5f, 0f);
            }
        }
    }

    public override void StartSimulating()
    {
        base.StartSimulating();
        switch (FlowmapGenerator.SimulationPath)
        {
            case SimulationPath.CPU:
                if (simulationDataCpu == null)
                {
                    initializedCpu = false;
                }
                if (!initializedCpu)
                {
                    Init();
                }
                break;
            case SimulationPath.GPU:
                if (!initializedGpu)
                {
                    Init();
                }
                break;
        }
    }

    public override void Tick()
    {
        base.Tick();
        if (!base.Simulating)
        {
            return;
        }
        switch (FlowmapGenerator.SimulationPath)
        {
            case SimulationPath.GPU:
                {
                    if (!heightFluidRT.IsCreated() || !outflowRT.IsCreated() || !velocityRT.IsCreated() || !velocityAccumulatedRT.IsCreated())
                    {
                        if (this.OnRenderTextureReset != null)
                        {
                            this.OnRenderTextureReset();
                        }
                        Init();
                    }
                    Vector3 position = base.Generator.transform.position;
                    float num15 = position.y;
                    Vector3 position2 = base.Generator.transform.position;
                    float num16 = position2.y;
                    for (int num17 = 0; num17 < base.Generator.Fields.Length; num17++)
                    {
                        float a = num15;
                        Vector3 position3 = base.Generator.Fields[num17].transform.position;
                        num15 = Mathf.Max(a, position3.y);
                        float a2 = num16;
                        Vector3 position4 = base.Generator.Fields[num17].transform.position;
                        num16 = Mathf.Min(a2, position4.y);
                    }
                    fieldRenderCamera.transform.localPosition = base.Generator.transform.position;
                    Transform transform = fieldRenderCamera.transform;
                    Vector3 position5 = fieldRenderCamera.transform.position;
                    float x = position5.x;
                    float y = num15 + 1f;
                    Vector3 position6 = fieldRenderCamera.transform.position;
                    transform.position = new Vector3(x, y, position6.z);
                    fieldRenderCamera.farClipPlane = num15 - num16 + 2f;
                    fieldRenderCamera.transform.rotation = Quaternion.LookRotation(Vector3.down, Vector3.forward);
                    SimulationMaterial.SetVector("_Resolution", new Vector4(resolutionX, resolutionY, 0f, 0f));
                    SimulationMaterial.SetFloat("_Timestep", timestep);
                    SimulationMaterial.SetFloat("_Gravity", gravity);
                    SimulationMaterial.SetFloat("_VelocityScale", velocityScale);
                    FlowSimulationField[] fields = base.Generator.Fields;
                    foreach (FlowSimulationField flowSimulationField in fields)
                    {
                        if (flowSimulationField.Pass == FieldPass.AddFluid)
                        {
                            flowSimulationField.TickStart();
                        }
                    }
                    fieldRenderCamera.backgroundColor = Color.black;
                    fieldRenderCamera.targetTexture = fluidAddRT;
                    fieldRenderCamera.RenderWithShader(Shader.Find("Hidden/FlowmapOffscreenRender"), "Offscreen");
                    FlowSimulationField[] fields2 = base.Generator.Fields;
                    foreach (FlowSimulationField flowSimulationField2 in fields2)
                    {
                        if (flowSimulationField2.Pass == FieldPass.AddFluid)
                        {
                            flowSimulationField2.TickEnd();
                        }
                    }
                    FlowSimulationField[] fields3 = base.Generator.Fields;
                    foreach (FlowSimulationField flowSimulationField3 in fields3)
                    {
                        if (flowSimulationField3.Pass == FieldPass.RemoveFluid)
                        {
                            flowSimulationField3.TickStart();
                        }
                    }
                    fieldRenderCamera.backgroundColor = Color.black;
                    fieldRenderCamera.targetTexture = fluidRemoveRT;
                    fieldRenderCamera.RenderWithShader(Shader.Find("Hidden/FlowmapOffscreenRender"), "Offscreen");
                    FlowSimulationField[] fields4 = base.Generator.Fields;
                    foreach (FlowSimulationField flowSimulationField4 in fields4)
                    {
                        if (flowSimulationField4.Pass == FieldPass.RemoveFluid)
                        {
                            flowSimulationField4.TickEnd();
                        }
                    }
                    FlowSimulationField[] fields5 = base.Generator.Fields;
                    foreach (FlowSimulationField flowSimulationField5 in fields5)
                    {
                        if (flowSimulationField5.Pass == FieldPass.Force)
                        {
                            flowSimulationField5.TickStart();
                        }
                    }
                    fieldRenderCamera.backgroundColor = new Color(Mathf.LinearToGammaSpace(0.5f), Mathf.LinearToGammaSpace(0.5f), 0f, 1f);
                    fieldRenderCamera.targetTexture = fluidForceRT;
                    fieldRenderCamera.RenderWithShader(Shader.Find("Hidden/FlowmapOffscreenRender"), "Offscreen");
                    FlowSimulationField[] fields6 = base.Generator.Fields;
                    foreach (FlowSimulationField flowSimulationField6 in fields6)
                    {
                        if (flowSimulationField6.Pass == FieldPass.Force)
                        {
                            flowSimulationField6.TickEnd();
                        }
                    }
                    if ((bool)base.Generator.Heightmap && base.Generator.Heightmap is FlowRenderHeightmap && (base.Generator.Heightmap as FlowRenderHeightmap).dynamicUpdating)
                    {
                        (base.Generator.Heightmap as FlowRenderHeightmap).UpdateHeightmap();
                    }
                    if ((bool)base.Generator.Heightmap)
                    {
                        if (base.Generator.Heightmap is FlowTextureHeightmap && (base.Generator.Heightmap as FlowTextureHeightmap).isRaw)
                        {
                            SimulationMaterial.SetFloat("_IsFloatRGBA", 1f);
                        }
                        else
                        {
                            SimulationMaterial.SetFloat("_IsFloatRGBA", 0f);
                        }
                        SimulationMaterial.SetTexture("_NewHeightTex", base.Generator.Heightmap.HeightmapTexture);
                        Graphics.Blit(heightFluidRT, bufferRT1, SimulationMaterial, 9);
                        Graphics.Blit(bufferRT1, heightFluidRT);
                    }
                    FlowSimulationField[] fields7 = base.Generator.Fields;
                    foreach (FlowSimulationField flowSimulationField7 in fields7)
                    {
                        if (flowSimulationField7.Pass == FieldPass.Heightmap)
                        {
                            flowSimulationField7.TickStart();
                        }
                    }
                    fieldRenderCamera.backgroundColor = Color.black;
                    RenderTexture temporary = RenderTexture.GetTemporary(resolutionX, resolutionY, 0, FlowmapGenerator.GetSingleChannelRTFormat, RenderTextureReadWrite.Linear);
                    fieldRenderCamera.targetTexture = temporary;
                    fieldRenderCamera.RenderWithShader(Shader.Find("Hidden/FlowmapOffscreenRender"), "Offscreen");
                    FlowSimulationField[] fields8 = base.Generator.Fields;
                    foreach (FlowSimulationField flowSimulationField8 in fields8)
                    {
                        if (flowSimulationField8.Pass == FieldPass.Heightmap)
                        {
                            flowSimulationField8.TickEnd();
                        }
                    }
                    SimulationMaterial.SetTexture("_HeightmapFieldsTex", temporary);
                    Graphics.Blit(heightFluidRT, bufferRT1, SimulationMaterial, 11);
                    Graphics.Blit(bufferRT1, heightFluidRT);
                    RenderTexture.ReleaseTemporary(temporary);
                    SimulationMaterial.SetTexture("_FluidAddTex", fluidAddRT);
                    SimulationMaterial.SetTexture("_FluidRemoveTex", fluidRemoveRT);
                    SimulationMaterial.SetFloat("_Evaporation", evaporationRate);
                    SimulationMaterial.SetFloat("_FluidAddMultiplier", fluidAddMultiplier);
                    SimulationMaterial.SetFloat("_FluidRemoveMultiplier", fluidRemoveMultiplier);
                    Graphics.Blit(heightFluidRT, bufferRT1, SimulationMaterial, 0);
                    Graphics.Blit(bufferRT1, heightFluidRT);
                    SimulationMaterial.SetTexture("_FluidForceTex", fluidForceRT);
                    SimulationMaterial.SetFloat("_FluidForceMultiplier", fluidForceMultiplier);
                    SimulationMaterial.SetTexture("_OutflowTex", outflowRT);
                    SimulationMaterial.SetTexture("_VelocityTex", velocityRT);
                    SimulationMaterial.SetFloat("_BorderCollision", (borderCollision == SimulationBorderCollision.Collide) ? 1 : 0);
                    Graphics.Blit(heightFluidRT, bufferRT1, SimulationMaterial, 1);
                    Graphics.Blit(bufferRT1, outflowRT);
                    SimulationMaterial.SetTexture("_OutflowTex", outflowRT);
                    Graphics.Blit(heightFluidRT, bufferRT1, SimulationMaterial, 2);
                    Graphics.Blit(bufferRT1, heightFluidRT);
                    SimulationMaterial.SetTexture("_OutflowTex", outflowRT);
                    Graphics.Blit(heightFluidRT, bufferRT1, SimulationMaterial, 3);
                    Graphics.Blit(bufferRT1, velocityRT);
                    SimulationMaterial.SetFloat("_Delta", outputAccumulationRate);
                    SimulationMaterial.SetTexture("_VelocityTex", velocityRT);
                    SimulationMaterial.SetTexture("_VelocityAccumTex", velocityAccumulatedRT);
                    Graphics.Blit(heightFluidRT, bufferRT1, SimulationMaterial, 5);
                    Graphics.Blit(bufferRT1, velocityAccumulatedRT);
                    if (simulateFoam)
                    {
                        SimulationMaterial.SetFloat("_Delta", outputAccumulationRate);
                        SimulationMaterial.SetTexture("_FluidAddTex", fluidAddRT);
                        SimulationMaterial.SetTexture("_VelocityAccumTex", velocityAccumulatedRT);
                        SimulationMaterial.SetFloat("_FoamVelocityScale", foamVelocityScale);
                        Graphics.Blit(heightFluidRT, bufferRT1, SimulationMaterial, 10);
                        Graphics.Blit(bufferRT1, velocityAccumulatedRT);
                    }
                    if (writeFluidDepthInAlpha)
                    {
                        SimulationMaterial.SetTexture("_HeightFluidTex", heightFluidRT);
                        Graphics.Blit(velocityAccumulatedRT, bufferRT1, SimulationMaterial, 15);
                        Graphics.Blit(bufferRT1, velocityAccumulatedRT);
                    }
                    if (outputFilterStrength > 0f)
                    {
                        SimulationMaterial.SetFloat("_BlurSpread", outputFilterSize);
                        SimulationMaterial.SetFloat("_Strength", outputFilterStrength);
                        Graphics.Blit(velocityAccumulatedRT, bufferRT1, SimulationMaterial, 7);
                        Graphics.Blit(bufferRT1, velocityAccumulatedRT, SimulationMaterial, 8);
                    }
                    break;
                }
            case SimulationPath.CPU:
                if (FlowmapGenerator.ThreadCount > 1)
                {
                    int num = Mathf.CeilToInt((float)resolutionX / (float)FlowmapGenerator.ThreadCount);
                    ManualResetEvent[] array = new ManualResetEvent[FlowmapGenerator.ThreadCount];
                    ArrayThreadedInfo[] array2 = new ArrayThreadedInfo[FlowmapGenerator.ThreadCount];
                    for (int i = 0; i < FlowmapGenerator.ThreadCount; i++)
                    {
                        array[i] = new ManualResetEvent(initialState: false);
                        array2[i] = new ArrayThreadedInfo(0, 0, null);
                    }
                    for (int j = 0; j < FlowmapGenerator.ThreadCount; j++)
                    {
                        array[j].Reset();
                        array2[j].start = j * num;
                        array2[j].length = ((j != FlowmapGenerator.ThreadCount - 1) ? num : (resolutionX - 1 - j * num));
                        array2[j].resetEvent = array[j];
                        ThreadPool.QueueUserWorkItem(AddRemoveFluidThreaded, array2[j]);
                    }
                    WaitHandle.WaitAll(array);
                    for (int k = 0; k < FlowmapGenerator.ThreadCount; k++)
                    {
                        int start = k * num;
                        int length = (k != FlowmapGenerator.ThreadCount - 1) ? num : (resolutionX - 1 - k * num);
                        array[k] = new ManualResetEvent(initialState: false);
                        ThreadPool.QueueUserWorkItem(OutflowThreaded, new ArrayThreadedInfo(start, length, array[k]));
                    }
                    WaitHandle.WaitAll(array);
                    for (int l = 0; l < FlowmapGenerator.ThreadCount; l++)
                    {
                        int start2 = l * num;
                        int length2 = (l != FlowmapGenerator.ThreadCount - 1) ? num : (resolutionX - 1 - l * num);
                        array[l] = new ManualResetEvent(initialState: false);
                        ThreadPool.QueueUserWorkItem(UpdateVelocityThreaded, new ArrayThreadedInfo(start2, length2, array[l]));
                    }
                    WaitHandle.WaitAll(array);
                    if (simulateFoam)
                    {
                        for (int m = 0; m < FlowmapGenerator.ThreadCount; m++)
                        {
                            int start3 = m * num;
                            int length3 = (m != FlowmapGenerator.ThreadCount - 1) ? num : (resolutionX - 1 - m * num);
                            array[m] = new ManualResetEvent(initialState: false);
                            ThreadPool.QueueUserWorkItem(FoamThreaded, new ArrayThreadedInfo(start3, length3, array[m]));
                        }
                        WaitHandle.WaitAll(array);
                    }
                    if (outputFilterStrength > 0f)
                    {
                        for (int n = 0; n < FlowmapGenerator.ThreadCount; n++)
                        {
                            int start4 = n * num;
                            int length4 = (n != FlowmapGenerator.ThreadCount - 1) ? num : (resolutionX - 1 - n * num);
                            array[n] = new ManualResetEvent(initialState: false);
                            ThreadPool.QueueUserWorkItem(BlurVelocityAccumulatedHorizontalThreaded, new ArrayThreadedInfo(start4, length4, array[n]));
                        }
                        WaitHandle.WaitAll(array);
                        for (int num2 = 0; num2 < FlowmapGenerator.ThreadCount; num2++)
                        {
                            int start5 = num2 * num;
                            int length5 = (num2 != FlowmapGenerator.ThreadCount - 1) ? num : (resolutionX - 1 - num2 * num);
                            array[num2] = new ManualResetEvent(initialState: false);
                            ThreadPool.QueueUserWorkItem(BlurVelocityAccumulatedVerticalThreaded, new ArrayThreadedInfo(start5, length5, array[num2]));
                        }
                        WaitHandle.WaitAll(array);
                    }
                }
                else
                {
                    for (int num3 = 0; num3 < resolutionX; num3++)
                    {
                        for (int num4 = 0; num4 < resolutionY; num4++)
                        {
                            AddRemoveFluidCpu(num3, num4);
                        }
                    }
                    for (int num5 = 0; num5 < resolutionX; num5++)
                    {
                        for (int num6 = 0; num6 < resolutionY; num6++)
                        {
                            OutflowCpu(num5, num6);
                        }
                    }
                    for (int num7 = 0; num7 < resolutionX; num7++)
                    {
                        for (int num8 = 0; num8 < resolutionY; num8++)
                        {
                            UpdateVelocityCpu(num7, num8);
                        }
                    }
                    if (simulateFoam)
                    {
                        for (int num9 = 0; num9 < resolutionX; num9++)
                        {
                            for (int num10 = 0; num10 < resolutionY; num10++)
                            {
                                FoamCpu(num9, num10);
                            }
                        }
                    }
                    if (outputFilterStrength > 0f)
                    {
                        for (int num11 = 0; num11 < resolutionX; num11++)
                        {
                            for (int num12 = 0; num12 < resolutionY; num12++)
                            {
                                BlurVelocityAccumulatedHorizontalCpu(num11, num12);
                            }
                        }
                        for (int num13 = 0; num13 < resolutionX; num13++)
                        {
                            for (int num14 = 0; num14 < resolutionY; num14++)
                            {
                                BlurVelocityAccumulatedVerticalCpu(num13, num14);
                            }
                        }
                    }
                }
                if (base.SimulationStepsCount % updateTextureDelayCPU == 0)
                {
                    WriteCpuDataToTexture();
                }
                break;
        }
    }

    private void BakeFieldsCpu()
    {
        if ((bool)base.Generator.Heightmap)
        {
            Texture2D texture2D = null;
            bool flag = false;
            bool flag2 = false;
            if (base.Generator.Heightmap is FlowTextureHeightmap && (base.Generator.Heightmap as FlowTextureHeightmap).HeightmapTexture as Texture2D != null)
            {
                texture2D = ((base.Generator.Heightmap as FlowTextureHeightmap).HeightmapTexture as Texture2D);
                flag = (base.Generator.Heightmap as FlowTextureHeightmap).isRaw;
            }
            else if (base.Generator.Heightmap is FlowRenderHeightmap && FlowRenderHeightmap.Supported)
            {
                (generator.Heightmap as FlowRenderHeightmap).UpdateHeightmap();
                RenderTexture renderTexture = (generator.Heightmap as FlowRenderHeightmap).HeightmapTexture as RenderTexture;
                RenderTexture temporary = RenderTexture.GetTemporary(renderTexture.width, renderTexture.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
                Graphics.Blit(renderTexture, temporary);
                texture2D = new Texture2D(renderTexture.width, renderTexture.height);
                texture2D.hideFlags = HideFlags.HideAndDontSave;
                RenderTexture.active = temporary;
                texture2D.ReadPixels(new Rect(0f, 0f, texture2D.width, texture2D.height), 0, 0);
                texture2D.Apply();
                flag2 = true;
                RenderTexture.ReleaseTemporary(temporary);
            }
            if (texture2D != null)
            {
                Color[] pixels = texture2D.GetPixels();
                for (int i = 0; i < resolutionX; i++)
                {
                    for (int j = 0; j < resolutionY; j++)
                    {
                        if (flag)
                        {
                            simulationDataCpu[i][j].height = TextureUtilities.DecodeFloatRGBA(TextureUtilities.SampleColorBilinear(pixels, texture2D.width, texture2D.height, (float)i / (float)resolutionX, (float)j / (float)resolutionY));
                            continue;
                        }
                        ref SimulationData reference = ref simulationDataCpu[i][j];
                        Color color = TextureUtilities.SampleColorBilinear(pixels, texture2D.width, texture2D.height, (float)i / (float)resolutionX, (float)j / (float)resolutionY);
                        reference.height = color.r;
                    }
                }
                if (flag2)
                {
                    if (Application.isPlaying)
                    {
                        UnityEngine.Object.Destroy(texture2D);
                    }
                    else
                    {
                        UnityEngine.Object.DestroyImmediate(texture2D);
                    }
                }
            }
        }
        if (FlowmapGenerator.ThreadCount > 1)
        {
            int num = Mathf.CeilToInt((float)resolutionX / (float)FlowmapGenerator.ThreadCount);
            ManualResetEvent[] array = new ManualResetEvent[FlowmapGenerator.ThreadCount];
            FlowSimulationField[] array2 = base.Generator.Fields.Where((FlowSimulationField f) => f is FluidAddField && f.enabled).ToArray();
            FlowSimulationField[] array3 = array2;
            foreach (FlowSimulationField flowSimulationField in array3)
            {
                flowSimulationField.TickStart();
            }
            for (int l = 0; l < FlowmapGenerator.ThreadCount; l++)
            {
                int start = l * num;
                int length = (l != FlowmapGenerator.ThreadCount - 1) ? num : (resolutionX - 1 - l * num);
                array[l] = new ManualResetEvent(initialState: false);
                ThreadPool.QueueUserWorkItem(BakeAddFluidThreaded, new ThreadedFieldBakeInfo(start, length, array[l], array2, base.Generator));
            }
            WaitHandle.WaitAll(array);
            FlowSimulationField[] array4 = array2;
            foreach (FlowSimulationField flowSimulationField2 in array4)
            {
                flowSimulationField2.TickEnd();
            }
            FlowSimulationField[] array5 = base.Generator.Fields.Where((FlowSimulationField f) => f is FluidRemoveField && f.enabled).ToArray();
            FlowSimulationField[] array6 = array5;
            foreach (FlowSimulationField flowSimulationField3 in array6)
            {
                flowSimulationField3.TickStart();
            }
            for (int num2 = 0; num2 < FlowmapGenerator.ThreadCount; num2++)
            {
                int start2 = num2 * num;
                int length2 = (num2 != FlowmapGenerator.ThreadCount - 1) ? num : (resolutionX - 1 - num2 * num);
                array[num2] = new ManualResetEvent(initialState: false);
                ThreadPool.QueueUserWorkItem(BakeRemoveFluidThreaded, new ThreadedFieldBakeInfo(start2, length2, array[num2], array5, base.Generator));
            }
            WaitHandle.WaitAll(array);
            FlowSimulationField[] array7 = array5;
            foreach (FlowSimulationField flowSimulationField4 in array7)
            {
                flowSimulationField4.TickEnd();
            }
            FlowSimulationField[] array8 = base.Generator.Fields.Where((FlowSimulationField f) => f is FlowForceField && f.enabled).ToArray();
            FlowSimulationField[] array9 = array8;
            foreach (FlowSimulationField flowSimulationField5 in array9)
            {
                flowSimulationField5.TickStart();
            }
            for (int num5 = 0; num5 < FlowmapGenerator.ThreadCount; num5++)
            {
                int start3 = num5 * num;
                int length3 = (num5 != FlowmapGenerator.ThreadCount - 1) ? num : (resolutionX - 1 - num5 * num);
                array[num5] = new ManualResetEvent(initialState: false);
                ThreadPool.QueueUserWorkItem(BakeForcesThreaded, new ThreadedFieldBakeInfo(start3, length3, array[num5], array8, base.Generator));
            }
            WaitHandle.WaitAll(array);
            FlowSimulationField[] array10 = array8;
            foreach (FlowSimulationField flowSimulationField6 in array10)
            {
                flowSimulationField6.TickEnd();
            }
            FlowSimulationField[] array11 = base.Generator.Fields.Where((FlowSimulationField f) => f is HeightmapField && f.enabled).ToArray();
            FlowSimulationField[] array12 = array11;
            foreach (FlowSimulationField flowSimulationField7 in array12)
            {
                flowSimulationField7.TickStart();
            }
            for (int num8 = 0; num8 < FlowmapGenerator.ThreadCount; num8++)
            {
                int start4 = num8 * num;
                int length4 = (num8 != FlowmapGenerator.ThreadCount - 1) ? num : (resolutionX - 1 - num8 * num);
                array[num8] = new ManualResetEvent(initialState: false);
                ThreadPool.QueueUserWorkItem(BakeHeightmapThreaded, new ThreadedFieldBakeInfo(start4, length4, array[num8], array11, base.Generator));
            }
            WaitHandle.WaitAll(array);
            FlowSimulationField[] array13 = array11;
            foreach (FlowSimulationField flowSimulationField8 in array13)
            {
                flowSimulationField8.TickEnd();
            }
        }
        else
        {
            FlowSimulationField[] array14 = base.Generator.Fields.Where((FlowSimulationField f) => f is FluidAddField && f.enabled).ToArray();
            FlowSimulationField[] array15 = array14;
            foreach (FlowSimulationField flowSimulationField9 in array15)
            {
                flowSimulationField9.TickStart();
            }
            for (int num11 = 0; num11 < resolutionX; num11++)
            {
                for (int num12 = 0; num12 < resolutionY; num12++)
                {
                    FlowSimulationField[] array16 = array14;
                    foreach (FlowSimulationField flowSimulationField10 in array16)
                    {
                        simulationDataCpu[num11][num12].addFluid += flowSimulationField10.GetStrengthCpu(base.Generator, new Vector2((float)num11 / (float)resolutionX, (float)num12 / (float)resolutionY));
                    }
                }
            }
            FlowSimulationField[] array17 = array14;
            foreach (FlowSimulationField flowSimulationField11 in array17)
            {
                flowSimulationField11.TickEnd();
            }
            FlowSimulationField[] array18 = base.Generator.Fields.Where((FlowSimulationField f) => f is FluidRemoveField && f.enabled).ToArray();
            FlowSimulationField[] array19 = array18;
            foreach (FlowSimulationField flowSimulationField12 in array19)
            {
                flowSimulationField12.TickStart();
            }
            for (int num16 = 0; num16 < resolutionX; num16++)
            {
                for (int num17 = 0; num17 < resolutionY; num17++)
                {
                    FlowSimulationField[] array20 = array18;
                    foreach (FlowSimulationField flowSimulationField13 in array20)
                    {
                        simulationDataCpu[num16][num17].removeFluid += flowSimulationField13.GetStrengthCpu(base.Generator, new Vector2((float)num16 / (float)resolutionX, (float)num17 / (float)resolutionY));
                    }
                }
            }
            FlowSimulationField[] array21 = array18;
            foreach (FlowSimulationField flowSimulationField14 in array21)
            {
                flowSimulationField14.TickEnd();
            }
            FlowSimulationField[] array22 = base.Generator.Fields.Where((FlowSimulationField f) => f is FlowForceField && f.enabled).ToArray();
            FlowSimulationField[] array23 = array22;
            foreach (FlowSimulationField flowSimulationField15 in array23)
            {
                flowSimulationField15.TickStart();
            }
            for (int num21 = 0; num21 < resolutionX; num21++)
            {
                for (int num22 = 0; num22 < resolutionY; num22++)
                {
                    FlowSimulationField[] array24 = array22;
                    foreach (FlowSimulationField flowSimulationField16 in array24)
                    {
                        simulationDataCpu[num21][num22].force += (flowSimulationField16 as FlowForceField).GetForceCpu(base.Generator, new Vector2((float)num21 / (float)resolutionX, (float)num22 / (float)resolutionY));
                        simulationDataCpu[num21][num22].force.z = Mathf.Max(simulationDataCpu[num21][num22].force.z, 0f);
                    }
                }
            }
            FlowSimulationField[] array25 = array22;
            foreach (FlowSimulationField flowSimulationField17 in array25)
            {
                flowSimulationField17.TickEnd();
            }
            FlowSimulationField[] array26 = base.Generator.Fields.Where((FlowSimulationField f) => f is HeightmapField && f.enabled).ToArray();
            FlowSimulationField[] array27 = array26;
            foreach (FlowSimulationField flowSimulationField18 in array27)
            {
                flowSimulationField18.TickStart();
            }
            for (int num26 = 0; num26 < resolutionX; num26++)
            {
                for (int num27 = 0; num27 < resolutionY; num27++)
                {
                    FlowSimulationField[] array28 = array26;
                    foreach (FlowSimulationField flowSimulationField19 in array28)
                    {
                        float strengthCpu = flowSimulationField19.GetStrengthCpu(base.Generator, new Vector2((float)num26 / (float)resolutionX, (float)num27 / (float)resolutionY));
                        simulationDataCpu[num26][num27].height = Mathf.Lerp(simulationDataCpu[num26][num27].height, strengthCpu, strengthCpu * (1f - simulationDataCpu[num26][num27].height));
                    }
                }
            }
            FlowSimulationField[] array29 = array26;
            foreach (FlowSimulationField flowSimulationField20 in array29)
            {
                flowSimulationField20.TickEnd();
            }
        }
        WriteCpuDataToTexture();
    }

    private void AddRemoveFluidCpu(int x, int y)
    {
        simulationDataCpu[x][y].fluid += simulationDataCpu[x][y].addFluid * timestep * fluidAddMultiplier;
        simulationDataCpu[x][y].fluid = Mathf.Max(0f, simulationDataCpu[x][y].fluid - simulationDataCpu[x][y].removeFluid * fluidRemoveMultiplier);
        simulationDataCpu[x][y].fluid = simulationDataCpu[x][y].fluid * (1f - evaporationRate * timestep);
    }

    private void OutflowCpu(int x, int y)
    {
        int num = Mathf.Min(y + 1, resolutionY - 1);
        int num2 = Mathf.Min(x + 1, resolutionX - 1);
        int num3 = Mathf.Max(y - 1, 0);
        int num4 = Mathf.Max(x - 1, 0);
        Vector2 lhs = new Vector2(simulationDataCpu[x][y].force.x, simulationDataCpu[x][y].force.y);
        float num5 = Mathf.Max(0f, simulationDataCpu[x][y].outflow.x + timestep * gravity * (simulationDataCpu[x][y].height + simulationDataCpu[x][y].fluid - simulationDataCpu[x][num].height - simulationDataCpu[x][num].fluid) + Mathf.Clamp01(Vector2.Dot(lhs, new Vector2(0f, 1f))) * timestep * fluidForceMultiplier);
        float num6 = Mathf.Max(0f, simulationDataCpu[x][y].outflow.y + timestep * gravity * (simulationDataCpu[x][y].height + simulationDataCpu[x][y].fluid - simulationDataCpu[num2][y].height - simulationDataCpu[num2][y].fluid) + Mathf.Clamp01(Vector2.Dot(lhs, new Vector2(1f, 0f))) * timestep * fluidForceMultiplier);
        float num7 = Mathf.Max(0f, simulationDataCpu[x][y].outflow.z + timestep * gravity * (simulationDataCpu[x][y].height + simulationDataCpu[x][y].fluid - simulationDataCpu[x][num3].height - simulationDataCpu[x][num3].fluid) + Mathf.Clamp01(Vector2.Dot(lhs, new Vector2(0f, -1f))) * timestep * fluidForceMultiplier);
        float num8 = Mathf.Max(0f, simulationDataCpu[x][y].outflow.w + timestep * gravity * (simulationDataCpu[x][y].height + simulationDataCpu[x][y].fluid - simulationDataCpu[num4][y].height - simulationDataCpu[num4][y].fluid) + Mathf.Clamp01(Vector2.Dot(lhs, new Vector2(-1f, 0f))) * timestep * fluidForceMultiplier);
        if (borderCollision == SimulationBorderCollision.PassThrough)
        {
            if (x == 0)
            {
                num6 = 0f;
            }
            if (x == resolutionX - 1)
            {
                num8 = 0f;
            }
            if (y == 0)
            {
                num5 = 0f;
            }
            if (y == resolutionY - 1)
            {
                num7 = 0f;
            }
        }
        float num9 = (!(num5 + num6 + num7 + num8 > 0f)) ? 0f : Mathf.Min(1f, simulationDataCpu[x][y].fluid / (timestep * (num5 + num6 + num7 + num8)));
        num9 *= 1f - simulationDataCpu[x][y].force.z;
        simulationDataCpu[x][y].outflow = new Vector4(num5 * num9, num6 * num9, num7 * num9, num8 * num9);
    }

    private void UpdateVelocityCpu(int x, int y)
    {
        int num = Mathf.Min(y + 1, resolutionY - 1);
        int num2 = Mathf.Min(x + 1, resolutionX - 1);
        int num3 = Mathf.Max(y - 1, 0);
        int num4 = Mathf.Max(x - 1, 0);
        float z = simulationDataCpu[x][num].outflow.z;
        float w = simulationDataCpu[num2][y].outflow.w;
        float x2 = simulationDataCpu[x][num3].outflow.x;
        float y2 = simulationDataCpu[num4][y].outflow.y;
        float num5 = timestep * (z + w + x2 + y2 - (simulationDataCpu[x][y].outflow.x + simulationDataCpu[x][y].outflow.y + simulationDataCpu[x][y].outflow.z + simulationDataCpu[x][y].outflow.w));
        simulationDataCpu[x][y].fluid = simulationDataCpu[x][y].fluid + num5;
        float num6 = 0.5f * (y2 - w + (simulationDataCpu[x][y].outflow.y - simulationDataCpu[x][y].outflow.w));
        float num7 = 0.5f * (x2 - z + (simulationDataCpu[x][y].outflow.x - simulationDataCpu[x][y].outflow.z));
        float num8 = 0.5f * (simulationDataCpu[x][y].fluid + (simulationDataCpu[x][y].fluid + num5));
        Vector2 zero = Vector2.zero;
        if (num8 != 0f)
        {
            zero.x = num6 / num8;
            zero.y = num7 / num8;
        }
        zero.x = Mathf.Clamp(zero.x * velocityScale, -1f, 1f) * 0.5f + 0.5f;
        zero.y = Mathf.Clamp(zero.y * velocityScale, -1f, 1f) * 0.5f + 0.5f;
        simulationDataCpu[x][y].velocity = zero;
        float z2 = simulationDataCpu[x][y].velocityAccumulated.z;
        simulationDataCpu[x][y].velocityAccumulated = Vector3.Lerp(simulationDataCpu[x][y].velocityAccumulated, simulationDataCpu[x][y].velocity, outputAccumulationRate);
        simulationDataCpu[x][y].velocityAccumulated.z = z2;
    }

    private void BlurVelocityAccumulatedHorizontalCpu(int x, int y)
    {
        Vector3 velocityAccumulated = simulationDataCpu[Mathf.Max(0, x - 1)][y].velocityAccumulated;
        Vector3 velocityAccumulated2 = simulationDataCpu[Mathf.Min(resolutionX - 1, x + 1)][y].velocityAccumulated;
        simulationDataCpu[x][y].velocityAccumulated = Vector3.Lerp(simulationDataCpu[x][y].velocityAccumulated, velocityAccumulated * 0.25f + simulationDataCpu[x][y].velocityAccumulated * 0.5f + velocityAccumulated2 * 0.25f, outputFilterStrength);
    }

    private void BlurVelocityAccumulatedVerticalCpu(int x, int y)
    {
        Vector3 velocityAccumulated = simulationDataCpu[x][Mathf.Max(0, y - 1)].velocityAccumulated;
        Vector3 velocityAccumulated2 = simulationDataCpu[x][Mathf.Min(resolutionY - 1, y + 1)].velocityAccumulated;
        simulationDataCpu[x][y].velocityAccumulated = Vector3.Lerp(simulationDataCpu[x][y].velocityAccumulated, velocityAccumulated * 0.25f + simulationDataCpu[x][y].velocityAccumulated * 0.5f + velocityAccumulated2 * 0.25f, outputFilterStrength);
    }

    private void FoamCpu(int x, int y)
    {
        int num = Mathf.Min(y + 1, resolutionY - 1);
        int num2 = Mathf.Min(x + 1, resolutionX - 1);
        int num3 = Mathf.Max(y - 1, 0);
        int num4 = Mathf.Max(x - 1, 0);
        float magnitude = new Vector2(simulationDataCpu[x][y].velocityAccumulated.x * 2f - 1f, simulationDataCpu[x][y].velocityAccumulated.y * 2f - 1f).magnitude;
        float magnitude2 = new Vector2(simulationDataCpu[x][num].velocityAccumulated.x * 2f - 1f, simulationDataCpu[x][num].velocityAccumulated.y * 2f - 1f).magnitude;
        float magnitude3 = new Vector2(simulationDataCpu[num2][y].velocityAccumulated.x * 2f - 1f, simulationDataCpu[num2][y].velocityAccumulated.y * 2f - 1f).magnitude;
        float magnitude4 = new Vector2(simulationDataCpu[x][num3].velocityAccumulated.x * 2f - 1f, simulationDataCpu[x][num3].velocityAccumulated.y * 2f - 1f).magnitude;
        float magnitude5 = new Vector2(simulationDataCpu[num4][y].velocityAccumulated.x * 2f - 1f, simulationDataCpu[num4][y].velocityAccumulated.y * 2f - 1f).magnitude;
        float value = 100f * (magnitude2 - magnitude + (magnitude3 - magnitude) + (magnitude4 - magnitude) + (magnitude5 - magnitude));
        float num5 = Mathf.Pow(1f - Mathf.Clamp01(new Vector2(simulationDataCpu[x][y].velocity.x * 2f - 1f, simulationDataCpu[x][y].velocity.y * 2f - 1f).magnitude * foamVelocityScale), 2f);
        num5 *= 1f - simulationDataCpu[x][y].addFluid;
        num5 = (Mathf.Clamp01((num5 * 1.2f - 0.5f) * 4f) + 0.5f) * Mathf.Clamp01(value);
        simulationDataCpu[x][y].velocityAccumulated.z = Mathf.Lerp(simulationDataCpu[x][y].velocityAccumulated.z, num5, outputAccumulationRate);
    }

    private void BakeAddFluidThreaded(object threadContext)
    {
        ThreadedFieldBakeInfo threadedFieldBakeInfo = threadContext as ThreadedFieldBakeInfo;
        try
        {
            for (int i = threadedFieldBakeInfo.start; i < threadedFieldBakeInfo.start + threadedFieldBakeInfo.length; i++)
            {
                for (int j = 0; j < resolutionY; j++)
                {
                    FlowSimulationField[] fields = threadedFieldBakeInfo.fields;
                    foreach (FlowSimulationField flowSimulationField in fields)
                    {
                        simulationDataCpu[i][j].addFluid += flowSimulationField.GetStrengthCpu(threadedFieldBakeInfo.generator, new Vector2((float)i / (float)resolutionX, (float)j / (float)resolutionY));
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.Log(ex.ToString());
        }
        threadedFieldBakeInfo.resetEvent.Set();
    }

    private void BakeRemoveFluidThreaded(object threadContext)
    {
        ThreadedFieldBakeInfo threadedFieldBakeInfo = threadContext as ThreadedFieldBakeInfo;
        try
        {
            for (int i = threadedFieldBakeInfo.start; i < threadedFieldBakeInfo.start + threadedFieldBakeInfo.length; i++)
            {
                for (int j = 0; j < resolutionY; j++)
                {
                    FlowSimulationField[] fields = threadedFieldBakeInfo.fields;
                    foreach (FlowSimulationField flowSimulationField in fields)
                    {
                        simulationDataCpu[i][j].removeFluid += flowSimulationField.GetStrengthCpu(threadedFieldBakeInfo.generator, new Vector2((float)i / (float)resolutionX, (float)j / (float)resolutionY));
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.Log(ex.ToString());
        }
        threadedFieldBakeInfo.resetEvent.Set();
    }

    private void BakeForcesThreaded(object threadContext)
    {
        ThreadedFieldBakeInfo threadedFieldBakeInfo = threadContext as ThreadedFieldBakeInfo;
        try
        {
            for (int i = threadedFieldBakeInfo.start; i < threadedFieldBakeInfo.start + threadedFieldBakeInfo.length; i++)
            {
                for (int j = 0; j < resolutionY; j++)
                {
                    FlowSimulationField[] fields = threadedFieldBakeInfo.fields;
                    foreach (FlowSimulationField flowSimulationField in fields)
                    {
                        simulationDataCpu[i][j].force += (flowSimulationField as FlowForceField).GetForceCpu(threadedFieldBakeInfo.generator, new Vector2((float)i / (float)resolutionX, (float)j / (float)resolutionY));
                        simulationDataCpu[i][j].force.z = Mathf.Max(simulationDataCpu[i][j].force.z, 0f);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.Log(ex.ToString());
        }
        threadedFieldBakeInfo.resetEvent.Set();
    }

    private void BakeHeightmapThreaded(object threadContext)
    {
        ThreadedFieldBakeInfo threadedFieldBakeInfo = threadContext as ThreadedFieldBakeInfo;
        try
        {
            for (int i = threadedFieldBakeInfo.start; i < threadedFieldBakeInfo.start + threadedFieldBakeInfo.length; i++)
            {
                for (int j = 0; j < resolutionY; j++)
                {
                    FlowSimulationField[] fields = threadedFieldBakeInfo.fields;
                    foreach (FlowSimulationField flowSimulationField in fields)
                    {
                        float strengthCpu = flowSimulationField.GetStrengthCpu(threadedFieldBakeInfo.generator, new Vector2((float)i / (float)resolutionX, (float)j / (float)resolutionY));
                        simulationDataCpu[i][j].height = Mathf.Lerp(simulationDataCpu[i][j].height, strengthCpu, strengthCpu * (1f - simulationDataCpu[i][j].height));
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.Log(ex.ToString());
        }
        threadedFieldBakeInfo.resetEvent.Set();
    }

    private void AddRemoveFluidThreaded(object threadContext)
    {
        ArrayThreadedInfo arrayThreadedInfo = threadContext as ArrayThreadedInfo;
        try
        {
            for (int i = arrayThreadedInfo.start; i < arrayThreadedInfo.start + arrayThreadedInfo.length; i++)
            {
                for (int j = 0; j < resolutionY; j++)
                {
                    AddRemoveFluidCpu(i, j);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.Log(ex.ToString());
        }
        arrayThreadedInfo.resetEvent.Set();
    }

    private void OutflowThreaded(object threadContext)
    {
        ArrayThreadedInfo arrayThreadedInfo = threadContext as ArrayThreadedInfo;
        try
        {
            for (int i = arrayThreadedInfo.start; i < arrayThreadedInfo.start + arrayThreadedInfo.length; i++)
            {
                for (int j = 0; j < resolutionY; j++)
                {
                    OutflowCpu(i, j);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.Log(ex.ToString());
        }
        arrayThreadedInfo.resetEvent.Set();
    }

    private void UpdateVelocityThreaded(object threadContext)
    {
        ArrayThreadedInfo arrayThreadedInfo = threadContext as ArrayThreadedInfo;
        try
        {
            for (int i = arrayThreadedInfo.start; i < arrayThreadedInfo.start + arrayThreadedInfo.length; i++)
            {
                for (int j = 0; j < resolutionY; j++)
                {
                    UpdateVelocityCpu(i, j);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.Log(ex.ToString());
        }
        arrayThreadedInfo.resetEvent.Set();
    }

    private void BlurVelocityAccumulatedHorizontalThreaded(object threadContext)
    {
        ArrayThreadedInfo arrayThreadedInfo = threadContext as ArrayThreadedInfo;
        try
        {
            for (int i = arrayThreadedInfo.start; i < arrayThreadedInfo.start + arrayThreadedInfo.length; i++)
            {
                for (int j = 0; j < resolutionY; j++)
                {
                    BlurVelocityAccumulatedHorizontalCpu(i, j);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.Log(ex.ToString());
        }
        arrayThreadedInfo.resetEvent.Set();
    }

    private void BlurVelocityAccumulatedVerticalThreaded(object threadContext)
    {
        ArrayThreadedInfo arrayThreadedInfo = threadContext as ArrayThreadedInfo;
        try
        {
            for (int i = arrayThreadedInfo.start; i < arrayThreadedInfo.start + arrayThreadedInfo.length; i++)
            {
                for (int j = 0; j < resolutionY; j++)
                {
                    BlurVelocityAccumulatedVerticalCpu(i, j);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.Log(ex.ToString());
        }
        arrayThreadedInfo.resetEvent.Set();
    }

    private void FoamThreaded(object threadContext)
    {
        ArrayThreadedInfo arrayThreadedInfo = threadContext as ArrayThreadedInfo;
        try
        {
            for (int i = arrayThreadedInfo.start; i < arrayThreadedInfo.start + arrayThreadedInfo.length; i++)
            {
                for (int j = 0; j < resolutionY; j++)
                {
                    FoamCpu(i, j);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.Log(ex.ToString());
        }
        arrayThreadedInfo.resetEvent.Set();
    }

    protected override void Update()
    {
        base.Update();
        AssignToMaterials();
    }

    private void WriteCpuDataToTexture()
    {
        if (heightFluidCpu == null || heightFluidCpu.width != resolutionX || heightFluidCpu.height != resolutionY)
        {
            if ((bool)heightFluidCpu)
            {
                DestroyProperly(heightFluidCpu);
            }
            heightFluidCpu = new Texture2D(resolutionX, resolutionY, TextureFormat.ARGB32, mipmap: true, linear: true);
            heightFluidCpu.hideFlags = HideFlags.HideAndDontSave;
            heightFluidCpu.name = "HeightFluidCpu";
        }
        Color[] array = new Color[resolutionX * resolutionY];
        for (int i = 0; i < resolutionY; i++)
        {
            for (int j = 0; j < resolutionX; j++)
            {
                array[j + i * resolutionX] = new Color(simulationDataCpu[j][i].height, simulationDataCpu[j][i].fluid, 0f, 1f);
            }
        }
        heightFluidCpu.SetPixels(array);
        heightFluidCpu.Apply();
        if (velocityAccumulatedCpu == null || velocityAccumulatedCpu.width != resolutionX || velocityAccumulatedCpu.height != resolutionY)
        {
            if ((bool)velocityAccumulatedCpu)
            {
                DestroyProperly(velocityAccumulatedCpu);
            }
            velocityAccumulatedCpu = new Texture2D(resolutionX, resolutionY, TextureFormat.ARGB32, mipmap: true, linear: true);
            velocityAccumulatedCpu.hideFlags = HideFlags.HideAndDontSave;
            velocityAccumulatedCpu.name = "VelocityAccumulatedCpu";
        }
        for (int k = 0; k < resolutionY; k++)
        {
            for (int l = 0; l < resolutionX; l++)
            {
                array[l + k * resolutionX] = new Color(simulationDataCpu[l][k].velocityAccumulated.x, simulationDataCpu[l][k].velocityAccumulated.y, simulationDataCpu[l][k].velocityAccumulated.z, 1f);
            }
        }
        velocityAccumulatedCpu.SetPixels(array);
        velocityAccumulatedCpu.Apply();
    }

    private void AssignToMaterials()
    {
        if (assignFlowmapToMaterials == null)
        {
            return;
        }
        Material[] array = assignFlowmapToMaterials;
        foreach (Material material in array)
        {
            if (material == null)
            {
                continue;
            }
            if (assignFlowmap)
            {
                material.SetTexture(assignedFlowmapName, (FlowmapGenerator.SimulationPath != 0) ? ((Texture)velocityAccumulatedCpu) : ((Texture)velocityAccumulatedRT));
            }
            if (assignHeightAndFluid)
            {
                material.SetTexture(assignedHeightAndFluidName, (FlowmapGenerator.SimulationPath != 0) ? ((Texture)heightFluidCpu) : ((Texture)heightFluidRT));
            }
            if (assignUVScaleTransform)
            {
                Vector2 dimensions = base.Generator.Dimensions;
                float x = dimensions.x;
                Vector2 dimensions2 = base.Generator.Dimensions;
                if (x < dimensions2.y)
                {
                    Vector2 dimensions3 = base.Generator.Dimensions;
                    float y = dimensions3.y;
                    Vector2 dimensions4 = base.Generator.Dimensions;
                    float num = y / dimensions4.x;
                    string propertyName = assignUVCoordsName;
                    Vector2 dimensions5 = base.Generator.Dimensions;
                    float x2 = dimensions5.x * num;
                    Vector2 dimensions6 = base.Generator.Dimensions;
                    float y2 = dimensions6.y;
                    Vector3 position = base.Generator.Position;
                    float x3 = position.x;
                    Vector3 position2 = base.Generator.Position;
                    material.SetVector(propertyName, new Vector4(x2, y2, x3, position2.z));
                }
                else
                {
                    Vector2 dimensions7 = base.Generator.Dimensions;
                    float x4 = dimensions7.x;
                    Vector2 dimensions8 = base.Generator.Dimensions;
                    float num2 = x4 / dimensions8.y;
                    string propertyName2 = assignUVCoordsName;
                    Vector2 dimensions9 = base.Generator.Dimensions;
                    float x5 = dimensions9.x;
                    Vector2 dimensions10 = base.Generator.Dimensions;
                    float y3 = dimensions10.y * num2;
                    Vector3 position3 = base.Generator.Position;
                    float x6 = position3.x;
                    Vector3 position4 = base.Generator.Position;
                    material.SetVector(propertyName2, new Vector4(x5, y3, x6, position4.z));
                }
            }
        }
    }

    public void WriteTextureToDisk(OutputTexture textureToWrite, string path)
    {
        switch (FlowmapGenerator.SimulationPath)
        {
            case SimulationPath.GPU:
                switch (textureToWrite)
                {
                    case OutputTexture.HeightAndFluid:
                        TextureUtilities.WriteRenderTextureToFile(heightFluidRT, path, linear: true, TextureUtilities.SupportedFormats[generator.outputFileFormat], "Hidden/WriteHeightFluid");
                        break;
                    case OutputTexture.Flowmap:
                        if (writeFoamSeparately)
                        {
                            RenderTexture flowmapWithoutFoamRT = GetFlowmapWithoutFoamRT();
                            TextureUtilities.WriteRenderTextureToFile(flowmapWithoutFoamRT, path, linear: true, TextureUtilities.SupportedFormats[generator.outputFileFormat]);
                            if (Application.isPlaying)
                            {
                                UnityEngine.Object.Destroy(flowmapWithoutFoamRT);
                            }
                        }
                        else
                        {
                            TextureUtilities.WriteRenderTextureToFile(velocityAccumulatedRT, path, linear: true, TextureUtilities.SupportedFormats[generator.outputFileFormat]);
                        }
                        break;
                    case OutputTexture.Foam:
                        {
                            RenderTexture foamRT = GetFoamRT();
                            TextureUtilities.WriteRenderTextureToFile(foamRT, path, linear: true, TextureUtilities.SupportedFormats[generator.outputFileFormat]);
                            if (Application.isPlaying)
                            {
                                UnityEngine.Object.Destroy(foamRT);
                            }
                            else
                            {
                                UnityEngine.Object.DestroyImmediate(foamRT);
                            }
                            break;
                        }
                }
                break;
            case SimulationPath.CPU:
                switch (textureToWrite)
                {
                    case OutputTexture.HeightAndFluid:
                        TextureUtilities.WriteTexture2DToFile(heightFluidCpu, path, TextureUtilities.SupportedFormats[generator.outputFileFormat]);
                        break;
                    case OutputTexture.Flowmap:
                        if (writeFoamSeparately)
                        {
                            Texture2D flowmapWithoutFoamTextureCPU = GetFlowmapWithoutFoamTextureCPU();
                            TextureUtilities.WriteTexture2DToFile(flowmapWithoutFoamTextureCPU, path, TextureUtilities.SupportedFormats[generator.outputFileFormat]);
                            if (Application.isPlaying)
                            {
                                UnityEngine.Object.Destroy(flowmapWithoutFoamTextureCPU);
                            }
                            else
                            {
                                UnityEngine.Object.DestroyImmediate(flowmapWithoutFoamTextureCPU);
                            }
                        }
                        else
                        {
                            TextureUtilities.WriteTexture2DToFile(velocityAccumulatedCpu, path, TextureUtilities.SupportedFormats[generator.outputFileFormat]);
                        }
                        break;
                    case OutputTexture.Foam:
                        {
                            Texture2D foamTextureCPU = GetFoamTextureCPU();
                            TextureUtilities.WriteTexture2DToFile(foamTextureCPU, path, TextureUtilities.SupportedFormats[generator.outputFileFormat]);
                            if (Application.isPlaying)
                            {
                                UnityEngine.Object.Destroy(foamTextureCPU);
                            }
                            else
                            {
                                UnityEngine.Object.DestroyImmediate(foamTextureCPU);
                            }
                            break;
                        }
                }
                break;
        }
    }

    private Texture2D GetFoamTextureCPU()
    {
        Texture2D texture2D = new Texture2D(resolutionX, resolutionY, TextureFormat.ARGB32, mipmap: true);
        texture2D.hideFlags = HideFlags.HideAndDontSave;
        Color[] array = new Color[resolutionX * resolutionY];
        for (int i = 0; i < resolutionY; i++)
        {
            for (int j = 0; j < resolutionX; j++)
            {
                array[j + i * resolutionX] = new Color(simulationDataCpu[j][i].velocityAccumulated.z, simulationDataCpu[j][i].velocityAccumulated.z, simulationDataCpu[j][i].velocityAccumulated.z, 1f);
            }
        }
        texture2D.SetPixels(array);
        texture2D.Apply();
        return texture2D;
    }

    private Texture2D GetFlowmapWithoutFoamTextureCPU()
    {
        Texture2D texture2D = new Texture2D(resolutionX, resolutionY, TextureFormat.ARGB32, mipmap: true);
        texture2D.hideFlags = HideFlags.HideAndDontSave;
        Color[] array = new Color[resolutionX * resolutionY];
        for (int i = 0; i < resolutionY; i++)
        {
            for (int j = 0; j < resolutionX; j++)
            {
                array[j + i * resolutionX] = new Color(simulationDataCpu[j][i].velocityAccumulated.x, simulationDataCpu[j][i].velocityAccumulated.y, 0f, 1f);
            }
        }
        texture2D.SetPixels(array);
        texture2D.Apply();
        return texture2D;
    }

    private RenderTexture GetFoamRT()
    {
        RenderTexture renderTexture = new RenderTexture(resolutionX, resolutionY, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
        Graphics.Blit(velocityAccumulatedRT, renderTexture, SimulationMaterial, 12);
        return renderTexture;
    }

    private RenderTexture GetFlowmapWithoutFoamRT()
    {
        RenderTexture renderTexture = new RenderTexture(resolutionX, resolutionY, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
        Graphics.Blit(velocityAccumulatedRT, renderTexture, SimulationMaterial, 13);
        return renderTexture;
    }

    protected override void MaxStepsReached()
    {
        base.MaxStepsReached();
        if (writeToFileOnMaxSimulationSteps && !string.IsNullOrEmpty(outputFolderPath) && Directory.Exists(outputFolderPath))
        {
            WriteAllTextures();
        }
        if (this.OnMaxStepsReached != null)
        {
            this.OnMaxStepsReached();
        }
    }

    public void WriteAllTextures()
    {
        switch (FlowmapGenerator.SimulationPath)
        {
            case SimulationPath.GPU:
                if (writeHeightAndFluid)
                {
                    TextureUtilities.WriteRenderTextureToFile(heightFluidRT, outputFolderPath + "/" + outputPrefix + "HeightAndFluid", linear: true, TextureUtilities.SupportedFormats[generator.outputFileFormat], "Hidden/WriteHeightFluid");
                }
                if (writeFoamSeparately)
                {
                    RenderTexture foamRT = GetFoamRT();
                    TextureUtilities.WriteRenderTextureToFile(foamRT, outputFolderPath + "/" + outputPrefix + "Foam", TextureUtilities.SupportedFormats[generator.outputFileFormat]);
                    if (Application.isPlaying)
                    {
                        UnityEngine.Object.Destroy(foamRT);
                    }
                    else
                    {
                        UnityEngine.Object.DestroyImmediate(foamRT);
                    }
                    RenderTexture flowmapWithoutFoamRT = GetFlowmapWithoutFoamRT();
                    TextureUtilities.WriteRenderTextureToFile(flowmapWithoutFoamRT, outputFolderPath + "/" + outputPrefix + "Flowmap", TextureUtilities.SupportedFormats[generator.outputFileFormat]);
                    if (Application.isPlaying)
                    {
                        UnityEngine.Object.Destroy(flowmapWithoutFoamRT);
                    }
                    else
                    {
                        UnityEngine.Object.DestroyImmediate(flowmapWithoutFoamRT);
                    }
                }
                else
                {
                    TextureUtilities.WriteRenderTextureToFile(velocityAccumulatedRT, outputFolderPath + "/" + outputPrefix + "Flowmap", TextureUtilities.SupportedFormats[generator.outputFileFormat]);
                }
                break;
            case SimulationPath.CPU:
                if (simulationDataCpu == null)
                {
                    Init();
                }
                WriteCpuDataToTexture();
                if (writeHeightAndFluid)
                {
                    TextureUtilities.WriteTexture2DToFile(heightFluidCpu, outputFolderPath + "/" + outputPrefix + "HeightAndFluid", TextureUtilities.SupportedFormats[generator.outputFileFormat]);
                }
                if (writeFoamSeparately)
                {
                    Texture2D foamTextureCPU = GetFoamTextureCPU();
                    TextureUtilities.WriteTexture2DToFile(foamTextureCPU, outputFolderPath + "/" + outputPrefix + "Foam", TextureUtilities.SupportedFormats[generator.outputFileFormat]);
                    if (Application.isPlaying)
                    {
                        UnityEngine.Object.Destroy(foamTextureCPU);
                    }
                    else
                    {
                        UnityEngine.Object.DestroyImmediate(foamTextureCPU);
                    }
                    Texture2D flowmapWithoutFoamTextureCPU = GetFlowmapWithoutFoamTextureCPU();
                    TextureUtilities.WriteTexture2DToFile(flowmapWithoutFoamTextureCPU, outputFolderPath + "/" + outputPrefix + "Flowmap", TextureUtilities.SupportedFormats[generator.outputFileFormat]);
                    if (Application.isPlaying)
                    {
                        UnityEngine.Object.Destroy(flowmapWithoutFoamTextureCPU);
                    }
                    else
                    {
                        UnityEngine.Object.DestroyImmediate(flowmapWithoutFoamTextureCPU);
                    }
                }
                else
                {
                    TextureUtilities.WriteTexture2DToFile(velocityAccumulatedCpu, outputFolderPath + "/" + outputPrefix + "Flowmap", TextureUtilities.SupportedFormats[generator.outputFileFormat]);
                }
                break;
        }
    }
}
