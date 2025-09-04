using TspAcoSolver;

namespace Tests;

[TestClass]
public sealed class AcsSolverTests
{

    ColonyParams _cParams;
    PheromoneParams _pParams;

    [TestInitialize]
    public void TestInitialize()
    {
        _cParams = new();
        _cParams.TrailLevelFactor = 2;
        _cParams.AttractivenessFactor = 4;

        _pParams = new();
        _pParams.EvaporationCoef = 0.1;
        _pParams.PheromoneAmount = 1d;
        _pParams.CalculateInitialPheromoneAmount = true;
    }

    [TestMethod]
    public void TestSolve()
    {
    }
}
