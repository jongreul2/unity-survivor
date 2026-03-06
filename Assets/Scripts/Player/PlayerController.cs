using UnityEngine;
using UnityEngine.InputSystem;

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
    private Vector2 _moveInput;

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
        ReadInput();
        HandleMovement();
        ClampPosition();
    }

    private void ReadInput()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        _moveInput = Vector2.zero;
        if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) _moveInput.y += 1f;
        if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) _moveInput.y -= 1f;
        if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) _moveInput.x += 1f;
        if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) _moveInput.x -= 1f;
    }

    private void HandleMovement()
    {
        Vector3 dir = new Vector3(_moveInput.x, 0f, _moveInput.y).normalized;
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
