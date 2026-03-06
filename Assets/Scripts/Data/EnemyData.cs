using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemyData", menuName = "GeometrySurvivor/EnemyData")]
public class EnemyData : ScriptableObject
{
    public string enemyName;
    public GameObject prefab;
    public float maxHP;
    public float moveSpeed;
    public float damage;
    public int xpValue;
    public Color color;
}
