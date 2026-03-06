using NUnit.Framework;
using UnityEngine;

[TestFixture]
public class DataModelTest
{
    [Test]
    public void EnemyData_CanBeCreated()
    {
        var data = ScriptableObject.CreateInstance<EnemyData>();
        data.enemyName = "추적자";
        data.maxHP = 10f;
        data.moveSpeed = 5f;
        data.damage = 1f;
        data.xpValue = 5;
        data.color = Color.red;

        Assert.AreEqual("추적자", data.enemyName);
        Assert.AreEqual(10f, data.maxHP);

        Object.DestroyImmediate(data);
    }

    [Test]
    public void WeaponData_LevelBonusApplied()
    {
        var data = ScriptableObject.CreateInstance<WeaponData>();
        data.baseDamage = 10f;
        data.fireRate = 1f;
        data.levelBonuses = new LevelBonus[]
        {
            new() { damageMultiplier = 1.0f, fireRateMultiplier = 1.0f, projectileCount = 1, particleScale = 1.0f },
            new() { damageMultiplier = 1.5f, fireRateMultiplier = 1.2f, projectileCount = 2, particleScale = 1.5f },
        };

        float lv1Damage = data.baseDamage * data.levelBonuses[0].damageMultiplier;
        float lv2Damage = data.baseDamage * data.levelBonuses[1].damageMultiplier;

        Assert.AreEqual(10f, lv1Damage);
        Assert.AreEqual(15f, lv2Damage);

        Object.DestroyImmediate(data);
    }

    [Test]
    public void SpawnWaveData_HasCorrectFields()
    {
        var data = ScriptableObject.CreateInstance<SpawnWaveData>();
        data.startTime = 60f;
        data.spawnInterval = 1.0f;
        data.hpMultiplier = 1.5f;
        data.maxConcurrentEnemies = 30;

        Assert.AreEqual(60f, data.startTime);
        Assert.AreEqual(30, data.maxConcurrentEnemies);

        Object.DestroyImmediate(data);
    }
}
