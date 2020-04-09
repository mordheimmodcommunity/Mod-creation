using UnityEngine;

[RequireComponent(typeof(TNObject))]
public abstract class TNBehaviour : MonoBehaviour
{
	private TNObject mTNO;

	public TNObject tno
	{
		get
		{
			if (mTNO == null)
			{
				mTNO = GetComponent<TNObject>();
			}
			return mTNO;
		}
	}

	protected virtual void OnEnable()
	{
		if (Application.isPlaying)
		{
			tno.rebuildMethodList = true;
		}
	}

	public void DestroySelf()
	{
		TNManager.Destroy(base.gameObject);
	}
}
