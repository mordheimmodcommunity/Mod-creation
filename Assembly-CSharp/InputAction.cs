using UnityEngine;

public class InputAction : MonoBehaviour
{
    public string actionName;

    [SerializeField]
    public DigitalAxis dAxis;

    [SerializeField]
    public AnalogAxis aAxis;

    public void Init()
    {
        dAxis.Init();
        aAxis.Init();
    }

    public float GetAxis()
    {
        float axis = dAxis.GetAxis();
        if (axis != 0f)
        {
            return axis;
        }
        axis = aAxis.GetAxis();
        if (axis != 0f)
        {
            return axis;
        }
        return 0f;
    }

    public float GetAxisRaw()
    {
        float axisRaw = dAxis.GetAxisRaw();
        if (axisRaw != 0f)
        {
            return axisRaw;
        }
        axisRaw = aAxis.GetAxisRaw();
        if (axisRaw != 0f)
        {
            return axisRaw;
        }
        return 0f;
    }

    public bool GetKeyDown()
    {
        bool keyDown = dAxis.GetKeyDown();
        if (keyDown)
        {
            return keyDown;
        }
        keyDown = aAxis.GetKeyDown();
        if (keyDown)
        {
            return keyDown;
        }
        return false;
    }

    public bool GetNegKeyDown()
    {
        bool negKeyDown = dAxis.GetNegKeyDown();
        if (negKeyDown)
        {
            return negKeyDown;
        }
        negKeyDown = aAxis.GetNegKeyDown();
        if (negKeyDown)
        {
            return negKeyDown;
        }
        return false;
    }

    public bool GetKeyUp()
    {
        bool keyUp = dAxis.GetKeyUp();
        if (keyUp)
        {
            return keyUp;
        }
        keyUp = aAxis.GetKeyUp();
        if (keyUp)
        {
            return keyUp;
        }
        return false;
    }

    public bool GetNegKeyUp()
    {
        bool negKeyUp = dAxis.GetNegKeyUp();
        if (negKeyUp)
        {
            return negKeyUp;
        }
        negKeyUp = aAxis.GetNegKeyUp();
        if (negKeyUp)
        {
            return negKeyUp;
        }
        return false;
    }

    public bool GetKey()
    {
        bool key = dAxis.GetKey();
        if (key)
        {
            return key;
        }
        key = aAxis.GetKey();
        if (key)
        {
            return key;
        }
        return false;
    }

    public bool GetNegKey()
    {
        bool negKey = dAxis.GetNegKey();
        if (negKey)
        {
            return negKey;
        }
        negKey = aAxis.GetNegKey();
        if (negKey)
        {
            return negKey;
        }
        return false;
    }

    private void Update()
    {
    }
}
