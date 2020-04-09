using UnityEngine;

public class test_network : MonoBehaviour
{
	private void FixedUpdate()
	{
		if (Network.isServer)
		{
			Animator component = GetComponent<Animator>();
			float axis = PandoraSingleton<PandoraInput>.Instance.GetAxis("V");
			float axis2 = PandoraSingleton<PandoraInput>.Instance.GetAxis("H");
			if (axis2 != 0f || axis != 0f)
			{
				Vector3 forward = Camera.main.transform.forward;
				forward.y = 0f;
				forward.Normalize();
				forward *= axis;
				Vector3 right = Camera.main.transform.right;
				right.y = 0f;
				right.Normalize();
				right *= axis2;
				Vector3 forward2 = forward + right;
				Quaternion b = Quaternion.LookRotation(forward2, Vector3.up);
				Quaternion rot = Quaternion.Lerp(GetComponent<Rigidbody>().rotation, b, 7f * Time.fixedDeltaTime);
				GetComponent<Rigidbody>().MoveRotation(rot);
				component.SetFloat(AnimatorIds.speed, forward2.magnitude, 0.1f, Time.fixedDeltaTime);
			}
			else
			{
				component.SetFloat(AnimatorIds.speed, 0f);
			}
		}
	}

	private void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info)
	{
		Animator component = GetComponent<Animator>();
		float value = 0f;
		Quaternion value2 = base.transform.rotation;
		if (stream.isWriting)
		{
			value = component.GetFloat(AnimatorIds.speed);
			stream.Serialize(ref value);
			stream.Serialize(ref value2);
			Vector3 value3 = base.transform.position;
			stream.Serialize(ref value3);
		}
		else
		{
			stream.Serialize(ref value);
			component.SetFloat(AnimatorIds.speed, value);
			stream.Serialize(ref value2);
			base.transform.rotation = value2;
			Vector3 value4 = Vector3.zero;
			stream.Serialize(ref value4);
			base.transform.position = value4;
		}
	}
}
