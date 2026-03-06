using NUnit.Framework;
using UnityEngine;

[TestFixture]
public class PlayerControllerTest
{
    private GameObject _playerObj;
    private PlayerController _player;

    [SetUp]
    public void SetUp()
    {
        _playerObj = new GameObject("Player");
        _player = _playerObj.AddComponent<PlayerController>();
        _player.maxHP = 100f;
        _player.moveSpeed = 5f;
        _player.mapBoundary = 25f;
        _player.InitializeForTest();
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(_playerObj);
    }

    [Test]
    public void TakeDamage_ReducesHP()
    {
        _player.TakeDamage(30f);
        Assert.AreEqual(70f, _player.CurrentHP);
    }

    [Test]
    public void TakeDamage_BelowZero_ClampedToZero()
    {
        _player.TakeDamage(150f);
        Assert.AreEqual(0f, _player.CurrentHP);
    }

    [Test]
    public void IsDead_WhenHPZero_ReturnsTrue()
    {
        _player.TakeDamage(100f);
        Assert.IsTrue(_player.IsDead);
    }

    [Test]
    public void ClampPosition_RestrictsToMapBoundary()
    {
        _playerObj.transform.position = new Vector3(30f, 0f, 30f);
        _player.ClampPosition();
        Vector3 pos = _playerObj.transform.position;
        Assert.LessOrEqual(Mathf.Abs(pos.x), 25f);
        Assert.LessOrEqual(Mathf.Abs(pos.z), 25f);
    }
}
