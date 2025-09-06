using Microsoft.Extensions.Options;
using TspAcoSolver;

namespace Tests;

[TestClass]
public sealed class AsAntTests
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
    public void TestFindTour()
    {

        AsAnt ant = new(Options.Create(_cParams), new MockRandom([1]));

        WeightedGraph wGraph = new(new double[,]
        {
        { 1d, 2d},
        { 3d, 2d}
        });

        PheromoneGraph graph = new(wGraph, _pParams, new NullPheromoneVisualiser());

        ant.FindTour(graph);

        Tour tour = new(wGraph);
        Assert.AreEqual(tour, ant.LastTour);
    }
}
