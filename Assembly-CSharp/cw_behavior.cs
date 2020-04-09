using UnityEngine;

public class cw_behavior : MonoBehaviour
{
	private const float m_DirectionDampTime = 0.25f;

	private const float m_SpeedDampTime = 0.25f;

	public GameObject[] cw_foodpts;

	public GameObject[] cw_freepts;

	public float timetoSelectAgain;

	public Transform target;

	public bool FoundFoodTarget;

	public bool FoundFreeTarget;

	public bool PointReached;

	private Animator animator;

	private void Start()
	{
		PointReached = false;
		animator = GetComponent<Animator>();
		GetComponent<Rigidbody>().isKinematic = true;
		cw_freepts = GameObject.FindGameObjectsWithTag("cw_freepts");
		cw_foodpts = GameObject.FindGameObjectsWithTag("cw_foodpts");
		FoundFoodTarget = false;
		FoundFreeTarget = false;
	}

	private void SetFoodTarget()
	{
		int num = Random.Range(0, cw_foodpts.Length - 1);
		target = cw_foodpts[num].transform;
		FoundFoodTarget = true;
	}

	private void SetFreeTarget()
	{
		int num = Random.Range(0, cw_freepts.Length - 1);
		target = cw_freepts[num].transform;
		FoundFreeTarget = true;
	}

	private void Update()
	{
		timetoSelectAgain += Time.deltaTime;
		int num = Random.Range(16, 50);
		if (timetoSelectAgain > (float)num)
		{
			if (!FoundFoodTarget)
			{
				SetFoodTarget();
			}
			base.transform.LookAt(target);
			if (Vector3.Distance(target.position, animator.rootPosition) > 0.75f)
			{
				PointReached = false;
				animator.SetBool("feasting", value: false);
				if (Vector3.Distance(target.position, animator.rootPosition) > 15f)
				{
					animator.SetFloat("Speed", 3f, 0.25f, Time.deltaTime);
				}
				if (Vector3.Distance(target.position, animator.rootPosition) > 10f && Vector3.Distance(target.position, animator.rootPosition) < 15f)
				{
					animator.SetFloat("Speed", 1.5f, 0.25f, Time.deltaTime);
				}
				if (Vector3.Distance(target.position, animator.rootPosition) < 10f)
				{
					animator.SetFloat("Speed", 0.5f, 0.25f, Time.deltaTime);
				}
				GetComponent<Rigidbody>().isKinematic = true;
			}
			else
			{
				animator.SetFloat("Speed", 0f, 0.25f, Time.deltaTime);
				base.transform.LookAt(target);
				animator.SetBool("feasting", value: true);
				if (!PointReached)
				{
					Transform transform = base.transform;
					Vector3 position = target.position;
					float x = position.x;
					Vector3 position2 = target.position;
					float y = position2.y;
					Vector3 position3 = target.position;
					transform.position = new Vector3(x, y, position3.z);
					PointReached = true;
				}
				GetComponent<Rigidbody>().isKinematic = false;
			}
		}
		if (timetoSelectAgain < 15f)
		{
			if (!FoundFreeTarget)
			{
				SetFreeTarget();
			}
			base.transform.LookAt(target);
			if (Vector3.Distance(target.position, animator.rootPosition) > 0.5f)
			{
				PointReached = false;
				animator.SetBool("feasting", value: false);
				if (Vector3.Distance(target.position, animator.rootPosition) > 15f)
				{
					animator.SetFloat("Speed", 3f, 0.25f, Time.deltaTime);
				}
				if (Vector3.Distance(target.position, animator.rootPosition) > 10f && Vector3.Distance(target.position, animator.rootPosition) < 15f)
				{
					animator.SetFloat("Speed", 1.5f, 0.25f, Time.deltaTime);
				}
				if (Vector3.Distance(target.position, animator.rootPosition) < 10f)
				{
					animator.SetFloat("Speed", 0.5f, 0.25f, Time.deltaTime);
				}
				GetComponent<Rigidbody>().isKinematic = true;
			}
			else
			{
				animator.SetFloat("Speed", 0f, 0.25f, Time.deltaTime);
				base.transform.LookAt(target);
				if (!PointReached)
				{
					Transform transform2 = base.transform;
					Vector3 position4 = target.position;
					float x2 = position4.x;
					Vector3 position5 = target.position;
					float y2 = position5.y;
					Vector3 position6 = target.position;
					transform2.position = new Vector3(x2, y2, position6.z);
					PointReached = true;
				}
				GetComponent<Rigidbody>().isKinematic = false;
			}
		}
		int num2 = Random.Range(80, 120);
		if (timetoSelectAgain > (float)num2)
		{
			timetoSelectAgain = 0f;
			FoundFoodTarget = false;
			FoundFreeTarget = false;
		}
		if (Vector3.Distance(target.position, animator.rootPosition) < 0.2f)
		{
			GetComponent<Rigidbody>().isKinematic = false;
		}
	}
}
