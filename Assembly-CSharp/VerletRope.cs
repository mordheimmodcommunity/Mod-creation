using System.Collections.Generic;
using UnityEngine;

public class VerletRope : MonoBehaviour
{
    public Transform HookedPosition;

    private uint m_iNumParticles = 60u;

    private uint m_iNumIterations = 7u;

    private float m_fParticleRadius = 0.08f;

    private float m_fRelaxationPercentage = 0.85f;

    private float m_fFixedTimeStepSpeed = 0.02f;

    private float m_fGravityAmount = 7f;

    private uint m_iNumSticks;

    private List<GameObject> m_listRopeParticles;

    private List<VerletStick> m_listSticks;

    private void Start()
    {
        m_listRopeParticles = new List<GameObject>();
        m_listSticks = new List<VerletStick>();
        Vector3 position = HookedPosition.position;
        m_iNumSticks = m_iNumParticles - 1;
        GameObject original = GameObject.Find("RopeParticle");
        for (int i = 0; i < m_iNumParticles; i++)
        {
            Object @object = Object.Instantiate(original, position, HookedPosition.rotation);
            position.y -= m_fParticleRadius;
            m_listRopeParticles.Add((GameObject)@object);
        }
        for (int j = 1; j < m_iNumParticles; j++)
        {
            VerletStick verletStick = new VerletStick();
            GameObject go = m_listRopeParticles[j - 1];
            GameObject go2 = m_listRopeParticles[j];
            verletStick.Setup(go, go2, m_fParticleRadius, m_fRelaxationPercentage, m_fFixedTimeStepSpeed, new Vector3(0f, 0f - m_fGravityAmount, 0f));
            m_listSticks.Add(verletStick);
        }
    }

    private void FixedUpdate()
    {
        m_listRopeParticles[0].transform.position = HookedPosition.position;
        for (int i = 0; i < m_iNumSticks; i += 2)
        {
            m_listSticks[i].Simulate();
        }
        for (int j = 0; j < m_iNumIterations; j++)
        {
            m_listRopeParticles[0].transform.position = HookedPosition.position;
            for (int k = 0; k < m_iNumSticks; k++)
            {
                m_listSticks[k].SatisfyConstraint();
            }
            m_listRopeParticles[0].transform.position = HookedPosition.position;
            for (int num = (int)(m_iNumSticks - 1); num >= 0; num--)
            {
                m_listSticks[num].SatisfyConstraint();
            }
        }
        for (int l = 0; l < m_iNumSticks; l++)
        {
            m_listSticks[l].Reorient();
        }
        m_listRopeParticles[0].transform.position = HookedPosition.position;
        for (int m = 0; m < m_iNumParticles; m++)
        {
            m_listRopeParticles[m].GetComponent<Rigidbody>().velocity = new Vector3(0f, 0f, 0f);
        }
    }
}
