using Pathfinding.RVO;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Pathfinding.Examples
{
    [HelpURL("http://arongranberg.com/astar/docs/class_pathfinding_1_1_examples_1_1_lightweight_r_v_o.php")]
    [RequireComponent(typeof(MeshFilter))]
    public class LightweightRVO : MonoBehaviour
    {
        public enum RVOExampleType
        {
            Circle,
            Line,
            Point,
            RandomStreams,
            Crossing
        }

        public int agentCount = 100;

        public float exampleScale = 100f;

        public RVOExampleType type;

        public float radius = 3f;

        public float maxSpeed = 2f;

        public float agentTimeHorizon = 10f;

        [HideInInspector]
        public float obstacleTimeHorizon = 10f;

        public int maxNeighbours = 10;

        public float neighbourDist = 15f;

        public Vector3 renderingOffset = Vector3.up * 0.1f;

        public bool debug;

        private Mesh mesh;

        private Simulator sim;

        private List<IAgent> agents;

        private List<Vector3> goals;

        private List<Color> colors;

        private Vector3[] verts;

        private Vector2[] uv;

        private int[] tris;

        private Color[] meshColors;

        private Vector2[] interpolatedVelocities;

        private Vector2[] interpolatedRotations;

        public void Start()
        {
            mesh = new Mesh();
            RVOSimulator rVOSimulator = UnityEngine.Object.FindObjectOfType(typeof(RVOSimulator)) as RVOSimulator;
            if (rVOSimulator == null)
            {
                Debug.LogError("No RVOSimulator could be found in the scene. Please add a RVOSimulator component to any GameObject");
                return;
            }
            sim = rVOSimulator.GetSimulator();
            GetComponent<MeshFilter>().mesh = mesh;
            CreateAgents(agentCount);
        }

        public void OnGUI()
        {
            if (GUILayout.Button("2"))
            {
                CreateAgents(2);
            }
            if (GUILayout.Button("10"))
            {
                CreateAgents(10);
            }
            if (GUILayout.Button("100"))
            {
                CreateAgents(100);
            }
            if (GUILayout.Button("500"))
            {
                CreateAgents(500);
            }
            if (GUILayout.Button("1000"))
            {
                CreateAgents(1000);
            }
            if (GUILayout.Button("5000"))
            {
                CreateAgents(5000);
            }
            GUILayout.Space(5f);
            if (GUILayout.Button("Random Streams"))
            {
                type = RVOExampleType.RandomStreams;
                CreateAgents((agents == null) ? 100 : agents.Count);
            }
            if (GUILayout.Button("Line"))
            {
                type = RVOExampleType.Line;
                CreateAgents((agents == null) ? 10 : Mathf.Min(agents.Count, 100));
            }
            if (GUILayout.Button("Circle"))
            {
                type = RVOExampleType.Circle;
                CreateAgents((agents == null) ? 100 : agents.Count);
            }
            if (GUILayout.Button("Point"))
            {
                type = RVOExampleType.Point;
                CreateAgents((agents == null) ? 100 : agents.Count);
            }
            if (GUILayout.Button("Crossing"))
            {
                type = RVOExampleType.Crossing;
                CreateAgents((agents == null) ? 100 : agents.Count);
            }
        }

        private float uniformDistance(float radius)
        {
            float num = UnityEngine.Random.value + UnityEngine.Random.value;
            if (num > 1f)
            {
                return radius * (2f - num);
            }
            return radius * num;
        }

        public void CreateAgents(int num)
        {
            agentCount = num;
            agents = new List<IAgent>(agentCount);
            goals = new List<Vector3>(agentCount);
            colors = new List<Color>(agentCount);
            sim.ClearAgents();
            if (type == RVOExampleType.Circle)
            {
                float d = Mathf.Sqrt((float)agentCount * radius * radius * 4f / MathF.PI) * exampleScale * 0.05f;
                for (int i = 0; i < agentCount; i++)
                {
                    Vector3 a = new Vector3(Mathf.Cos((float)i * MathF.PI * 2f / (float)agentCount), 0f, Mathf.Sin((float)i * MathF.PI * 2f / (float)agentCount)) * d * (1f + UnityEngine.Random.value * 0.01f);
                    IAgent item = sim.AddAgent(new Vector2(a.x, a.z), a.y);
                    agents.Add(item);
                    goals.Add(-a);
                    colors.Add(AstarMath.HSVToRGB((float)i * 360f / (float)agentCount, 0.8f, 0.6f));
                }
            }
            else if (type == RVOExampleType.Line)
            {
                for (int j = 0; j < agentCount; j++)
                {
                    Vector3 vector = new Vector3((float)((j % 2 == 0) ? 1 : (-1)) * exampleScale, 0f, (float)(j / 2) * radius * 2.5f);
                    IAgent item2 = sim.AddAgent(new Vector2(vector.x, vector.z), vector.y);
                    agents.Add(item2);
                    goals.Add(new Vector3(0f - vector.x, vector.y, vector.z));
                    colors.Add((j % 2 != 0) ? Color.blue : Color.red);
                }
            }
            else if (type == RVOExampleType.Point)
            {
                for (int k = 0; k < agentCount; k++)
                {
                    Vector3 vector2 = new Vector3(Mathf.Cos((float)k * MathF.PI * 2f / (float)agentCount), 0f, Mathf.Sin((float)k * MathF.PI * 2f / (float)agentCount)) * exampleScale;
                    IAgent item3 = sim.AddAgent(new Vector2(vector2.x, vector2.z), vector2.y);
                    agents.Add(item3);
                    goals.Add(new Vector3(0f, vector2.y, 0f));
                    colors.Add(AstarMath.HSVToRGB((float)k * 360f / (float)agentCount, 0.8f, 0.6f));
                }
            }
            else if (type == RVOExampleType.RandomStreams)
            {
                float num2 = Mathf.Sqrt((float)agentCount * radius * radius * 4f / MathF.PI) * exampleScale * 0.05f;
                for (int l = 0; l < agentCount; l++)
                {
                    float f = UnityEngine.Random.value * MathF.PI * 2f;
                    float num3 = UnityEngine.Random.value * MathF.PI * 2f;
                    Vector3 vector3 = new Vector3(Mathf.Cos(f), 0f, Mathf.Sin(f)) * uniformDistance(num2);
                    IAgent item4 = sim.AddAgent(new Vector2(vector3.x, vector3.z), vector3.y);
                    agents.Add(item4);
                    goals.Add(new Vector3(Mathf.Cos(num3), 0f, Mathf.Sin(num3)) * uniformDistance(num2));
                    colors.Add(AstarMath.HSVToRGB(num3 * 57.29578f, 0.8f, 0.6f));
                }
            }
            else if (type == RVOExampleType.Crossing)
            {
                float num4 = exampleScale * radius * 0.5f;
                int a2 = (int)Mathf.Sqrt((float)agentCount / 25f);
                a2 = Mathf.Max(a2, 2);
                for (int m = 0; m < agentCount; m++)
                {
                    float num5 = (float)(m % a2) / (float)a2 * MathF.PI * 2f;
                    float d2 = num4 * ((float)(m / (a2 * 10) + 1) + 0.3f * UnityEngine.Random.value);
                    Vector3 vector4 = new Vector3(Mathf.Cos(num5), 0f, Mathf.Sin(num5)) * d2;
                    IAgent agent = sim.AddAgent(new Vector2(vector4.x, vector4.z), vector4.y);
                    agent.Priority = ((m % a2 != 0) ? 0.01f : 1f);
                    agents.Add(agent);
                    goals.Add(-vector4.normalized * num4 * 3f);
                    colors.Add(AstarMath.HSVToRGB(num5 * 57.29578f, 0.8f, 0.6f));
                }
            }
            for (int n = 0; n < agents.Count; n++)
            {
                IAgent agent2 = agents[n];
                agent2.Radius = radius;
                agent2.AgentTimeHorizon = agentTimeHorizon;
                agent2.ObstacleTimeHorizon = obstacleTimeHorizon;
                agent2.MaxNeighbours = maxNeighbours;
                agent2.NeighbourDist = neighbourDist;
                agent2.DebugDraw = (n == 0 && debug);
            }
            verts = new Vector3[4 * agents.Count];
            uv = new Vector2[verts.Length];
            tris = new int[agents.Count * 2 * 3];
            meshColors = new Color[verts.Length];
        }

        public void Update()
        {
            if (agents == null || mesh == null)
            {
                return;
            }
            if (agents.Count != goals.Count)
            {
                Debug.LogError("Agent count does not match goal count");
                return;
            }
            if (interpolatedVelocities == null || interpolatedVelocities.Length < agents.Count)
            {
                Vector2[] array = new Vector2[agents.Count];
                Vector2[] array2 = new Vector2[agents.Count];
                if (interpolatedVelocities != null)
                {
                    for (int i = 0; i < interpolatedVelocities.Length; i++)
                    {
                        array[i] = interpolatedVelocities[i];
                    }
                }
                if (interpolatedRotations != null)
                {
                    for (int j = 0; j < interpolatedRotations.Length; j++)
                    {
                        array2[j] = interpolatedRotations[j];
                    }
                }
                interpolatedVelocities = array;
                interpolatedRotations = array2;
            }
            Vector2 vector5 = default(Vector2);
            for (int k = 0; k < agents.Count; k++)
            {
                IAgent agent = agents[k];
                Vector2 position = agent.Position;
                Vector2 vector = Vector2.ClampMagnitude(agent.CalculatedTargetPoint - position, agent.CalculatedSpeed * Time.deltaTime);
                position = (agent.Position = position + vector);
                agent.ElevationCoordinate = 0f;
                Vector3 vector3 = goals[k];
                float x = vector3.x;
                Vector3 vector4 = goals[k];
                vector5 = new Vector2(x, vector4.z);
                float magnitude = (vector5 - position).magnitude;
                agent.SetTarget(vector5, Mathf.Min(magnitude, maxSpeed), maxSpeed * 1.1f);
                interpolatedVelocities[k] += vector;
                if (interpolatedVelocities[k].magnitude > maxSpeed * 0.1f)
                {
                    interpolatedVelocities[k] = Vector2.ClampMagnitude(interpolatedVelocities[k], maxSpeed * 0.1f);
                    interpolatedRotations[k] = Vector2.Lerp(interpolatedRotations[k], interpolatedVelocities[k], agent.CalculatedSpeed * Time.deltaTime * 4f);
                }
                Vector3 vector6 = new Vector3(interpolatedRotations[k].x, 0f, interpolatedRotations[k].y).normalized * agent.Radius;
                if (vector6 == Vector3.zero)
                {
                    vector6 = new Vector3(0f, 0f, agent.Radius);
                }
                Vector3 b = Vector3.Cross(Vector3.up, vector6);
                Vector2 position2 = agent.Position;
                float x2 = position2.x;
                float elevationCoordinate = agent.ElevationCoordinate;
                Vector2 position3 = agent.Position;
                Vector3 a = new Vector3(x2, elevationCoordinate, position3.y) + renderingOffset;
                int num = 4 * k;
                int num2 = 6 * k;
                verts[num] = a + vector6 - b;
                verts[num + 1] = a + vector6 + b;
                verts[num + 2] = a - vector6 + b;
                verts[num + 3] = a - vector6 - b;
                uv[num] = new Vector2(0f, 1f);
                uv[num + 1] = new Vector2(1f, 1f);
                uv[num + 2] = new Vector2(1f, 0f);
                uv[num + 3] = new Vector2(0f, 0f);
                meshColors[num] = colors[k];
                meshColors[num + 1] = colors[k];
                meshColors[num + 2] = colors[k];
                meshColors[num + 3] = colors[k];
                tris[num2] = num;
                tris[num2 + 1] = num + 1;
                tris[num2 + 2] = num + 2;
                tris[num2 + 3] = num;
                tris[num2 + 4] = num + 2;
                tris[num2 + 5] = num + 3;
            }
            mesh.Clear();
            mesh.vertices = verts;
            mesh.uv = uv;
            mesh.colors = meshColors;
            mesh.triangles = tris;
            mesh.RecalculateNormals();
        }
    }
}
