using UnityEngine;

[CreateAssetMenu(fileName = "NewWeaponData", menuName = "GeometrySurvivor/WeaponData")]
public class WeaponData : ScriptableObject
{
    public string weaponName;
    public float baseDamage;
    public float fireRate;
    public float projectileSpeed;
    public GameObject projectilePrefab;
    public LevelBonus[] levelBonuses;
}
