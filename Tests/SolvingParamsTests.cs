using TspAcoSolver;

namespace Tests;

[TestClass]
public sealed class SolvingParamsTests
{
    [TestMethod]
    public void TestOverwrite_NonNullWithNull()
    {
        SolvingParams original = new()
        {
            Algorithm = "AS"
        };
        SolvingParams replacement = new()
        {
            Algorithm = null
        };

        original.Overwrite(replacement);

        Assert.AreEqual("AS", original.Algorithm);
    }

    [TestMethod]
    public void TestOverwrite_NullWithNull()
    {
        SolvingParams original = new()
        {
            Algorithm = null
        };
        SolvingParams replacement = new()
        {
            Algorithm = null
        };

        original.Overwrite(replacement);

        Assert.AreEqual(null, original.Algorithm);
    }

    [TestMethod]
    public void TestOverwrite_NonNullWithNonNull()
    {
        SolvingParams original = new()
        {
            Algorithm = "AS"
        };
        SolvingParams replacement = new()
        {
            Algorithm = "ACS"
        };

        original.Overwrite(replacement);

        Assert.AreEqual("ACS", original.Algorithm);
    }

    [TestMethod]
    public void TestOverwrite_NullWithNonNull()
    {
        SolvingParams original = new()
        {
            Algorithm = null
        };
        SolvingParams replacement = new()
        {
            Algorithm = "ACS"
        };

        original.Overwrite(replacement);

        Assert.AreEqual("ACS", original.Algorithm);
    }

    [TestMethod]
    public void TestOverwrite_RecursiveNonNullWithNonNull()
    {
        SolvingParams original = new()
        {
            TerminationParams = new()
            {
                TerminationRule = TerminationRule.Fixed,
            }
        };
        SolvingParams replacement = new()
        {
            TerminationParams = new()
            {
                TerminationRule = TerminationRule.WithinPercentage,
            }
        };

        original.Overwrite(replacement);

        Assert.AreEqual(TerminationRule.WithinPercentage, original.TerminationParams.TerminationRule);
    }

    [TestMethod]
    public void TestOverwrite_NonNullWithNothing()
    {
        SolvingParams original = new()
        {
            Algorithm = "AS"
        };
        SolvingParams replacement = new()
        {
        };

        original.Overwrite(replacement);

        Assert.AreEqual("AS", original.Algorithm);
        Assert.AreEqual(null, replacement.Algorithm);
    }
    [TestMethod]
    [ExpectedException(typeof(Exception))]
    public void TestOverwrite_NotSameParams()
    {
        SolvingParams original = new()
        {
        };
        TerminationParams replacement = new()
        {
        };

        original.Overwrite(replacement);
    }
}
