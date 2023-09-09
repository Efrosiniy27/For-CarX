using System;
using System.Collections;
using Effects;
using UnityEngine;
using UnityEngine.Serialization;

namespace Cannon
{
    public class CannonTower : Tower
    {
        [SerializeField] private Transform m_shootPoint;
        [SerializeField] private Transform m_cannonHub;
        [SerializeField] private Transform m_cannonCannon;
        [SerializeField] private Transform m_tower;
        [SerializeField] private WaterWaveController m_waterWaveController;
        [Space]
        [SerializeField] private CannonTowerType m_CannonTowerType;
        [SerializeField] private float m_speedRotation = 10f;

        private Vector3 m_originalPositionTower;
        private Quaternion m_originalRotationTower;
        private CameraShake m_cameraShake;

        private float m_lastShotTime = -0.5f;
        private float m_ProjectilePhysicsMaxPosY = 6.0f;

        protected override void Start() {
            base.Start();
            m_originalPositionTower = m_tower.localPosition;
            m_originalRotationTower = m_tower.localRotation;
            m_cameraShake = Camera.main.GetComponent<CameraShake>();
        }

        private void Update() {
            if (m_lastShotTime + m_shootInterval > Time.time || !IsHasTargets) return;
            var isPhysics = m_CannonTowerType == CannonTowerType.Physics;
            var direction = isPhysics
                ? GetFireVelocityPhysics()
                : GetFireDirectionLinear();
            if (IsNeedRotateCannon(direction))
            {
                RotateCannon(direction);
            }
            else
            {
                StartCoroutine(Recoil(direction));
                m_waterWaveController?.CreateWave(transform.position);
                CameraShake();
                if (isPhysics)
                {
                    ShotPhysics(direction);
                }
                else
                {
                    ShotLinear(direction);
                }
                m_lastShotTime = Time.time;
            }
        }


        private void CameraShake() {
            m_cameraShake.Shake();
        }

        private void RotateCannon(Vector3 direction) {
            var targetRotation = Quaternion.LookRotation(direction);
            m_cannonCannon.rotation =
                Quaternion.Slerp(m_cannonCannon.rotation, targetRotation, Time.deltaTime * m_speedRotation);
            Vector3 horizontalDirection = new Vector3(direction.x, 0, direction.z);
            var targetHorizontalDirection = Quaternion.LookRotation(horizontalDirection);
            m_cannonHub.rotation = Quaternion.Slerp(m_cannonHub.rotation, targetHorizontalDirection,
                Time.deltaTime * m_speedRotation);
        }

        private void ShotPhysics(Vector3 velocity) {
            Quaternion fireRotation = Quaternion.LookRotation(velocity);
            RunProjectilePhysics(velocity, fireRotation, m_shootPoint.position);
        }

        private void ShotLinear(Vector3 direction) {
            Quaternion fireRotation = Quaternion.LookRotation(direction);
            RunProjectile(m_shootPoint.position, fireRotation);
        }

        private bool IsNeedRotateCannon(Vector3 direction) {
            var dotProduct = Vector3.Dot(m_cannonCannon.forward.normalized, direction.normalized);
            var isNeedRotateCannon = Math.Abs(dotProduct - 1.0f) > 0.01f;
            return isNeedRotateCannon;
        }


        private Vector3 GetFireVelocityPhysics() {
            var closestMonster = GetClosestTarget();
            var g = Physics.gravity.y;
            var positionPositionShot = m_shootPoint.position;
            var startVerticalSpeed = Mathf.Sqrt(-2 * g * (m_ProjectilePhysicsMaxPosY - positionPositionShot.y));
            var positionMonster = closestMonster.transform.position;
            var t1 = -startVerticalSpeed / g;
            var t2 = Mathf.Sqrt(-2 * (m_ProjectilePhysicsMaxPosY - positionMonster.y) / g);
            var allT = t1 + t2;
            var positionShotHorizontal = positionPositionShot;
            positionShotHorizontal.y = 0;
            var monsterPosHorizontal = positionMonster;
            monsterPosHorizontal.y = 0;
            var startApproximateHorizontalSpeed = Vector3.Distance(positionShotHorizontal, monsterPosHorizontal) / allT;
            var interceptPositionMonster = CalculateInterceptPoint(closestMonster, startApproximateHorizontalSpeed);
            var deltaHorizontal = interceptPositionMonster - positionPositionShot;
            deltaHorizontal.y = 0;
            var distanceHorizontal = deltaHorizontal.magnitude;
            var startHorizontalSpeed = distanceHorizontal / allT;
            var direction = deltaHorizontal.normalized * startHorizontalSpeed + new Vector3(0, startVerticalSpeed, 0);
            return direction;
        }

        private Vector3 GetFireDirectionLinear() {
            var closestMonster = GetClosestTarget();
            Vector3 interceptPoint = CalculateInterceptPoint(closestMonster, m_projectilePrefab.GetSpeed());
            var direction = interceptPoint - m_shootPoint.position;
            return direction;
        }


        private float m_recoilAmount = 0.02f;
        private float m_recoilRotation = 6.0f;
        private float m_recoilTime = 0.25f;

        IEnumerator Recoil(Vector3 direction) {
            float startTime = Time.time;
            Vector3 targetPosition = m_originalPositionTower +
                                     m_tower.InverseTransformDirection(-direction.normalized) * m_recoilAmount;
            Quaternion targetRotation = m_originalRotationTower * Quaternion.Euler(-m_recoilRotation, 0, 0);

            while (Time.time < startTime + m_recoilTime)
            {
                float t = (Time.time - startTime) / m_recoilTime;
                m_tower.localPosition = Vector3.Lerp(m_originalPositionTower, targetPosition, t);
                m_tower.localRotation = Quaternion.Lerp(m_originalRotationTower, targetRotation, t);
                yield return null;
            }

            m_tower.localPosition = m_originalPositionTower;
            m_tower.localRotation = m_originalRotationTower;
        }

        private Vector3 CalculateInterceptPoint(Monster target, float projectileSpeed) {
            Vector3 targetPosition = target.transform.position;
            Vector3 targetVelocity = target.GetVelocity();
            Vector3 deltaPosition = targetPosition - m_shootPoint.position;

            float a = Vector3.Dot(targetVelocity, targetVelocity) - projectileSpeed * projectileSpeed;
            float b = 2f * Vector3.Dot(deltaPosition, targetVelocity);
            float c = Vector3.Dot(deltaPosition, deltaPosition);

            float discriminant = b * b - 4f * a * c;
            if (discriminant < 0f)
            {
                return targetPosition;
            }

            float t1 = (-b + Mathf.Sqrt(discriminant)) / (2f * a);
            float t2 = (-b - Mathf.Sqrt(discriminant)) / (2f * a);

            float interceptTime = Mathf.Max(t1, t2);
            Vector3 interceptPoint = targetPosition + targetVelocity * interceptTime;
            return interceptPoint;
        }
    }

    public enum CannonTowerType
    {
        Linear,
        Physics
    }
}