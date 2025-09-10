using TspAcoSolver;

namespace Tests;

[TestClass]
public sealed class TerminationCheckerTests
{
    [TestMethod]
    public void TestFixed_Lower()
    {
        TerminationChecker termChecker = new(new TerminationParams()
        {
            TerminationRule = TerminationRule.Fixed,
            IterationCount = 1000
        });

        Counter counter = new();

        termChecker.CurrIterationCounter = counter;

        for (int i = 0; i < 500; i++)
        {
            counter.Inc();
        }

        Assert.IsFalse(termChecker.Terminated());
    }
    [TestMethod]
    public void TestFixed_Equal()
    {
        TerminationChecker termChecker = new(new TerminationParams()
        {
            TerminationRule = TerminationRule.Fixed,
            IterationCount = 1000
        });

        Counter counter = new();

        termChecker.CurrIterationCounter = counter;

        for (int i = 0; i < 1000; i++)
        {
            counter.Inc();
        }

        Assert.IsTrue(termChecker.Terminated());
    }
    [TestMethod]
    public void TestFixed_Higher()
    {
        TerminationChecker termChecker = new(new TerminationParams()
        {
            TerminationRule = TerminationRule.Fixed,
            IterationCount = 1000
        });

        Counter counter = new();

        termChecker.CurrIterationCounter = counter;

        for (int i = 0; i < 1500; i++)
        {
            counter.Inc();
        }

        Assert.IsTrue(termChecker.Terminated());
    }
    [TestMethod]
    public void TestWithinPercentage_BelowPercentage()
    {
        TerminationChecker termChecker = new(new TerminationParams()
        {
            TerminationRule = TerminationRule.WithinPercentage,
            CeilingPercentage = 50,
            InRowTerminationCount = 1
        });

        termChecker.CurrBestTour = new MockTour(100);
        termChecker.MinimumLengthSolInIter = new MockTour(125);

        Assert.IsTrue(termChecker.Terminated());
    }
    [TestMethod]
    public void TestWithinPercentage_EqualPercentage()
    {
        TerminationChecker termChecker = new(new TerminationParams()
        {
            TerminationRule = TerminationRule.WithinPercentage,
            CeilingPercentage = 50,
            InRowTerminationCount = 1
        });

        termChecker.CurrBestTour = new MockTour(100);
        termChecker.MinimumLengthSolInIter = new MockTour(150);

        Assert.IsTrue(termChecker.Terminated());
    }
    [TestMethod]
    public void TestWithinPercentage_AbovePercentage()
    {
        TerminationChecker termChecker = new(new TerminationParams()
        {
            TerminationRule = TerminationRule.WithinPercentage,
            CeilingPercentage = 50,
            InRowTerminationCount = 1
        });

        termChecker.CurrBestTour = new MockTour(100);
        termChecker.MinimumLengthSolInIter = new MockTour(175);

        Assert.IsFalse(termChecker.Terminated());
    }
}
