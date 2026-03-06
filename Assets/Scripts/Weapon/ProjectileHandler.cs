using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ProjectileHandler : MonoBehaviour
{
    [HideInInspector] public ProjectileInstance projectileInstance;
    [HideInInspector] public WeaponManager weaponManager;
}
