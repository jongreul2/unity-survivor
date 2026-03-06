using NUnit.Framework;
using UnityEngine;

[TestFixture]
public class ObjectPoolTest
{
    private GameObject _prefab;
    private ObjectPool _pool;
    private Transform _parent;

    [SetUp]
    public void SetUp()
    {
        _prefab = new GameObject("TestPrefab");
        _parent = new GameObject("PoolParent").transform;
        _pool = new ObjectPool(_prefab, 5, _parent);
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(_parent.gameObject);
        Object.DestroyImmediate(_prefab);
    }

    [Test]
    public void Pool_InitializesWithCorrectCount()
    {
        Assert.AreEqual(5, _parent.childCount);
    }

    [Test]
    public void Get_ReturnsActiveObject()
    {
        GameObject obj = _pool.Get();
        Assert.IsNotNull(obj);
        Assert.IsTrue(obj.activeSelf);
    }

    [Test]
    public void Return_DeactivatesObject()
    {
        GameObject obj = _pool.Get();
        _pool.Return(obj);
        Assert.IsFalse(obj.activeSelf);
    }

    [Test]
    public void Get_WhenPoolExhausted_CreatesNewObject()
    {
        for (int i = 0; i < 6; i++)
            _pool.Get();
        Assert.AreEqual(6, _parent.childCount);
    }
}
