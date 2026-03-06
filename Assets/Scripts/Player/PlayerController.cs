using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("스탯")]
    public float maxHP = 100f;
    public float moveSpeed = 5f;
    public float mapBoundary = 25f;

    public float CurrentHP { get; private set; }
    public bool IsDead => CurrentHP <= 0f;

    private float _damageCooldown = 0.5f;
    private float _lastDamageTime;

    private void Start()
    {
        CurrentHP = maxHP;
    }

    public void InitializeForTest()
    {
        CurrentHP = maxHP;
    }

    private void Update()
    {
        HandleMovement();
        ClampPosition();
    }

    private void HandleMovement()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector3 dir = new Vector3(h, 0f, v).normalized;
        transform.position += dir * (moveSpeed * Time.deltaTime);
    }

    public void ClampPosition()
    {
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, -mapBoundary, mapBoundary);
        pos.z = Mathf.Clamp(pos.z, -mapBoundary, mapBoundary);
        transform.position = pos;
    }

    public void TakeDamage(float amount)
    {
        CurrentHP = Mathf.Max(0f, CurrentHP - amount);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (Time.time - _lastDamageTime < _damageCooldown) return;

        var damageHandler = other.GetComponent<DamageHandler>();
        if (damageHandler != null && damageHandler.enemyInstance != null && damageHandler.enemyInstance.isActive)
        {
            TakeDamage(damageHandler.enemyInstance.data.damage);
            _lastDamageTime = Time.time;
        }
    }
}
