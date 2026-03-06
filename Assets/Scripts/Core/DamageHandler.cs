using UnityEngine;

[RequireComponent(typeof(Collider))]
public class DamageHandler : MonoBehaviour
{
    [HideInInspector] public EnemyInstance enemyInstance;
    [HideInInspector] public EnemyManager enemyManager;
    [HideInInspector] public ParticleManager particleManager;
    [HideInInspector] public XPManager xpManager;
    [HideInInspector] public WorldSpaceBar healthBar;

    private void OnTriggerEnter(Collider other)
    {
        if (enemyInstance == null || !enemyInstance.isActive) return;

        var projHandler = other.GetComponent<ProjectileHandler>();
        if (projHandler != null && projHandler.projectileInstance != null && projHandler.projectileInstance.isActive)
        {
            enemyInstance.TakeDamage(projHandler.projectileInstance.damage);

            if (healthBar != null)
                healthBar.SetValue(enemyInstance.HPRatio);

            particleManager?.Play(ParticleType.HitSpark, transform.position, enemyInstance.data.color);

            projHandler.weaponManager.ReturnProjectile(projHandler.projectileInstance);

            if (enemyInstance.IsDead)
            {
                particleManager?.Play(ParticleType.EnemyDeath, transform.position, enemyInstance.data.color, 1.5f);

                if (xpManager != null)
                {
                    xpManager.DropGem(transform.position, enemyInstance.data.xpValue);
                }

                enemyManager.RemoveEnemy(enemyInstance);
            }
        }
    }
}
