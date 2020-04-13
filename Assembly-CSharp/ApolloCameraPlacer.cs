using System.Collections.Generic;
using UnityEngine;

public class ApolloCameraPlacer : MonoBehaviour
{
    public bool next;

    private int idx;

    private List<Transform> cameraPositions;

    private CameraManager cam;

    private void Start()
    {
        idx = -1;
        cameraPositions = new List<Transform>();
        for (int i = 0; i < base.transform.childCount; i++)
        {
            cameraPositions.Add(base.transform.GetChild(i));
        }
        cam = Camera.main.gameObject.GetComponent<CameraManager>();
    }

    private void Update()
    {
        if (next)
        {
            next = false;
            Transform transform = cameraPositions[++idx % cameraPositions.Count];
            cam.dummyCam.transform.position = transform.position;
            cam.dummyCam.transform.rotation = transform.rotation;
            GameObject gameObject = GameObject.Find("movement_lines");
            if ((bool)gameObject)
            {
                gameObject.SetActive(value: false);
            }
        }
    }
}
