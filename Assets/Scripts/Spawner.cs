using UnityEngine;
using System.Collections;

public class Spawner : MonoBehaviour
{
    [SerializeField] private float m_interval = 3;
    [SerializeField] private GameObject m_moveTarget;

    private float m_lastSpawn = -1;

    void Update() {
        if (m_lastSpawn + m_interval > Time.time) return;
        var capsulePrimitive = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        var rigidbody = capsulePrimitive.AddComponent<Rigidbody>();
        var monster = capsulePrimitive.AddComponent<Monster>();
        capsulePrimitive.transform.position = transform.position;
        rigidbody.useGravity = false;
        monster.SetTarget(m_moveTarget);
        m_lastSpawn = Time.time;
    }
}