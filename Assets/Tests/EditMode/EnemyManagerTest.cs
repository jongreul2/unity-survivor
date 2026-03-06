using NUnit.Framework;
using UnityEngine;

[TestFixture]
public class EnemyManagerTest
{
    [Test]
    public void EnemyInstance_TakeDamage_ReducesHP()
    {
        var instance = new EnemyInstance { currentHP = 10f, isActive = true };
        instance.TakeDamage(3f);
        Assert.AreEqual(7f, instance.currentHP);
    }

    [Test]
    public void EnemyInstance_TakeDamage_Dies_WhenHPZero()
    {
        var instance = new EnemyInstance { currentHP = 5f, isActive = true };
        instance.TakeDamage(5f);
        Assert.IsTrue(instance.IsDead);
    }

    [Test]
    public void EnemyInstance_MoveToward_MovesInCorrectDirection()
    {
        var obj = new GameObject("Enemy");
        obj.transform.position = new Vector3(10f, 0f, 0f);
        var instance = new EnemyInstance
        {
            currentHP = 10f,
            isActive = true,
            transform = obj.transform,
            moveSpeed = 5f
        };

        Vector3 target = Vector3.zero;
        instance.MoveToward(target, 1f);

        Assert.Less(obj.transform.position.x, 10f);

        Object.DestroyImmediate(obj);
    }
}
