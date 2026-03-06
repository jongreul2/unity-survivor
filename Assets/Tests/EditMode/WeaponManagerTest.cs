using NUnit.Framework;
using UnityEngine;

[TestFixture]
public class WeaponManagerTest
{
    [Test]
    public void ProjectileInstance_MoveForward_UpdatesPosition()
    {
        var obj = new GameObject("Projectile");
        obj.transform.position = Vector3.zero;
        var instance = new ProjectileInstance
        {
            transform = obj.transform,
            direction = Vector3.forward,
            speed = 10f,
            damage = 5f,
            isActive = true
        };

        instance.MoveForward(1f);

        Assert.AreEqual(10f, obj.transform.position.z, 0.01f);
        Object.DestroyImmediate(obj);
    }

    [Test]
    public void GetDamageAtLevel_AppliesLevelBonus()
    {
        var data = ScriptableObject.CreateInstance<WeaponData>();
        data.baseDamage = 10f;
        data.levelBonuses = new LevelBonus[]
        {
            new() { damageMultiplier = 1.0f, fireRateMultiplier = 1.0f, projectileCount = 1, particleScale = 1f },
            new() { damageMultiplier = 2.0f, fireRateMultiplier = 1.5f, projectileCount = 3, particleScale = 2f },
        };

        float lv0 = WeaponManager.GetDamageAtLevel(data, 0);
        float lv1 = WeaponManager.GetDamageAtLevel(data, 1);

        Assert.AreEqual(10f, lv0);
        Assert.AreEqual(20f, lv1);

        Object.DestroyImmediate(data);
    }

    [Test]
    public void GetProjectileCountAtLevel_ReturnsCorrectCount()
    {
        var data = ScriptableObject.CreateInstance<WeaponData>();
        data.levelBonuses = new LevelBonus[]
        {
            new() { damageMultiplier = 1f, fireRateMultiplier = 1f, projectileCount = 1, particleScale = 1f },
            new() { damageMultiplier = 1f, fireRateMultiplier = 1f, projectileCount = 3, particleScale = 1f },
            new() { damageMultiplier = 1f, fireRateMultiplier = 1f, projectileCount = 5, particleScale = 1f },
        };

        Assert.AreEqual(1, WeaponManager.GetProjectileCountAtLevel(data, 0));
        Assert.AreEqual(5, WeaponManager.GetProjectileCountAtLevel(data, 2));

        Object.DestroyImmediate(data);
    }
}
