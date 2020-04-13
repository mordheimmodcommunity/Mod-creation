using UnityEngine;

public class ConstrainedCamera : ICheapState
{
    private const float MAX_ROT = 20f;

    private CameraManager mngr;

    private Transform dummyCam;

    private float rotX;

    private float rotY;

    private float oRotX;

    private float oRotY;

    public ConstrainedCamera(CameraManager camMngr)
    {
        mngr = camMngr;
        dummyCam = camMngr.dummyCam.transform;
        rotX = 0f;
        rotY = 0f;
        oRotX = 0f;
        oRotY = 0f;
    }

    public void Destroy()
    {
    }

    public void Enter(int iFrom)
    {
        SetCamBehindUnit();
        rotX = 0f;
        rotY = 0f;
    }

    public void Exit(int iTo)
    {
    }

    public void SetOrigins(Transform trans)
    {
        dummyCam.position = trans.position;
        dummyCam.rotation = trans.rotation;
        Vector3 eulerAngles = dummyCam.rotation.eulerAngles;
        oRotX = eulerAngles.x;
        Vector3 eulerAngles2 = dummyCam.rotation.eulerAngles;
        oRotY = eulerAngles2.y;
    }

    private void SetCamBehindUnit()
    {
        mngr.SetSideCam(isLarge: false);
        Vector3 eulerAngles = dummyCam.rotation.eulerAngles;
        oRotX = eulerAngles.x;
        Vector3 eulerAngles2 = dummyCam.rotation.eulerAngles;
        oRotY = eulerAngles2.y;
    }

    public void Update()
    {
        float num = 0f;
        float num2 = 0f;
        num = PandoraSingleton<PandoraInput>.Instance.GetAxis("mouse_x") / 2f;
        num2 = (0f - PandoraSingleton<PandoraInput>.Instance.GetAxis("mouse_y")) / 2f;
        num += PandoraSingleton<PandoraInput>.Instance.GetAxis("cam_x") * 4f;
        num2 += (0f - PandoraSingleton<PandoraInput>.Instance.GetAxis("cam_y")) * 4f;
        rotX += num2;
        rotY += num;
        rotX = Mathf.Clamp(rotX, -20f, 20f);
        rotY = Mathf.Clamp(rotY, -20f, 20f);
        Quaternion lhs = Quaternion.AngleAxis(oRotY + rotY, Vector3.up);
        Quaternion rhs = Quaternion.AngleAxis(oRotX + rotX, Vector3.right);
        dummyCam.rotation = lhs * rhs;
    }

    public void FixedUpdate()
    {
    }
}
