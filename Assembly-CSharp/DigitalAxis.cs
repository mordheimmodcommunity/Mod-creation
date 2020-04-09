using System;
using UnityEngine;

[Serializable]
public class DigitalAxis
{
	public KeyCode posKey;

	public KeyCode posAltKey;

	public KeyCode posAltKey2;

	public KeyCode negKey;

	public KeyCode negAltKey;

	public KeyCode negAltKey2;

	public float gravity;

	public float sensitivity;

	public bool snap;

	public bool invert;

	private float axis;

	private float rawAxis;

	private float oldRawAxis;

	private int inverse;

	private bool isDown;

	private bool isNegDown;

	private float isDownDelay;

	public void Init()
	{
		axis = 0f;
		rawAxis = 0f;
		oldRawAxis = 0f;
		inverse = ((!invert) ? 1 : (-1));
		isDown = false;
		isNegDown = false;
		isDownDelay = 0f;
	}

	public float GetAxis()
	{
		return axis;
	}

	public float GetAxisRaw()
	{
		return rawAxis;
	}

	public bool GetKeyDown()
	{
		return Input.GetKeyDown(posKey) || Input.GetKeyDown(posAltKey) || Input.GetKeyDown(posAltKey2) || (isDown && isDownDelay <= 0f);
	}

	public bool GetNegKeyDown()
	{
		return Input.GetKeyDown(negKey) || Input.GetKeyDown(negAltKey) || Input.GetKeyDown(negAltKey2) || (isNegDown && isDownDelay <= 0f);
	}

	public bool GetKeyUp()
	{
		return Input.GetKeyUp(posKey) || Input.GetKeyUp(posAltKey) || Input.GetKeyUp(posAltKey2);
	}

	public bool GetNegKeyUp()
	{
		return Input.GetKeyUp(negKey) || Input.GetKeyUp(negAltKey) || Input.GetKeyUp(negAltKey2);
	}

	public bool GetKey()
	{
		return Input.GetKey(posKey) || Input.GetKey(posAltKey) || Input.GetKey(posAltKey2);
	}

	public bool GetNegKey()
	{
		return Input.GetKey(negKey) || Input.GetKey(negAltKey) || Input.GetKey(negAltKey2);
	}

	public void Update()
	{
		if (isDown || isNegDown)
		{
			if (isDownDelay > 0f)
			{
				isDownDelay -= Time.deltaTime;
			}
			else
			{
				isDownDelay = PandoraSingleton<PandoraInput>.Instance.subsequentRepeatRate;
			}
		}
		if (Input.GetKeyDown(posKey) || Input.GetKeyDown(posAltKey) || Input.GetKeyDown(posAltKey2))
		{
			if (snap && oldRawAxis != 1f)
			{
				axis = 0f;
			}
			rawAxis = 1f;
			isDown = true;
			isDownDelay = PandoraSingleton<PandoraInput>.Instance.firstRepeatDelay;
		}
		else if (Input.GetKeyDown(negKey) || Input.GetKeyDown(negAltKey) || Input.GetKeyDown(negAltKey2))
		{
			if (snap && (double)oldRawAxis != -1.0)
			{
				axis = 0f;
			}
			rawAxis = -1f;
			isNegDown = true;
			isDownDelay = PandoraSingleton<PandoraInput>.Instance.firstRepeatDelay;
		}
		if (Input.GetKeyUp(posKey) || Input.GetKeyUp(posAltKey) || Input.GetKeyUp(posAltKey2) || Input.GetKeyUp(negKey) || Input.GetKeyUp(negAltKey) || Input.GetKeyUp(negAltKey2))
		{
			oldRawAxis = rawAxis;
			rawAxis = 0f;
			if (Input.GetKeyUp(posKey) || Input.GetKeyUp(posAltKey) || Input.GetKeyUp(posAltKey2))
			{
				isDown = false;
			}
			else if (Input.GetKeyUp(negKey) || Input.GetKeyUp(negAltKey) || Input.GetKeyUp(negAltKey2))
			{
				isNegDown = false;
			}
			CheckKeysDown();
		}
		if (Input.GetKey(posKey) || Input.GetKey(posAltKey) || Input.GetKey(posAltKey2) || Input.GetKey(negKey) || Input.GetKey(negAltKey) || Input.GetKey(negAltKey2))
		{
			axis += rawAxis * (float)inverse * sensitivity * Time.deltaTime;
			axis = Mathf.Clamp(axis, -1f, 1f);
		}
		else if (oldRawAxis != 0f)
		{
			float num = axis;
			axis -= oldRawAxis * (float)inverse * gravity * Time.deltaTime;
			if ((num > 0f && axis <= 0f) || (num < 0f && axis >= 0f))
			{
				axis = 0f;
				oldRawAxis = 0f;
			}
		}
	}

	private void CheckKeysDown()
	{
		if (Input.GetKey(posKey) || Input.GetKey(posAltKey) || Input.GetKey(posAltKey2))
		{
			if (snap)
			{
				axis = 0f;
			}
			rawAxis = 1f;
		}
		else if (Input.GetKey(negKey) || Input.GetKey(negAltKey) || Input.GetKey(negAltKey2))
		{
			if (snap)
			{
				axis = 0f;
			}
			rawAxis = -1f;
		}
	}
}
