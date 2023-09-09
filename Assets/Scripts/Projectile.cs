using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] protected float m_speed = 20f;
    [SerializeField] protected int m_damage = 10;

    public bool IsRunned { get; private set; }

    protected Transform m_transform;

    private const float m_lifeTime = 4.0f;
    private float m_startTime;
    private Rigidbody m_rigidBody;

    private void Awake() {
        m_transform = GetComponent<Transform>();
        m_rigidBody = GetComponent<Rigidbody>();
    }

    private void Update() {
        if (m_lifeTime + m_startTime < Time.time)
        {
            Deactivate();
            return;
        }

        OnUpdate();
    }

    protected virtual void OnUpdate() {
    }

    public void Deactivate() {
        IsRunned = false;
        gameObject.SetActive(false);
    }


    public void Run(Vector3 position, Quaternion rotation) {
        m_startTime = Time.time;
        m_transform.position = position;
        m_transform.rotation = rotation;
        IsRunned = true;
        gameObject.SetActive(true);
    }

    public void RunPhysics(Vector3 position,Vector3 startVelocity, Quaternion rotation) {
        m_startTime = Time.time;
        m_rigidBody.velocity = startVelocity;
        m_transform.rotation = rotation;
        m_transform.position = position;
        IsRunned = true;
        gameObject.SetActive(true);
    }

    private void OnTriggerEnter(Collider other) {
        var monster = other.gameObject.GetComponent<Monster>();
        if (monster == null) return;
        monster.TakeDamage(m_damage);
        Deactivate();
    }

    public float GetSpeed() {
        return m_speed;
    }
}