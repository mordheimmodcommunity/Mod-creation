using UnityEngine;

public class VerletCloth : MonoBehaviour
{
	public ClothKnot[] knots;

	public ClothThread[] threads;

	public ClothLock[] locks;

	public Mesh mesh;

	public GameObject colliderRoot;

	public float gravityDiv = 10f;

	public float moveDiv = 100f;

	private ArachneCollider[] colliders;

	private Vector3[] prevCollidersPos;

	private Mesh tMesh;

	public Transform rootBone;

	public Vector3 prevBonePos;

	public Vector3 prevRootPos;

	private Bounds bounds;

	private bool wasSequence;

	private void Start()
	{
		wasSequence = false;
		tMesh = new Mesh();
		if (base.transform.parent != null && base.transform.parent.parent != null)
		{
			rootBone = base.transform.parent.parent;
			prevBonePos = rootBone.localPosition;
		}
		if (base.transform.parent != null && base.transform.parent.parent != null && base.transform.parent.parent.parent != null)
		{
			colliderRoot = base.transform.parent.parent.parent.gameObject;
			prevRootPos = colliderRoot.transform.position;
			bounds = default(Bounds);
		}
		MeshFilter component = base.gameObject.GetComponent<MeshFilter>();
		if (component != null)
		{
			mesh = component.mesh;
		}
		else
		{
			SkinnedMeshRenderer component2 = base.gameObject.GetComponent<SkinnedMeshRenderer>();
			component2.BakeMesh(tMesh);
			component = base.gameObject.AddComponent<MeshFilter>();
			component.mesh = tMesh;
			mesh = tMesh;
			MeshRenderer meshRenderer = base.gameObject.AddComponent<MeshRenderer>();
			meshRenderer.sharedMaterial = component2.sharedMaterial;
			Object.Destroy(component2);
			for (int i = 0; i < knots.Length; i++)
			{
				knots[i].curPos = tMesh.vertices[knots[i].meshVertIdx];
				knots[i].oldPos = knots[i].curPos;
				knots[i].orgPos = knots[i].curPos;
			}
			for (int j = 0; j < locks.Length; j++)
			{
				locks[j].orgPos = tMesh.vertices[knots[locks[j].knot].meshVertIdx];
			}
		}
		mesh.MarkDynamic();
		colliders = colliderRoot.GetComponentsInChildren<ArachneCollider>(includeInactive: true);
		prevCollidersPos = new Vector3[colliders.Length];
		Debug.Log("Colliders " + colliders.Length);
		Renderer[] componentsInChildren = GetComponentsInChildren<Renderer>();
		Renderer[] array = componentsInChildren;
		foreach (Renderer renderer in array)
		{
			renderer.material.color = renderer.sharedMaterial.color;
		}
	}

	private void OnBecameVisible()
	{
		base.enabled = true;
	}

	private void OnBecameInvisible()
	{
		base.enabled = false;
	}

	private void Reset()
	{
		for (int i = 0; i < knots.Length; i++)
		{
			knots[i].curPos = knots[i].orgPos;
			knots[i].oldPos = knots[i].orgPos;
		}
	}

	private void FixedUpdate()
	{
		if (PandoraSingleton<SequenceManager>.Exists())
		{
			if (wasSequence && !PandoraSingleton<SequenceManager>.Instance.isPlaying)
			{
				Reset();
				wasSequence = false;
			}
			else if (PandoraSingleton<SequenceManager>.Instance.isPlaying)
			{
				wasSequence = true;
			}
		}
		Vector3 localPosition = rootBone.localPosition;
		Vector3 position = colliderRoot.transform.position;
		Vector3 b = position - prevRootPos;
		Vector3 a = rootBone.InverseTransformDirection(b.normalized);
		prevRootPos = position;
		prevBonePos = localPosition;
		Vector3 a2 = rootBone.InverseTransformDirection(Physics.gravity);
		for (int i = 0; i < knots.Length; i++)
		{
			Vector3 oldPos = knots[i].oldPos;
			Vector3 curPos = knots[i].curPos;
			curPos = ((!knots[i].worldMove || wasSequence) ? (curPos * 2f - oldPos + a / (moveDiv * 2f) + a2 / gravityDiv * Time.deltaTime * Time.deltaTime) : (curPos * 2f - oldPos - a / moveDiv + a2 / gravityDiv * Time.deltaTime * Time.deltaTime));
			knots[i].oldPos = knots[i].curPos;
			knots[i].curPos = curPos;
		}
		for (int j = 0; j < locks.Length; j++)
		{
			knots[locks[j].knot].curPos = locks[j].orgPos;
		}
		for (int k = 0; k < 1; k++)
		{
			for (int l = 0; l < threads.Length; l++)
			{
				Vector3 curPos2 = knots[threads[l].knot1].curPos;
				Vector3 curPos3 = knots[threads[l].knot2].curPos;
				Vector3 a3 = curPos3 - curPos2;
				float num = Vector3.Distance(curPos3, curPos2);
				float d = (num - threads[l].restLength) / num;
				curPos2 += a3 * 0.5f * d;
				curPos3 -= a3 * 0.5f * d;
				knots[threads[l].knot1].curPos = curPos2;
				knots[threads[l].knot2].curPos = curPos3;
			}
			for (int m = 0; m < locks.Length; m++)
			{
				knots[locks[m].knot].curPos = locks[m].orgPos;
			}
		}
		for (int n = 0; n < colliders.Length; n++)
		{
			float radius = colliders[n].radius;
			float num2 = radius * radius;
			Vector3 position2 = colliders[n].position;
			for (int num3 = 0; num3 < knots.Length; num3++)
			{
				Vector3 a4 = base.transform.TransformPoint(knots[num3].curPos) - position2;
				float num4 = Vector3.SqrMagnitude(a4);
				if (!(num2 > num4))
				{
					continue;
				}
				Vector3 vector = position2 - prevCollidersPos[n] - b;
				if (!wasSequence && !knots[num3].worldMove)
				{
					if (vector.y < 0f)
					{
						vector.y = 0f;
					}
					a4 += vector;
				}
				Vector3 position3 = position2 + a4.normalized * radius;
				knots[num3].curPos = base.transform.InverseTransformPoint(position3);
				knots[num3].oldPos = knots[num3].curPos;
			}
			prevCollidersPos[n] = position2;
		}
		for (int num5 = 0; num5 < locks.Length; num5++)
		{
			knots[locks[num5].knot].curPos = locks[num5].orgPos;
		}
		Vector3[] vertices = mesh.vertices;
		for (int num6 = 0; num6 < knots.Length; num6++)
		{
			vertices[knots[num6].meshVertIdx] = knots[num6].curPos;
		}
		mesh.vertices = vertices;
		mesh.RecalculateNormals();
	}
}
