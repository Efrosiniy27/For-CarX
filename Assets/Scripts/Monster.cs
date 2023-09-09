using System;
using UnityEngine;
using System.Collections;

public class Monster : MonoBehaviour
{
    private GameObject m_moveTarget;
    private float m_speed = 5f;
    private int m_maxHP = 50;

    public event Action<Monster> Killed;

    const float m_reachDistance = 0.3f;

    private Transform m_transform;
    private Vector3 m_velocity;
    private int m_hp;

    private void Awake() {
        m_transform = GetComponent<Transform>();
    }

    void Start() {
        m_hp = m_maxHP;
    }

    void Update() {
        if (m_moveTarget == null) return;
        if (IsReached())
        {
            Killed?.Invoke(this);
            Destroy(gameObject);
            return;
        }

        var translate = m_moveTarget.transform.position - m_transform.position;
        m_velocity = translate.normalized * m_speed;
        translate = m_velocity * Time.deltaTime;
        m_transform.Translate(translate);
    }

    private bool IsReached() {
        return Vector3.SqrMagnitude(m_transform.position - m_moveTarget.transform.position) <= m_reachDistance;
    }

    public void TakeDamage(int damage) {
        m_hp -= damage;
        if (m_hp <= 0)
        {
            Killed?.Invoke(this);
            Destroy(gameObject);
        }
    }

    public void SetTarget(GameObject moveTarget) {
        m_moveTarget = moveTarget;
    }

    public Vector3 GetVelocity() {
        return m_velocity;
    }
}