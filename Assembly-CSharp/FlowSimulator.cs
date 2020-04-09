using Flowmap;
using UnityEngine;

[RequireComponent(typeof(FlowmapGenerator))]
public abstract class FlowSimulator : MonoBehaviour
{
	public int resolutionX = 256;

	public int resolutionY = 256;

	public SimulationBorderCollision borderCollision;

	public bool simulateOnPlay;

	public int maxSimulationSteps = 500;

	private int simulationStepsCount;

	public bool continuousSimulation;

	public string outputFolderPath;

	public string outputPrefix;

	public bool writeToFileOnMaxSimulationSteps = true;

	private bool simulating;

	private bool initialized;

	protected FlowmapGenerator generator;

	public int SimulationStepsCount => simulationStepsCount;

	public bool Simulating => simulating;

	protected bool Initialized => initialized;

	public FlowmapGenerator Generator
	{
		get
		{
			if (generator == null)
			{
				generator = GetComponent<FlowmapGenerator>();
			}
			return generator;
		}
	}

	protected virtual void Update()
	{
		if (!Initialized)
		{
			Init();
		}
		if (Application.isPlaying && Simulating)
		{
			Tick();
		}
	}

	public virtual void Init()
	{
		simulationStepsCount = 0;
		initialized = true;
	}

	public virtual void StartSimulating()
	{
		if (!Initialized || SimulationStepsCount == 0)
		{
			Init();
		}
		simulating = true;
	}

	public virtual void StopSimulating()
	{
		simulating = false;
	}

	public virtual void Reset()
	{
		simulationStepsCount = 0;
		if (!Initialized)
		{
			Init();
		}
	}

	public virtual void Tick()
	{
		if (Simulating)
		{
			simulationStepsCount++;
			if (simulationStepsCount == maxSimulationSteps && maxSimulationSteps != 0 && !continuousSimulation)
			{
				MaxStepsReached();
			}
		}
	}

	protected virtual void MaxStepsReached()
	{
		StopSimulating();
	}
}
