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
        _cParams.ExploProportionConst = 0.9;

        _pParams = new();
        _pParams.EvaporationCoef = 0.1;
        _pParams.DecayCoef = 0.1;
        _pParams.PheromoneAmount = 1d;
        _pParams.CalculateInitialPheromoneAmount = true;
    }

    [TestMethod]
    public void TestFindTour()
    {

        AsAnt ant = new(Options.Create(_cParams), new MockRandom([1], [0.5, 0.1]));

        WeightedGraph wGraph = new(new double[,]
        {
        { 1d, 2d, 5d},
        { 6d, 2d, 3d},
        { 2d, 7d, 1d}
        });

        PheromoneGraph graph = new(wGraph, _pParams, new NullPheromoneGraphVisualiser());

        ant.FindTour(graph);

        Tour tour = new(wGraph);
        tour.Add(1);
        tour.Add(2);
        tour.Add(0);
        for (int i = 0; i < 3; i++)
        {
            Assert.AreEqual(tour.Vertices[i], ant.LastTour.Vertices[i]);
        }
    }
}
