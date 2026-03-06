using NUnit.Framework;
using UnityEngine;

[TestFixture]
public class WorldSpaceBarTest
{
    [Test]
    public void SetValue_ClampsToZeroOne()
    {
        var obj = new GameObject("TestBar");
        var bar = obj.AddComponent<WorldSpaceBar>();
        bar.InitializeForTest();

        bar.SetValue(1.5f);
        Assert.AreEqual(1f, bar.CurrentValue, 0.001f);

        bar.SetValue(-0.5f);
        Assert.AreEqual(0f, bar.CurrentValue, 0.001f);

        Object.DestroyImmediate(obj);
    }

    [Test]
    public void SetValue_UpdatesCurrentValue()
    {
        var obj = new GameObject("TestBar");
        var bar = obj.AddComponent<WorldSpaceBar>();
        bar.InitializeForTest();

        bar.SetValue(0.6f);
        Assert.AreEqual(0.6f, bar.CurrentValue, 0.001f);

        Object.DestroyImmediate(obj);
    }

    [Test]
    public void Initialize_CreatesFillTransform()
    {
        var obj = new GameObject("TestBar");
        var bar = obj.AddComponent<WorldSpaceBar>();
        bar.InitializeForTest();

        Assert.IsTrue(bar.HasFill);

        Object.DestroyImmediate(obj);
    }
}
