using System.Collections;
using UnityEngine;

public class cad_movement : MonoBehaviour
{
    public GameObject LastBone;

    public int multiplier = 1;

    public int RandomWait;

    private bool FinishWait;

    private void Start()
    {
        FinishWait = true;
    }

    private IEnumerator RandomMov()
    {
        RandomWait = Random.Range(1, 10);
        int x = Random.Range(10, 50);
        int y = Random.Range(10, 50);
        Vector3 force = new Vector3(z: Random.Range(10, 50) * multiplier, x: x * multiplier, y: y * multiplier);
        yield return new WaitForSeconds(RandomWait);
        if (LastBone.transform.GetComponent<Rigidbody>() != null)
        {
            LastBone.transform.GetComponent<Rigidbody>().AddForce(force);
        }
        FinishWait = true;
    }

    private void Update()
    {
        if (FinishWait)
        {
            StartCoroutine(RandomMov());
            FinishWait = false;
        }
    }
}
