using UnityEngine;

public class test_spawner : MonoBehaviour
{
    public GameObject[] Models;

    private bool CHbool;

    private bool Optsbool;

    private bool Animsbool;

    private int anims;

    private GameObject SpawnedModel;

    public GameObject[] Props;

    public GUIStyle btnStyle;

    public GUIStyle btnStyle2;

    public RuntimeAnimatorController ViewerAnimator;

    public Camera[] Cams;

    private bool camerasbool;

    public GameObject[] Fxs;

    private bool fxsbool;

    private GameObject Spawnedfx;

    private Animator animator;

    private void Start()
    {
    }

    private void Update()
    {
        if (GameObject.Find("Origin").transform.childCount > 0)
        {
            SpawnedModel.transform.position = GameObject.Find("Origin").transform.position;
        }
        if (Cams != null)
        {
            for (int i = 0; i < Cams.Length; i++)
            {
                Cams[i].transform.LookAt(GameObject.Find("Origin").transform);
            }
        }
    }

    private void OnGUI()
    {
        GUILayout.BeginArea(new Rect(0f, 0f, Screen.width / 5, Screen.height));
        if (!Optsbool)
        {
            if (GUILayout.Button("+ Options", btnStyle))
            {
                Optsbool = true;
            }
        }
        else
        {
            if (GUILayout.Button("- Options", btnStyle))
            {
                Optsbool = false;
            }
            GameObject[] props = Props;
            foreach (GameObject gameObject in props)
            {
                if (GameObject.Find(gameObject.name).GetComponent<Renderer>().enabled && GUILayout.Button("Hide:  " + gameObject.name, btnStyle2))
                {
                    GameObject.Find(gameObject.name).GetComponent<Renderer>().enabled = false;
                }
                if (!GameObject.Find(gameObject.name).GetComponent<Renderer>().enabled && GUILayout.Button("Unhide:  " + gameObject.name, btnStyle2))
                {
                    GameObject.Find(gameObject.name).GetComponent<Renderer>().enabled = true;
                }
            }
            if (GUILayout.Button("Reset", btnStyle2))
            {
                Application.LoadLevel(Application.loadedLevelName);
            }
        }
        if (!CHbool)
        {
            if (GUILayout.Button("+ Characters", btnStyle))
            {
                CHbool = true;
            }
        }
        else
        {
            if (GUILayout.Button("- Characters", btnStyle))
            {
                CHbool = false;
            }
            GameObject[] models = Models;
            foreach (GameObject gameObject2 in models)
            {
                if (GUILayout.Button(gameObject2.name, btnStyle))
                {
                    if (GameObject.Find("Origin").transform.childCount > 0)
                    {
                        Object.Destroy(GameObject.Find("Origin").transform.GetChild(0).gameObject);
                    }
                    SpawnedModel = (Object.Instantiate(gameObject2, base.transform.position, Quaternion.identity) as GameObject);
                    SpawnedModel.transform.position = GameObject.Find("Origin").transform.position;
                    SpawnedModel.transform.rotation = GameObject.Find("Origin").transform.rotation;
                    SpawnedModel.transform.parent = GameObject.Find("Origin").transform;
                    SpawnedModel.GetComponent<Rigidbody>().useGravity = false;
                    Animsbool = false;
                }
            }
        }
        if (!Animsbool && GUILayout.Button("+ Animations", btnStyle))
        {
            Animsbool = true;
            animator = SpawnedModel.GetComponent<Animator>();
            animator.runtimeAnimatorController = ViewerAnimator;
        }
        if (Animsbool)
        {
            if (GUILayout.Button("- Animations", btnStyle))
            {
                Animsbool = false;
            }
            if (GUILayout.Button("Idle", btnStyle2))
            {
                animator.SetBool("idle", value: true);
                animator.SetInteger("animID", 0);
            }
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("<", btnStyle2))
            {
                animator.SetBool("idle", value: true);
                animator.SetInteger("animID", --anims);
                animator.SetBool("idle", value: false);
                MonoBehaviour.print(anims);
            }
            if (GUILayout.Button(">", btnStyle2))
            {
                animator.SetBool("idle", value: true);
                animator.SetInteger("animID", ++anims);
                animator.SetBool("idle", value: false);
                MonoBehaviour.print(anims);
            }
            GUILayout.EndHorizontal();
        }
        if (!camerasbool && GUILayout.Button("+  Cameras", btnStyle))
        {
            camerasbool = true;
        }
        if (camerasbool)
        {
            if (GUILayout.Button("-  Cameras", btnStyle))
            {
                camerasbool = false;
            }
            Camera[] cams = Cams;
            foreach (Camera camera in cams)
            {
                if (GUILayout.Button(camera.name, btnStyle))
                {
                    for (int l = 0; l < Cams.Length; l++)
                    {
                        Cams[l].enabled = false;
                    }
                    camera.enabled = true;
                }
            }
        }
        if (!fxsbool && GUILayout.Button("+  Fx's", btnStyle))
        {
            fxsbool = true;
        }
        if (fxsbool)
        {
            if (GUILayout.Button("-  Fx's", btnStyle))
            {
                fxsbool = false;
            }
            GameObject[] fxs = Fxs;
            foreach (GameObject gameObject3 in fxs)
            {
                if (GUILayout.Button(gameObject3.name, btnStyle))
                {
                    if (GameObject.Find("FXS").transform.childCount > 0)
                    {
                        Object.Destroy(GameObject.Find("FXS").transform.GetChild(0).gameObject);
                    }
                    Spawnedfx = (Object.Instantiate(gameObject3, base.transform.position, Quaternion.identity) as GameObject);
                    Spawnedfx.transform.position = GameObject.Find("FXS").transform.position;
                    Spawnedfx.transform.rotation = GameObject.Find("FXS").transform.rotation;
                    Spawnedfx.transform.parent = GameObject.Find("FXS").transform;
                }
            }
        }
        GUILayout.EndArea();
    }
}
