using UnityEngine;

namespace Cannon
{
    public class CannonProjectileLinear : Projectile
    {
        protected override void OnUpdate() {
            var translate = m_transform.forward * (m_speed * Time.deltaTime);
            m_transform.position += translate;
        }
    }
}