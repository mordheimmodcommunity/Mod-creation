using System;
using UnityEngine;

namespace Pathfinding.RVO
{
    [AddComponentMenu("Pathfinding/Local Avoidance/RVO Controller")]
    [HelpURL("http://arongranberg.com/astar/docs/class_pathfinding_1_1_r_v_o_1_1_r_v_o_controller.php")]
    public class RVOController : MonoBehaviour
    {
        public MovementMode movementMode;

        [Tooltip("Radius of the agent")]
        public float radius = 5f;

        [Tooltip("Height of the agent. In world units")]
        public float height = 1f;

        [Tooltip("A locked unit cannot move. Other units will still avoid it. But avoidance quality is not the best")]
        public bool locked;

        [Tooltip("Automatically set #locked to true when desired velocity is approximately zero")]
        public bool lockWhenNotMoving = true;

        [Tooltip("How far in the time to look for collisions with other agents")]
        public float agentTimeHorizon = 2f;

        public float obstacleTimeHorizon = 2f;

        [Tooltip("Maximum distance to other agents to take them into account for collisions.\nDecreasing this value can lead to better performance, increasing it can lead to better quality of the simulation")]
        public float neighbourDist = 10f;

        [Tooltip("Max number of other agents to take into account.\nA smaller value can reduce CPU load, a higher value can lead to better local avoidance quality.")]
        public int maxNeighbours = 10;

        public RVOLayer layer = RVOLayer.DefaultAgent;

        [AstarEnumFlag]
        public RVOLayer collidesWith = (RVOLayer)(-1);

        [HideInInspector]
        public float wallAvoidForce = 1f;

        [HideInInspector]
        public float wallAvoidFalloff = 1f;

        [Tooltip("How strongly other agents will avoid this agent")]
        [Range(0f, 1f)]
        public float priority = 0.5f;

        [Tooltip("Center of the agent relative to the pivot point of this game object")]
        public float center;

        private Transform tr;

        public bool debug;

        private static RVOSimulator cachedSimulator;

        private static readonly Color GizmoColor = new Color(0.9411765f, 71f / 85f, 0.117647059f);

        public IAgent rvoAgent
        {
            get;
            private set;
        }

        public Simulator simulator
        {
            get;
            private set;
        }

        public Vector3 position => To3D(rvoAgent.Position, rvoAgent.ElevationCoordinate);

        public Vector3 velocity
        {
            get
            {
                if (Time.deltaTime > 1E-05f)
                {
                    return CalculateMovementDelta(Time.deltaTime) / Time.deltaTime;
                }
                return Vector3.zero;
            }
        }

        public Vector3 CalculateMovementDelta(float deltaTime)
        {
            return To3D(Vector2.ClampMagnitude(rvoAgent.CalculatedTargetPoint - To2D(tr.position), rvoAgent.CalculatedSpeed * deltaTime), 0f);
        }

        public Vector3 CalculateMovementDelta(Vector3 position, float deltaTime)
        {
            return To3D(Vector2.ClampMagnitude(rvoAgent.CalculatedTargetPoint - To2D(position), rvoAgent.CalculatedSpeed * deltaTime), 0f);
        }

        public void SetCollisionNormal(Vector3 normal)
        {
            rvoAgent.SetCollisionNormal(To2D(normal));
        }

        public void ForceSetVelocity(Vector3 velocity)
        {
            rvoAgent.ForceSetVelocity(To2D(velocity));
        }

        private Vector2 To2D(Vector3 p)
        {
            float elevation;
            return To2D(p, out elevation);
        }

        private Vector2 To2D(Vector3 p, out float elevation)
        {
            if (movementMode == MovementMode.XY)
            {
                elevation = p.z;
                return new Vector2(p.x, p.y);
            }
            elevation = p.y;
            return new Vector2(p.x, p.z);
        }

        private Vector3 To3D(Vector2 p, float elevationCoordinate)
        {
            if (movementMode == MovementMode.XY)
            {
                return new Vector3(p.x, p.y, elevationCoordinate);
            }
            return new Vector3(p.x, elevationCoordinate, p.y);
        }

        public void OnDisable()
        {
            if (simulator != null)
            {
                simulator.RemoveAgent(rvoAgent);
            }
        }

        public void Awake()
        {
            tr = base.transform;
            if (cachedSimulator == null)
            {
                cachedSimulator = UnityEngine.Object.FindObjectOfType<RVOSimulator>();
            }
            if (cachedSimulator == null)
            {
                Debug.LogError("No RVOSimulator component found in the scene. Please add one.");
            }
            else
            {
                simulator = cachedSimulator.GetSimulator();
            }
        }

