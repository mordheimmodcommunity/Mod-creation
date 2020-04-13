using UnityEngine;

public class FreeFormCam : ICheapState
{
    public float moveSpeed = 10f;

    public float rotationSpeed = 10f;

    private CameraManager mngr;

    private Transform dummyCam;

    public FreeFormCam(CameraManager camMngr)
    {
        mngr = camMngr;
        dummyCam = mngr.dummyCam.transform;
    }

    public void Destroy()
    {
    }

    public void Enter(int from)
    {
    }

    public void Exit(int to)
    {
    }

    public void Update()
    {
        Vector3 a = default(Vector3) + dummyCam.forward * PandoraSingleton<PandoraInput>.Instance.GetAxisRaw("v");
        a += dummyCam.right * PandoraSingleton<PandoraInput>.Instance.GetAxisRaw("h");
        a.Normalize();
        if (Input.GetKey(KeyCode.Mouse1))
        {
            Vector3 eulerAngles = dummyCam.rotation.eulerAngles;
            eulerAngles.y += rotationSpeed * PandoraSingleton<PandoraInput>.Instance.GetAxis("cam_x") * 4f * Time.deltaTime;
            eulerAngles.x -= rotationSpeed * PandoraSingleton<PandoraInput>.Instance.GetAxis("cam_y") * 4f * Time.deltaTime;
            dummyCam.rotation = Quaternion.Euler(eulerAngles);
        }
        dummyCam.position += a * moveSpeed * Time.deltaTime;
    }

    public void FixedUpdate()
    {
    }
}
