using UnityEngine;

public class VerletStick
{
    private GameObject m_goParticle1;

    private GameObject m_goParticle2;

    private float m_fStickLength;

    private float m_fRelaxPercent;

    private VerletParticle m_verletParticle1;

    private VerletParticle m_verletParticle2;

    public void Setup(GameObject go1, GameObject go2, float fStickLength, float fRelaxPercent, float fTimeStep, Vector3 vec3Gravity)
    {
        m_goParticle1 = go1;
        m_goParticle2 = go2;
        m_fStickLength = fStickLength;
        m_fRelaxPercent = fRelaxPercent;
        m_verletParticle1 = new VerletParticle();
        m_verletParticle2 = new VerletParticle();
        m_verletParticle1.Setup(m_goParticle1, vec3Gravity, fTimeStep);
        m_verletParticle2.Setup(m_goParticle2, vec3Gravity, fTimeStep);
    }

    public void SatisfyConstraint()
    {
        Vector3 vec = m_goParticle1.transform.position;
        Vector3 vec2 = m_goParticle2.transform.position;
        ClampVector3(ref vec, -50f, 50f);
        ClampVector3(ref vec2, -50f, 50f);
        m_goParticle1.transform.position = vec;
        m_goParticle2.transform.position = vec2;
        Vector3 a = vec - vec2;
        float magnitude = a.magnitude;
        if (!(magnitude <= 0f))
        {
            float d = (magnitude - m_fStickLength) / magnitude;
            vec -= a * m_fRelaxPercent * d;
            vec2 += a * m_fRelaxPercent * d;
            m_goParticle1.transform.position = vec;
            m_goParticle2.transform.position = vec2;
        }
    }

    public void Simulate()
    {
        m_verletParticle1.Verlet();
        m_verletParticle2.Verlet();
    }

    public void Reorient()
    {
        Vector3 lookRotation = m_goParticle2.transform.position - m_goParticle1.transform.position;
        Quaternion rotation = m_goParticle2.transform.rotation;
        rotation.SetLookRotation(lookRotation);
        m_goParticle2.transform.rotation = rotation;
    }

    private void ClampVector3(ref Vector3 vec, float min, float max)
    {
        if (vec.y < min)
        {
            vec.y = min;
        }
        else if (vec.y > max)
        {
            vec.y = max;
        }
        vec.z = 0f;
    }
}
