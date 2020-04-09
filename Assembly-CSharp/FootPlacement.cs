using UnityEngine;

public class FootPlacement : MonoBehaviour
{
	private float leftWeight = 1f;

	private float rightWeight = 1f;

	public float weight;

	public Animator animator;

	public AnimationClip[] Idles;

	public Transform leftBone;

	public Transform rightBone;

	private float leftRot;

	private float rightRot;

	private string firstFoot;

	private string secondFoot;

	private string earthFoot;

	private string airFoot;

	public void FixFoot(AvatarIKGoal footGoal, RaycastHit hit, float _weight)
	{
		animator.GetComponent<Rigidbody>().useGravity = !animator.GetComponent<Rigidbody>().useGravity;
		CapsuleCollider component = animator.GetComponent<CapsuleCollider>();
		if (!hit.collider.Equals(component))
		{
			if (hit.distance < 1f + weight)
			{
				_weight = 1f;
			}
			animator.SetIKPosition(footGoal, hit.point + Vector3.up * weight);
			animator.SetIKPositionWeight(footGoal, _weight);
			Quaternion iKRotation = animator.GetIKRotation(footGoal);
			Vector3 fromDirection = iKRotation * Vector3.up;
			iKRotation = Quaternion.FromToRotation(fromDirection, hit.normal) * iKRotation;
			animator.SetIKRotation(footGoal, iKRotation);
			animator.SetIKRotationWeight(footGoal, _weight);
		}
	}

	private void Start()
	{
		Vector3 eulerAngles = leftBone.eulerAngles;
		leftRot = eulerAngles.x;
		if (leftRot > 180f)
		{
			leftRot -= 180f;
		}
		else
		{
			leftRot += 180f;
		}
		Vector3 eulerAngles2 = rightBone.eulerAngles;
		rightRot = eulerAngles2.x;
		if (rightRot > 180f)
		{
			rightRot -= 180f;
		}
		else
		{
			rightRot += 180f;
		}
	}

	public void legPosition()
	{
		Vector3 eulerAngles = leftBone.eulerAngles;
		float x = eulerAngles.x;
		x = ((!(x > 180f)) ? (x + 180f) : (x - 180f));
		Vector3 eulerAngles2 = rightBone.eulerAngles;
		float x2 = eulerAngles2.x;
		x2 = ((!(x2 > 180f)) ? (x2 + 180f) : (x2 - 180f));
		if (leftRot < rightRot)
		{
			firstFoot = "left";
			secondFoot = "right";
		}
		else if (leftRot > rightRot)
		{
			firstFoot = "right";
			secondFoot = "left";
		}
		if (leftRot < x && firstFoot == "left")
		{
			earthFoot = "left";
			airFoot = "right";
		}
		else if (leftRot > x && firstFoot == "left")
		{
			earthFoot = "right";
			airFoot = "left";
		}
		else if (rightRot < x2 && firstFoot == "right")
		{
			earthFoot = "right";
			airFoot = "left";
		}
		else if (rightRot > x2 && firstFoot == "right")
		{
			earthFoot = "left";
			airFoot = "right";
		}
		leftRot = x;
		rightRot = x2;
	}

	public void FixCollider(RaycastHit Lhit, RaycastHit Rhit, bool Idle)
	{
		legPosition();
		CapsuleCollider component = animator.GetComponent<CapsuleCollider>();
		if (Lhit.collider.Equals(component) || Rhit.collider.Equals(component))
		{
			return;
		}
		float num = Vector3.Angle(Vector3.up, Lhit.normal);
		float num2 = Vector3.Angle(Vector3.up, Rhit.normal);
		Vector3 center = component.center;
		Vector3 point = Lhit.point;
		float y = point.y;
		Vector3 point2 = Rhit.point;
		float num3 = Mathf.Abs(y - point2.y);
		float num4 = 0f;
		if (num == 0f && num2 == 0f)
		{
			if (Idle)
			{
				num4 = 1.05f + num3;
			}
			else if (num3 > 0f)
			{
				if (secondFoot == earthFoot && firstFoot == airFoot)
				{
					animator.GetComponent<Rigidbody>().constraints = (RigidbodyConstraints)116;
					num4 = 1.05f;
				}
				else if (firstFoot == earthFoot && secondFoot == airFoot)
				{
					animator.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotation;
					num4 = 1.05f;
				}
				if (earthFoot == "left" && Lhit.distance > 1.05f)
				{
					num4 += Lhit.distance - 1.05f;
				}
				else if (earthFoot == "right" && Rhit.distance > 1.05f)
				{
					num4 += Rhit.distance - 1.05f;
				}
			}
			else
			{
				animator.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotation;
				num4 = 1.05f;
			}
		}
		else
		{
			num4 = 1.05f + num3 * ((num + num2) / 360f);
		}
		if (Mathf.Abs(center.y - num4) < num4 / 100f)
		{
			center.y = num4;
		}
		else if (center.y < num4)
		{
			center.y += num4 / 100f;
		}
		else if (center.y > num4)
		{
			center.y -= num4 / 100f;
		}
		center.y = num4;
		component.center = center;
	}

	public void footPlanting()
	{
		bool flag = false;
		Vector3 origin = animator.GetIKPosition(AvatarIKGoal.LeftFoot) + Vector3.up * 1f;
		Vector3 origin2 = animator.GetIKPosition(AvatarIKGoal.RightFoot) + Vector3.up * 1f;
		RaycastHit hitInfo = default(RaycastHit);
		RaycastHit hitInfo2 = default(RaycastHit);
		if (Physics.Raycast(origin, -Vector3.up, out hitInfo, 2f, LayerMaskManager.footMask) && Physics.Raycast(origin2, -Vector3.up, out hitInfo2, 2f, LayerMaskManager.footMask))
		{
			if (flag)
			{
				leftWeight = (rightWeight = 1f);
			}
			else
			{
				leftWeight = Mathf.Pow(1f - hitInfo.distance, 5f);
				rightWeight = Mathf.Pow(1f - hitInfo2.distance, 5f);
			}
			FixFoot(AvatarIKGoal.LeftFoot, hitInfo, leftWeight);
			FixFoot(AvatarIKGoal.RightFoot, hitInfo2, rightWeight);
			FixCollider(hitInfo, hitInfo2, flag);
		}
	}

	private void OnAnimatorIK(int layerIndex)
	{
		if (layerIndex == 0)
		{
			footPlanting();
		}
	}
}
