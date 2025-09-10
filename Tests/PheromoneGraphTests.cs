using TspAcoSolver;

namespace Tests;

[TestClass]
public sealed class PheromoneGraphTests
{
    PheromoneParams _pParams;
    WeightedGraph _wGraph;
    PheromoneGraph _pGraph;

    [TestInitialize]
    public void TestInitialize()
    {
        _pParams = new();
        _pParams.EvaporationCoef = 0.5;
        _pParams.DecayCoef = 0.1;
        _pParams.PheromoneAmount = 1;
        _pParams.InitialPheromoneAmount = 10;

        _wGraph = new(new double[,]
        {
            {1d, 1d},
            {1d, 1d},
        });

        _pGraph = new(_wGraph, _pParams, new NullPheromoneVisualiser());
    }
    [TestMethod]
    public void TestUpdateGloballyPheromones_NoZeroPheromoneLevels()
    {
        Tour tour = new(_wGraph);
        tour.Add(0);
        tour.Add(1);

        for (int __ = 0; __ < 50_000; __++)
        {
            _pGraph.UpdateGloballyPheromones(new List<ITour> { tour });
        }
        Assert.AreNotEqual(0, _pGraph.Pheromones[0, 0]);
    }
}
