using UnityEngine;

public class CameraFixed : ICheapState
{
	private CameraManager mngr;

	private Transform dummyCam;

	public CameraFixed(CameraManager camMngr)
	{
		mngr = camMngr;
		dummyCam = camMngr.dummyCam.transform;
	}

	public void Destroy()
	{
	}

	public void Enter(int iFrom)
	{
		dummyCam.position = mngr.transform.position;
		dummyCam.rotation = mngr.transform.rotation;
	}

	public void Exit(int iTo)
	{
	}

	public void Update()
	{
	}

	public void FixedUpdate()
	{
	}
}
