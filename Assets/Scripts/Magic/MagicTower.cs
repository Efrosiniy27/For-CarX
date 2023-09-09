using UnityEngine;

namespace Magic
{
    public class MagicTower : Tower
    {
        private float m_lastShotTime = -0.5f;

        private void Update() {
            if (m_lastShotTime + m_shootInterval > Time.time || !IsHasTargets) return;
            Shot();
            m_lastShotTime = Time.time;
        }

        private void Shot() {
            var monster = GetClosestTarget();
            var projectile = RunProjectile(transform.position + Vector3.up * 1.5f, Quaternion.identity);
            var magicProjectile = projectile.GetComponent<MagicProjectile>();
            magicProjectile.SetTarget(monster.gameObject);
        }
    }
}