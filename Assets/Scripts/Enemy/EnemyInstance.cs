using UnityEngine;

[System.Serializable]
public class EnemyInstance
{
    public float currentHP;
    public float moveSpeed;
    public Transform transform;
    public EnemyData data;
    public bool isActive;

    public bool IsDead => currentHP <= 0f;

    public void TakeDamage(float amount)
    {
        currentHP -= amount;
    }

    public void MoveToward(Vector3 target, float deltaTime)
    {
        if (transform == null) return;
        Vector3 dir = (target - transform.position).normalized;
        transform.position += dir * (moveSpeed * deltaTime);
    }
}
