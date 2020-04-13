using UnityEngine;

namespace Pathfinding.Examples
{
    [RequireComponent(typeof(Seeker))]
    [HelpURL("http://arongranberg.com/astar/docs/class_pathfinding_1_1_examples_1_1_mine_bot_a_i.php")]
    public class MineBotAI : AIPath
    {
        public Animation anim;

        public float sleepVelocity = 0.4f;

        public float animationSpeed = 0.2f;

        public GameObject endOfPathEffect;

        protected Vector3 lastTarget;

        public new void Start()
        {
            anim["forward"].layer = 10;
            anim.Play("awake");
            anim.Play("forward");
            anim["awake"].wrapMode = WrapMode.Once;
            anim["awake"].speed = 0f;
            anim["awake"].normalizedTime = 1f;
            base.Start();
        }

        public override void OnTargetReached()
        {
            if (endOfPathEffect != null && Vector3.Distance(tr.position, lastTarget) > 1f)
            {
                Object.Instantiate(endOfPathEffect, tr.position, tr.rotation);
                lastTarget = tr.position;
            }
        }

        public override Vector3 GetFeetPosition()
        {
            return tr.position;
        }

        protected new void Update()
        {
            Vector3 direction;
            if (canMove)
            {
                Vector3 vector = CalculateVelocity(GetFeetPosition());
                RotateTowards(targetDirection);
                vector.y = 0f;
                if (!(vector.sqrMagnitude > sleepVelocity * sleepVelocity))
                {
                    vector = Vector3.zero;
                }
                if (rvoController != null)
                {
                    rvoController.Move(vector);
                    direction = rvoController.velocity;
                }
                else if (controller != null)
                {
                    controller.SimpleMove(vector);
                    direction = controller.velocity;
                }
                else
                {
                    Debug.LogWarning("No NavmeshController or CharacterController attached to GameObject");
                    direction = Vector3.zero;
                }
            }
            else
            {
                direction = Vector3.zero;
            }
            Vector3 vector2 = tr.InverseTransformDirection(direction);
            vector2.y = 0f;
            if (direction.sqrMagnitude <= sleepVelocity * sleepVelocity)
            {
                anim.Blend("forward", 0f, 0.2f);
                return;
            }
            anim.Blend("forward", 1f, 0.2f);
            AnimationState animationState = anim["forward"];
            float z = vector2.z;
            animationState.speed = z * animationSpeed;
        }
    }
}