        public void OnEnable()
        {
            if (simulator != null)
            {
                if (rvoAgent != null)
                {
                    simulator.AddAgent(rvoAgent);
                }
                else
                {
                    float elevation;
                    Vector2 position = To2D(base.transform.position, out elevation);
                    rvoAgent = simulator.AddAgent(position, elevation);
                    rvoAgent.PreCalculationCallback = UpdateAgentProperties;
                }
                UpdateAgentProperties();
            }
        }

        protected void UpdateAgentProperties()
        {
            rvoAgent.Radius = Mathf.Max(0.001f, radius);
            rvoAgent.AgentTimeHorizon = agentTimeHorizon;
            rvoAgent.ObstacleTimeHorizon = obstacleTimeHorizon;
            rvoAgent.Locked = locked;
            rvoAgent.MaxNeighbours = maxNeighbours;
            rvoAgent.DebugDraw = debug;
            rvoAgent.NeighbourDist = neighbourDist;
            rvoAgent.Layer = layer;
            rvoAgent.CollidesWith = collidesWith;
            rvoAgent.MovementMode = movementMode;
            rvoAgent.Priority = priority;
            rvoAgent.Position = To2D(base.transform.position, out float elevation);
            if (movementMode == MovementMode.XZ)
            {
                rvoAgent.Height = height;
                rvoAgent.ElevationCoordinate = elevation + center - 0.5f * height;
            }
            else
            {
                rvoAgent.Height = 1f;
                rvoAgent.ElevationCoordinate = 0f;
            }
        }

        public void SetTarget(Vector3 pos, float speed, float maxSpeed)
        {
            rvoAgent.SetTarget(To2D(pos), speed, maxSpeed);
            if (lockWhenNotMoving)
            {
                locked = (speed < 0.001f);
            }
        }

        public void Move(Vector3 vel)
        {
            Vector2 b = To2D(vel);
            float magnitude = b.magnitude;
            rvoAgent.SetTarget(To2D(tr.position) + b, magnitude, magnitude);
            if (lockWhenNotMoving)
            {
                locked = (magnitude < 0.001f);
            }
        }

        public void Teleport(Vector3 pos)
        {
            tr.position = pos;
        }

        public void Update()
        {
        }

        private static void DrawCircle(Vector3 p, float radius, float a0, float a1)
        {
            while (a0 > a1)
            {
                a0 -= MathF.PI * 2f;
            }
            Vector3 b = new Vector3(Mathf.Cos(a0) * radius, 0f, Mathf.Sin(a0) * radius);
            for (int i = 0; (float)i <= 40f; i++)
            {
                Vector3 vector = new Vector3(Mathf.Cos(Mathf.Lerp(a0, a1, (float)i / 40f)) * radius, 0f, Mathf.Sin(Mathf.Lerp(a0, a1, (float)i / 40f)) * radius);
                Gizmos.DrawLine(p + b, p + vector);
                b = vector;
            }
        }

        private static void DrawCylinder(Vector3 p, Vector3 up, float height, float radius)
        {
            Vector3 normalized = Vector3.Cross(up, Vector3.one).normalized;
            Gizmos.matrix = Matrix4x4.TRS(p, Quaternion.LookRotation(normalized, up), new Vector3(radius, height, radius));
            DrawCircle(new Vector2(0f, 0f), 1f, 0f, MathF.PI * 2f);
            if (height > 0f)
            {
                DrawCircle(new Vector2(0f, 1f), 1f, 0f, MathF.PI * 2f);
                Gizmos.DrawLine(new Vector3(1f, 0f, 0f), new Vector3(1f, 1f, 0f));
                Gizmos.DrawLine(new Vector3(-1f, 0f, 0f), new Vector3(-1f, 1f, 0f));
                Gizmos.DrawLine(new Vector3(0f, 0f, 1f), new Vector3(0f, 1f, 1f));
                Gizmos.DrawLine(new Vector3(0f, 0f, -1f), new Vector3(0f, 1f, -1f));
            }
        }

        private void OnDrawGizmos()
        {
            if (locked)
            {
                Gizmos.color = GizmoColor * 0.5f;
            }
            else
            {
                Gizmos.color = GizmoColor;
            }
            if (movementMode == MovementMode.XY)
            {
                DrawCylinder(base.transform.position, Vector3.forward, 0f, radius);
            }
            else
            {
                DrawCylinder(base.transform.position + To3D(Vector2.zero, center - height * 0.5f), To3D(Vector2.zero, 1f), height, radius);
            }
        }

        private void OnDrawGizmosSelected()
        {
        }
    }
}
