using TspAcoSolver;

namespace Tests;

[TestClass]
public sealed class RandomFuncsTests
{
    [TestMethod]
    public void TestChooseWeightBiased_RandomLowerBound()
    {
        RandomFuncs rndFuncs = new(new MockRandom([0.0]));
        Assert.AreEqual(0, rndFuncs.ChooseWeightBiased([5.4, 6.7, 9.0]));
    }

    [TestMethod]
    public void TestChooseWeightBiased_RandomUpperBound()
    {
        RandomFuncs rndFuncs = new(new MockRandom([Double.BitDecrement(1d)]));
        Assert.AreEqual(2, rndFuncs.ChooseWeightBiased([5.4, 6.7, 9.0]));
    }
    [TestMethod]
    public void TestChooseWeightBiased_RandomBorder()
    {
        RandomFuncs rndFuncs = new(new MockRandom([0.5]));
        Assert.AreEqual(1, rndFuncs.ChooseWeightBiased([1d, 1d]));
    }
    [TestMethod]
    public void TestChooseWeightBiased_Weighted1()
    {
        RandomFuncs rndFuncs = new(new MockRandom([0.5]));
        Assert.AreEqual(1, rndFuncs.ChooseWeightBiased([1d, 2d]));
    }
    [TestMethod]
    public void TestChooseWeightBiased_Weighted2()
    {
        RandomFuncs rndFuncs = new(new MockRandom([0.5]));
        Assert.AreEqual(0, rndFuncs.ChooseWeightBiased([2d, 1d]));
    }
}
