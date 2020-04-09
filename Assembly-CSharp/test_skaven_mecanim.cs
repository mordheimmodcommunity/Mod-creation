using UnityEngine;

public class test_skaven_mecanim : MonoBehaviour
{
	protected Animator animator;

	public float turnSmoothing = 55f;

	public float speedDampTime = 0.1f;

	private void Start()
	{
		animator = GetComponent<Animator>();
	}

	private void FixedUpdate()
	{
		float axis = PandoraSingleton<PandoraInput>.Instance.GetAxis("h");
		float axis2 = PandoraSingleton<PandoraInput>.Instance.GetAxis("v");
		if (axis != 0f || axis2 != 0f)
		{
			Vector3 forward = Camera.main.transform.forward;
			forward.y = 0f;
			forward.Normalize();
			forward *= axis2;
			Vector3 right = Camera.main.transform.right;
			right.y = 0f;
			right.Normalize();
			right *= axis;
			Vector3 forward2 = forward + right;
			Quaternion b = Quaternion.LookRotation(forward2, Vector3.up);
			Quaternion rot = Quaternion.Lerp(GetComponent<Rigidbody>().rotation, b, turnSmoothing * Time.deltaTime);
			GetComponent<Rigidbody>().MoveRotation(rot);
			animator.SetFloat("speed", axis2, speedDampTime, Time.deltaTime);
		}
		else
		{
			animator.SetFloat("speed", 0f);
		}
	}
}
