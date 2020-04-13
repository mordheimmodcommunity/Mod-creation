using System;
using UnityEngine;

[Serializable]
public class AnalogAxis
{
    public string inputName;

    public string altInputName;

    public float sensitivity = 1f;

    public bool invert;

    public bool repeatDown;

    private int inverse;

    private float prevAxis;

    private bool isPosDown;

    private bool isPosUp;

    private bool wasPosUp;

    private bool wasPosDown;

    private bool isNegDown;

    private bool isNegUp;

    private bool wasNegUp;

    private bool wasNegDown;

    private float isDownDelay;

    public void Init()
    {
        inverse = ((!invert) ? 1 : (-1));
    }

    public void Update()
    {
        if (string.IsNullOrEmpty(inputName))
        {
            return;
        }
        if (wasPosDown || wasNegDown)
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
        isPosDown = false;
        isPosUp = false;
        isNegDown = false;
        isNegUp = false;
        float num = Input.GetAxisRaw(inputName);
        if (!string.IsNullOrEmpty(altInputName))
        {
            num += Input.GetAxisRaw(altInputName);
        }
        if (num > 0.95f && prevAxis < 0.95f && !wasPosDown)
        {
            isPosDown = true;
            wasPosDown = true;
            isDownDelay = PandoraSingleton<PandoraInput>.Instance.firstRepeatDelay;
            wasPosUp = false;
        }
        else if (num < 0.5f && prevAxis > 0.5f && !wasPosUp)
        {
            isPosUp = true;
            wasPosUp = true;
            wasPosDown = false;
        }
        if (num < -0.95f && prevAxis > -0.95f && !wasNegDown)
        {
            isNegDown = true;
            wasNegDown = true;
            isDownDelay = PandoraSingleton<PandoraInput>.Instance.firstRepeatDelay;
            wasNegUp = false;
        }
        else if (num > -0.5f && prevAxis < -0.5f && !wasNegUp)
        {
            isNegUp = true;
            wasNegUp = true;
            wasNegDown = false;
        }
        prevAxis = num;
    }

    public bool GetKeyDown()
    {
        return ((!invert) ? isPosDown : isNegDown) || (((!invert) ? wasPosDown : wasNegDown) && isDownDelay <= 0f);
    }

    public bool GetNegKeyDown()
    {
        return invert ? isPosDown : (isNegDown || (((!invert) ? wasNegDown : wasPosDown) && isDownDelay <= 0f));
    }

    public bool GetKeyUp()
    {
        return (!invert) ? isPosUp : isNegUp;
    }

    public bool GetNegKeyUp()
    {
        return (!invert) ? isNegUp : isPosUp;
    }

    public bool GetKey()
    {
        if (string.IsNullOrEmpty(inputName))
        {
            return false;
        }
        bool flag = false;
        if (invert)
        {
            flag = (Input.GetAxis(inputName) < 0f);
            if (!string.IsNullOrEmpty(altInputName))
            {
                flag = (flag || Input.GetAxis(altInputName) < 0f);
            }
        }
        else
        {
            flag = (Input.GetAxis(inputName) > 0f);
            if (!string.IsNullOrEmpty(altInputName))
            {
                flag = (flag || Input.GetAxis(altInputName) > 0f);
            }
        }
        return flag;
    }

    public bool GetNegKey()
    {
        if (string.IsNullOrEmpty(inputName))
        {
            return false;
        }
        bool flag = false;
        if (invert)
        {
            flag = (Input.GetAxis(inputName) > 0f);
            if (!string.IsNullOrEmpty(altInputName))
            {
                flag = (flag || Input.GetAxis(altInputName) > 0f);
            }
        }
        else
        {
            flag = (Input.GetAxis(inputName) < 0f);
            if (!string.IsNullOrEmpty(altInputName))
            {
                flag = (flag || Input.GetAxis(altInputName) < 0f);
            }
        }
        return flag;
    }

    public float GetAxis()
    {
        if (string.IsNullOrEmpty(inputName))
        {
            return 0f;
        }
        float num = Input.GetAxis(inputName) * (float)inverse * sensitivity;
        if (!string.IsNullOrEmpty(altInputName))
        {
            num += Input.GetAxis(altInputName) * (float)inverse * sensitivity;
        }
        return num;
    }

    public float GetAxisRaw()
    {
        if (string.IsNullOrEmpty(inputName))
        {
            return 0f;
        }
        float num = Input.GetAxisRaw(inputName) * (float)inverse * sensitivity;
        if (!string.IsNullOrEmpty(altInputName))
        {
            num += Input.GetAxisRaw(altInputName) * (float)inverse * sensitivity;
        }
        return num;
    }
}
