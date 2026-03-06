using NUnit.Framework;

[TestFixture]
public class XPManagerTest
{
    [Test]
    public void AddXP_IncreasesCurrentXP()
    {
        var xpSystem = new XPSystem(new int[] { 10, 25, 50 });
        xpSystem.AddXP(5);
        Assert.AreEqual(5, xpSystem.CurrentXP);
    }

    [Test]
    public void AddXP_TriggersLevelUp_WhenThresholdReached()
    {
        var xpSystem = new XPSystem(new int[] { 10, 25, 50 });
        bool leveledUp = false;
        xpSystem.OnLevelUp += (level) => leveledUp = true;

        xpSystem.AddXP(10);
        Assert.IsTrue(leveledUp);
        Assert.AreEqual(1, xpSystem.CurrentLevel);
    }

    [Test]
    public void AddXP_MultipleLevelUps()
    {
        var xpSystem = new XPSystem(new int[] { 10, 25, 50 });
        xpSystem.AddXP(10); // Lv 1
        xpSystem.AddXP(15); // Lv 2
        Assert.AreEqual(2, xpSystem.CurrentLevel);
    }

    [Test]
    public void AddXP_MaxLevel_DoesNotExceed()
    {
        var xpSystem = new XPSystem(new int[] { 10 });
        xpSystem.AddXP(10); // Lv 1 (max)
        xpSystem.AddXP(100);
        Assert.AreEqual(1, xpSystem.CurrentLevel);
    }

    [Test]
    public void XPForNextLevel_ReturnsCorrectThreshold()
    {
        var xpSystem = new XPSystem(new int[] { 10, 25, 50 });
        Assert.AreEqual(10, xpSystem.XPForNextLevel);
        xpSystem.AddXP(10);
        Assert.AreEqual(25, xpSystem.XPForNextLevel);
    }
}
