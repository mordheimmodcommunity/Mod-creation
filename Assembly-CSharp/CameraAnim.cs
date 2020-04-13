using UnityEngine;
using UnityEngine.SceneManagement;

public class CameraAnim : ICheapState
{
    private GameObject camAnimRef;

    private GameObject anchor;

    private bool relative;

    private CamDelegate onDone;

    private CameraManager mngr;

    private Transform dummyCam;

    public CameraAnim(CameraManager camMngr)
    {
        mngr = camMngr;
        dummyCam = camMngr.dummyCam.transform;
        camAnimRef = new GameObject("camera_animator");
        camAnimRef.AddComponent<Animation>();
        SceneManager.MoveGameObjectToScene(camAnimRef.gameObject, camMngr.gameObject.scene);
        anchor = new GameObject("camera_anchor");
        SceneManager.MoveGameObjectToScene(anchor.gameObject, camMngr.gameObject.scene);
        camAnimRef.GetComponent<Animation>().Stop();
    }

    public void Destroy()
    {
    }

    public void Enter(int from)
    {
    }

    public void Exit(int to)
    {
        AnimationDone();
    }

    public void Update()
    {
        if (!camAnimRef.GetComponent<Animation>().isPlaying && (relative || onDone != null))
        {
            AnimationDone();
        }
    }

    public void FixedUpdate()
    {
    }

    public void AnimationDone()
    {
        if ((bool)camAnimRef)
        {
            camAnimRef.GetComponent<Animation>().Stop();
            if (relative)
            {
                relative = false;
                dummyCam.SetParent(null);
            }
            if (onDone != null)
            {
                CamDelegate camDelegate = onDone;
                onDone = null;
                camDelegate();
            }
        }
    }

    private void Play(string clipName)
    {
        camAnimRef.GetComponent<Animation>().Stop();
        if (camAnimRef.GetComponent<Animation>()[clipName] == null)
        {
            AnimationClip clip = Resources.Load<AnimationClip>("camera/clips/" + clipName);
            camAnimRef.GetComponent<Animation>().AddClip(clip, clipName);
        }
        camAnimRef.GetComponent<Animation>().Play(clipName);
    }

    public void PlayRelative(string clip, Transform anchorRef, CamDelegate done = null)
    {
        PlayRelative(clip, anchorRef.position, anchorRef.rotation, done);
    }

    public void PlayRelative(string clip, Vector3 position, Quaternion rotation, CamDelegate done = null)
    {
        relative = true;
        onDone = done;
        anchor.transform.position = position;
        anchor.transform.rotation = rotation;
        camAnimRef.transform.SetParent(anchor.transform);
        dummyCam.SetParent(camAnimRef.transform);
        dummyCam.localPosition = Vector3.zero;
        dummyCam.localRotation = Quaternion.identity;
        Play(clip);
    }

    public void PlayAttached(string clip, Transform anchorRef, CamDelegate done = null)
    {
        relative = true;
        onDone = done;
        camAnimRef.transform.SetParent(anchorRef.transform);
        dummyCam.SetParent(camAnimRef.transform);
        dummyCam.localPosition = Vector3.zero;
        dummyCam.localRotation = Quaternion.identity;
        Play(clip);
    }

    public void Stop()
    {
        camAnimRef.GetComponent<Animation>().Stop();
    }
}
