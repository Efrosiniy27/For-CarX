using UnityEngine;

namespace Magic
{
    public class MagicProjectile : Projectile
    {
        [SerializeField] private GameObject m_target;

        protected override void OnUpdate() {
            if (m_target == null)
            {
                Destroy(gameObject);
                return;
            }

            var translation = m_target.transform.position - m_transform.position;
            translation = translation.normalized * m_speed * Time.deltaTime;
            m_transform.Translate(translation);
        }

        public void SetTarget(GameObject target) {
            m_target = target;
        }
    }
}