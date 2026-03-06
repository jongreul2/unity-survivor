using UnityEngine;

[System.Serializable]
public class ProjectileInstance
{
    public Transform transform;
    public Vector3 direction;
    public float speed;
    public float damage;
    public bool isActive;

    public void MoveForward(float deltaTime)
    {
        if (transform == null) return;
        transform.position += direction * (speed * deltaTime);
    }
}
