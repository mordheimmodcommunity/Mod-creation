using UnityEngine;

public class Crow : MonoBehaviour
{
    private Animator animator;

    private bool flying;

    private int collisionMask;

    private Collider collider;

    private float time;

    private void Start()
    {
        collisionMask = LayerMask.NameToLayer("trigger_collision");
        animator = GetComponent<Animator>();
        collider = GetComponent<Collider>();
        flying = false;
        time = (float)PandoraSingleton<GameManager>.Instance.LocalTyche.Rand(2.0, 6.0);
    }

    private void Update()
    {
        if (flying)
        {
            base.transform.position += (2f * Vector3.up + base.transform.forward) * Time.deltaTime * 2f;
            Vector3 position = base.transform.position;
            if (position.y > 40f)
            {
                Object.Destroy(base.gameObject);
            }
        }
        else if (time > 0f)
        {
            time -= Time.deltaTime;
        }
        else
        {
            animator.SetBool("feasting", !animator.GetBool("feasting"));
            time = (float)PandoraSingleton<GameManager>.Instance.LocalTyche.Rand(2.0, 6.0);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == collisionMask)
        {
            flying = true;
            animator.SetBool("flying", value: true);
            collider.enabled = false;
        }
    }
}
