using UnityEngine;

namespace Effects
{
    public class CameraShake : MonoBehaviour
    {
        private float m_shakeDuration = 0.2f;
        private float m_shakeMagnitude = 0.1f;
        private float m_dampingSpeed = 1.0f;

        private Vector3 m_initialPosition;
        private float m_currentShakeDuration;

        private Transform m_transform;

        void Awake() {
            m_transform = GetComponent<Transform>();
            m_initialPosition = m_transform.localPosition;
        }

        void Update() {
            if (m_currentShakeDuration > 0)
            {
                m_transform.localPosition = m_initialPosition + Random.insideUnitSphere * m_shakeMagnitude;
                m_currentShakeDuration -= Time.deltaTime * m_dampingSpeed;
            }
            else
            {
                m_currentShakeDuration = 0f;
                m_transform.localPosition = m_initialPosition;
            }
        }

        public void Shake() {
            m_currentShakeDuration = m_shakeDuration;
        }
    }
}