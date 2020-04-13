using Pathfinding.RVO;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pathfinding
{
    [HelpURL("http://arongranberg.com/astar/docs/class_pathfinding_1_1_rich_a_i.php")]
    [RequireComponent(typeof(Seeker))]
    [AddComponentMenu("Pathfinding/AI/RichAI (3D, for navmesh)")]
    public class RichAI : MonoBehaviour
    {
        public Transform target;

        public bool drawGizmos = true;

        public bool repeatedlySearchPaths;

        public float repathRate = 0.5f;

        public float maxSpeed = 1f;

        public float acceleration = 5f;

        public float slowdownTime = 0.5f;

        public float rotationSpeed = 360f;

        public float endReachedDistance = 0.01f;

        public float wallForce = 3f;

        public float wallDist = 1f;

        public Vector3 gravity = new Vector3(0f, -9.82f, 0f);

        public bool raycastingForGroundPlacement = true;

        public LayerMask groundMask = -1;

        public float centerOffset = 1f;

        public RichFunnel.FunnelSimplification funnelSimplification;

        public Animation anim;

        public bool preciseSlowdown = true;

        public bool slowWhenNotFacingTarget = true;

        private Vector3 velocity;

        protected RichPath rp;

        protected Seeker seeker;

        protected Transform tr;

        private CharacterController controller;

        private RVOController rvoController;

        private Vector3 lastTargetPoint;

        private Vector3 currentTargetDirection;

        protected bool waitingForPathCalc;

        protected bool canSearchPath;

        protected bool delayUpdatePath;

        protected bool traversingSpecialPath;

        protected bool lastCorner;

        private float distanceToWaypoint = 999f;

        protected List<Vector3> nextCorners = new List<Vector3>();

        protected List<Vector3> wallBuffer = new List<Vector3>();

        private bool startHasRun;

        protected float lastRepath = -9999f;

        private static float deltaTime;

        public static readonly Color GizmoColorRaycast = new Color(118f / 255f, 206f / 255f, 112f / 255f);

        public static readonly Color GizmoColorPath = new Color(8f / 255f, 26f / 85f, 194f / 255f);

        public Vector3 Velocity => velocity;

        public bool TraversingSpecial => traversingSpecialPath;

        public Vector3 TargetPoint => lastTargetPoint;

        public bool ApproachingPartEndpoint => lastCorner;

        public bool ApproachingPathEndpoint => rp != null && ApproachingPartEndpoint && !rp.PartsLeft();

        public float DistanceToNextWaypoint => distanceToWaypoint;

        private void Awake()
        {
            seeker = GetComponent<Seeker>();
            controller = GetComponent<CharacterController>();
            rvoController = GetComponent<RVOController>();
            tr = base.transform;
        }

        protected virtual void Start()
        {
            startHasRun = true;
            OnEnable();
        }

        protected virtual void OnEnable()
        {
            lastRepath = -9999f;
            waitingForPathCalc = false;
            canSearchPath = true;
            if (startHasRun)
            {
                Seeker obj = seeker;
                obj.pathCallback = (OnPathDelegate)Delegate.Combine(obj.pathCallback, new OnPathDelegate(OnPathComplete));
                StartCoroutine(SearchPaths());
            }
        }

        public void OnDisable()
        {
            if (seeker != null && !seeker.IsDone())
            {
                seeker.GetCurrentPath().Error();
            }
            Seeker obj = seeker;
            obj.pathCallback = (OnPathDelegate)Delegate.Remove(obj.pathCallback, new OnPathDelegate(OnPathComplete));
        }

        public virtual void UpdatePath()
        {
            canSearchPath = true;
            waitingForPathCalc = false;
            Path currentPath = seeker.GetCurrentPath();
            if (currentPath != null && !seeker.IsDone())
            {
                currentPath.Error();
                currentPath.Claim(this);
                currentPath.Release(this);
            }
            waitingForPathCalc = true;
            lastRepath = Time.time;
            seeker.StartPath(tr.position, target.position);
        }

        private IEnumerator SearchPaths()
        {
            while (true)
            {
                if (!repeatedlySearchPaths || waitingForPathCalc || !canSearchPath || Time.time - lastRepath < repathRate)
                {
                    yield return null;
                    continue;
                }
                UpdatePath();
                yield return null;
            }
        }

        private void OnPathComplete(Path p)
        {
            waitingForPathCalc = false;
            p.Claim(this);
            if (p.error)
            {
                p.Release(this);
                return;
            }
            if (traversingSpecialPath)
            {
                delayUpdatePath = true;
            }
            else
            {
                if (rp == null)
                {
                    rp = new RichPath();
                }
                rp.Initialize(seeker, p, mergePartEndpoints: true, funnelSimplification);
            }
            p.Release(this);
        }

        private void NextPart()
        {
            rp.NextPart();
            lastCorner = false;
            if (!rp.PartsLeft())
            {
                OnTargetReached();
            }
        }

        protected virtual void OnTargetReached()
        {
        }

        protected virtual Vector3 UpdateTarget(RichFunnel fn)
        {
            nextCorners.Clear();
            Vector3 position = tr.position;
            position = fn.Update(position, nextCorners, 2, out lastCorner, out bool requiresRepath);
            if (requiresRepath && !waitingForPathCalc)
            {
                UpdatePath();
            }
            return position;
        }

        private static Vector2 To2D(Vector3 v)
        {
            return new Vector2(v.x, v.z);
        }

        protected virtual void Update()
        {
            deltaTime = Mathf.Min(Time.smoothDeltaTime * 2f, Time.deltaTime);
            if (rp != null)
            {
                RichPathPart currentPart = rp.GetCurrentPart();
                RichFunnel richFunnel = currentPart as RichFunnel;
                if (richFunnel != null)
                {
                    Vector3 vector = UpdateTarget(richFunnel);
                    if (Time.frameCount % 5 == 0 && wallForce > 0f && wallDist > 0f)
                    {
                        wallBuffer.Clear();
                        richFunnel.FindWalls(wallBuffer, wallDist);
                    }
                    int num = 0;
                    Vector3 vector2 = nextCorners[num];
                    Vector3 vector3 = vector2 - vector;
                    vector3.y = 0f;
                    if (Vector3.Dot(vector3, currentTargetDirection) < 0f && nextCorners.Count - num > 1)
                    {
                        num++;
                        vector2 = nextCorners[num];
                    }
                    if (vector2 != lastTargetPoint)
                    {
                        currentTargetDirection = vector2 - vector;
                        currentTargetDirection.y = 0f;
                        currentTargetDirection.Normalize();
                        lastTargetPoint = vector2;
                    }
                    vector3 = vector2 - vector;
                    vector3.y = 0f;
                    Vector3 vector4 = VectorMath.Normalize(vector3, out distanceToWaypoint);
                    bool flag = lastCorner && nextCorners.Count - num == 1;
                    if (flag && distanceToWaypoint < 0.01f * maxSpeed)
                    {
                        velocity = (vector2 - vector) * 100f;
                    }
                    else
                    {
                        Vector3 a = CalculateWallForce(vector, vector4);
                        Vector2 vector5;
                        if (flag)
                        {
                            vector5 = CalculateAccelerationToReachPoint(To2D(vector2 - vector), Vector2.zero, To2D(velocity));
                            a *= Math.Min(distanceToWaypoint / 0.5f, 1f);
                            if (distanceToWaypoint < endReachedDistance)
                            {
                                NextPart();
                            }
                        }
                        else
                        {
                            Vector3 a2 = (num >= nextCorners.Count - 1) ? ((vector2 - vector) * 2f + vector) : nextCorners[num + 1];
                            Vector3 v = (a2 - vector2).normalized * maxSpeed;
                            vector5 = CalculateAccelerationToReachPoint(To2D(vector2 - vector), To2D(v), To2D(velocity));
                        }
                        velocity += (new Vector3(vector5.x, 0f, vector5.y) + a * wallForce) * deltaTime;
                    }
                    Vector3 b = richFunnel.CurrentNode?.ClosestPointOnNode(vector) ?? vector;
                    float magnitude = (richFunnel.exactEnd - b).magnitude;
                    float num2 = maxSpeed;
                    num2 *= Mathf.Sqrt(Mathf.Min(1f, magnitude / (maxSpeed * slowdownTime)));
                    if (slowWhenNotFacingTarget)
                    {
                        float num3 = Mathf.Max((Vector3.Dot(vector4, tr.forward) + 0.5f) / 1.5f, 0.2f);
                        num2 *= num3;
                        float a3 = VectorMath.MagnitudeXZ(velocity);
                        float y = velocity.y;
                        velocity.y = 0f;
                        a3 = Mathf.Min(a3, num2);
                        velocity = Vector3.Lerp(velocity.normalized * a3, tr.forward * a3, Mathf.Clamp((!flag) ? 1f : (distanceToWaypoint * 2f), 0f, 0.5f));
                        velocity.y = y;
                    }
                    else
                    {
                        velocity = VectorMath.ClampMagnitudeXZ(velocity, num2);
                    }
                    velocity += deltaTime * gravity;
                    if (rvoController != null && rvoController.enabled)
                    {
                        Vector3 pos = vector + VectorMath.ClampMagnitudeXZ(velocity, magnitude);
                        rvoController.SetTarget(pos, VectorMath.MagnitudeXZ(velocity), maxSpeed);
                    }
                    Vector3 vector6;
                    if (rvoController != null && rvoController.enabled)
                    {
                        vector6 = rvoController.CalculateMovementDelta(vector, deltaTime);
                        vector6.y = velocity.y * deltaTime;
                    }
                    else
                    {
                        vector6 = velocity * deltaTime;
                    }
                    if (flag)
                    {
                        Vector3 trotdir = Vector3.Lerp(vector6.normalized, currentTargetDirection, Math.Max(1f - distanceToWaypoint * 2f, 0f));
                        RotateTowards(trotdir);
                    }
                    else
                    {
                        RotateTowards(vector6);
                    }
                    if (controller != null && controller.enabled)
                    {
                        tr.position = vector;
                        controller.Move(vector6);
                        vector = tr.position;
                    }
                    else
                    {
                        float y2 = vector.y;
                        vector += vector6;
                        vector = RaycastPosition(vector, y2);
                    }
                    Vector3 vector7 = richFunnel.ClampToNavmesh(vector);
                    if (vector != vector7)
                    {
                        Vector3 vector8 = vector7 - vector;
                        velocity -= vector8 * Vector3.Dot(vector8, velocity) / vector8.sqrMagnitude;
                        if (rvoController != null && rvoController.enabled)
                        {
                            rvoController.SetCollisionNormal(vector8);
                        }
                    }
                    tr.position = vector7;
                }
                else if (rvoController != null && rvoController.enabled)
                {
                    rvoController.Move(Vector3.zero);
                }
                if (currentPart is RichSpecial && !traversingSpecialPath)
                {
                    StartCoroutine(TraverseSpecial(currentPart as RichSpecial));
                }
            }
            else if (rvoController != null && rvoController.enabled)
            {
                rvoController.Move(Vector3.zero);
            }
            else if (!(controller != null) || !controller.enabled)
            {
                Transform transform = tr;
                Vector3 position = tr.position;
                Vector3 position2 = tr.position;
                transform.position = RaycastPosition(position, position2.y);
            }
        }

        private Vector2 CalculateAccelerationToReachPoint(Vector2 deltaPosition, Vector2 targetVelocity, Vector2 currentVelocity)
        {
            if (targetVelocity == Vector2.zero)
            {
                float num = 0.05f;
                float num2 = 10f;
                while (num2 - num > 0.01f)
                {
                    float num3 = (num2 + num) * 0.5f;
                    Vector2 a = (6f * deltaPosition - 4f * num3 * currentVelocity) / (num3 * num3);
                    Vector2 a2 = 6f * (num3 * currentVelocity - 2f * deltaPosition) / (num3 * num3 * num3);
                    if (a.sqrMagnitude > acceleration * acceleration || (a + a2 * num3).sqrMagnitude > acceleration * acceleration)
                    {
                        num = num3;
                    }
                    else
                    {
                        num2 = num3;
                    }
                }
                return (6f * deltaPosition - 4f * num2 * currentVelocity) / (num2 * num2);
            }
            float magnitude = deltaPosition.magnitude;
            float magnitude2 = currentVelocity.magnitude;
            float magnitude3;
            Vector2 a3 = VectorMath.Normalize(targetVelocity, out magnitude3);
            return (deltaPosition - a3 * Math.Min(0.5f * magnitude * magnitude3 / (magnitude2 + magnitude3), maxSpeed * 2f)).normalized * acceleration;
        }

        private Vector3 CalculateWallForce(Vector3 position, Vector3 directionToTarget)
        {
            if (wallForce > 0f && wallDist > 0f)
            {
                float num = 0f;
                float num2 = 0f;
                for (int i = 0; i < wallBuffer.Count; i += 2)
                {
                    Vector3 a = VectorMath.ClosestPointOnSegment(wallBuffer[i], wallBuffer[i + 1], tr.position);
                    float sqrMagnitude = (a - position).sqrMagnitude;
                    if (!(sqrMagnitude > wallDist * wallDist))
                    {
                        Vector3 normalized = (wallBuffer[i + 1] - wallBuffer[i]).normalized;
                        float num3 = Vector3.Dot(directionToTarget, normalized) * (1f - Math.Max(0f, 2f * (sqrMagnitude / (wallDist * wallDist)) - 1f));
                        if (num3 > 0f)
                        {
                            num2 = Math.Max(num2, num3);
                        }
                        else
                        {
                            num = Math.Max(num, 0f - num3);
                        }
                    }
                }
                Vector3 a2 = Vector3.Cross(Vector3.up, directionToTarget);
                return a2 * (num2 - num);
            }
            return Vector3.zero;
        }

        private Vector3 RaycastPosition(Vector3 position, float lasty)
        {
            if (raycastingForGroundPlacement)
            {
                float num = Mathf.Max(centerOffset, lasty - position.y + centerOffset);
                if (Physics.Raycast(position + Vector3.up * num, Vector3.down, out RaycastHit hitInfo, num, groundMask) && hitInfo.distance < num)
                {
                    position = hitInfo.point;
                    velocity.y = 0f;
                }
            }
            return position;
        }

        private bool RotateTowards(Vector3 trotdir)
        {
            trotdir.y = 0f;
            if (trotdir != Vector3.zero)
            {
                Quaternion rotation = tr.rotation;
                Vector3 eulerAngles = Quaternion.LookRotation(trotdir).eulerAngles;
                Vector3 eulerAngles2 = rotation.eulerAngles;
                eulerAngles2.y = Mathf.MoveTowardsAngle(eulerAngles2.y, eulerAngles.y, rotationSpeed * deltaTime);
                tr.rotation = Quaternion.Euler(eulerAngles2);
                return Mathf.Abs(eulerAngles2.y - eulerAngles.y) < 5f;
            }
            return false;
        }

        public void OnDrawGizmos()
        {
            if (!drawGizmos)
            {
                return;
            }
            if (raycastingForGroundPlacement)
            {
                Gizmos.color = GizmoColorRaycast;
                Gizmos.DrawLine(base.transform.position, base.transform.position + Vector3.up * centerOffset);
                Gizmos.DrawLine(base.transform.position + Vector3.left * 0.1f, base.transform.position + Vector3.right * 0.1f);
                Gizmos.DrawLine(base.transform.position + Vector3.back * 0.1f, base.transform.position + Vector3.forward * 0.1f);
            }
            if (tr != null && nextCorners != null)
            {
                Gizmos.color = GizmoColorPath;
                Vector3 from = tr.position;
                for (int i = 0; i < nextCorners.Count; i++)
                {
                    Gizmos.DrawLine(from, nextCorners[i]);
                    from = nextCorners[i];
                }
            }
        }

        private IEnumerator TraverseSpecial(RichSpecial rs)
        {
            traversingSpecialPath = true;
            velocity = Vector3.zero;
            AnimationLink al = rs.nodeLink as AnimationLink;
            if (al == null)
            {
                Debug.LogError("Unhandled RichSpecial");
                yield break;
            }
            while (!RotateTowards(rs.first.forward))
            {
                yield return null;
            }
            tr.parent.position = tr.position;
            tr.parent.rotation = tr.rotation;
            tr.localPosition = Vector3.zero;
            tr.localRotation = Quaternion.identity;
            if (rs.reverse && al.reverseAnim)
            {
                anim[al.clip].speed = 0f - al.animSpeed;
                anim[al.clip].normalizedTime = 1f;
                anim.Play(al.clip);
                anim.Sample();
            }
            else
            {
                anim[al.clip].speed = al.animSpeed;
                anim.Rewind(al.clip);
                anim.Play(al.clip);
            }
            tr.parent.position -= tr.position - tr.parent.position;
            yield return new WaitForSeconds(Mathf.Abs(anim[al.clip].length / al.animSpeed));
            traversingSpecialPath = false;
            NextPart();
            if (delayUpdatePath)
            {
                delayUpdatePath = false;
                UpdatePath();
            }
        }
    }
}
