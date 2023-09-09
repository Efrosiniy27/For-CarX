using System;
using System.Collections.Generic;
using UnityEngine;

public class Tower : MonoBehaviour
{
    [SerializeField] protected Projectile m_projectilePrefab;
    [Space]
    [SerializeField] protected float m_shootInterval = 0.5f;
    [SerializeField] protected float m_range = 4f;

    private SphereCollider m_sphereCollider;
    private Transform m_transform;

    private static int m_poolSize = 50;

    private List<Projectile> m_objectPool = new List<Projectile>();

    protected virtual void Awake() {
        m_sphereCollider = GetComponent<SphereCollider>();
        m_transform = GetComponent<Transform>();
    }

    protected virtual void Start() {
        if (m_projectilePrefab == null)
            Destroy(gameObject);
        InitPoolProjectiles();
        m_sphereCollider.radius = m_range;
    }

    private void InitPoolProjectiles() {
        for (int i = 0; i < m_poolSize; i++)
        {
            var obj = Instantiate(m_projectilePrefab);
            obj.Deactivate();
            m_objectPool.Add(obj);
        }
    }

    protected Projectile RunProjectile(Vector3 position, Quaternion rotation) {
        for (int i = 0; i < m_objectPool.Count; i++)
        {
            if (!m_objectPool[i].IsRunned)
            {
                m_objectPool[i].Run(position, rotation);
                return m_objectPool[i];
            }
        }
        return null;
    }
    
    protected Projectile RunProjectilePhysics(Vector3 velocity, Quaternion rotation, Vector3 position) {
        for (int i = 0; i < m_objectPool.Count; i++)
        {
            if (!m_objectPool[i].IsRunned)
            {
                m_objectPool[i].RunPhysics(position,velocity, rotation);
                return m_objectPool[i];
            }
        }
        return null;
    }

    private List<Monster> m_triggeredMonsters = new List<Monster>();

    private void OnTriggerEnter(Collider other) {
        var monster = other.GetComponent<Monster>();
        if (monster != null)
        {
            m_triggeredMonsters.Add(monster);
            monster.Killed += OnKilledTriggeredMonster;
        }
    }

    private void OnKilledTriggeredMonster(Monster monster) {
        m_triggeredMonsters.Remove(monster);
    }

    private void OnTriggerExit(Collider other) {
        var monster = other.GetComponent<Monster>();
        if (monster != null)
        {
            m_triggeredMonsters.Remove(monster);
            monster.Killed -= OnKilledTriggeredMonster;
        }
    }

    protected bool IsHasTargets => m_triggeredMonsters.Count > 0;

    protected Monster GetClosestTarget() {
        Monster closestMonster = null;
        float closestDistance = Mathf.Infinity;

        foreach (Monster monster in m_triggeredMonsters)
        {
            float distance = Vector3.Distance(m_transform.position, monster.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestMonster = monster;
            }
        }

        return closestMonster;
    }
}