using UnityEngine;

public class VerletParticle
{
    private GameObject m_goParticle;

    private Vector3 m_vec3OldPos;

    private Vector3 m_vec3Gravity;

    private float m_fTimeStep;

    public void Setup(GameObject goParticle, Vector3 vec3Gravity, float fTimeStep)
    {
        m_goParticle = goParticle;
        m_vec3OldPos = m_goParticle.transform.position;
        m_vec3Gravity = vec3Gravity;
        m_fTimeStep = fTimeStep;
    }

    public void Verlet()
    {
        Vector3 position = m_goParticle.transform.position;
        Vector3 vec3OldPos = new Vector3(position.x, position.y, position.z);
        Vector3 vec3OldPos2 = m_vec3OldPos;
        position += position - vec3OldPos2 + m_vec3Gravity * m_fTimeStep * m_fTimeStep;
        m_goParticle.transform.position = position;
        m_vec3OldPos = vec3OldPos;
    }
}
