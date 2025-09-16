using TspAcoSolver;
using Microsoft.Extensions.Options;

namespace Tests;

[TestClass]
public sealed class AcsAntTests
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
    public void TestFindTour_Exploitation()
    {

        AcsAnt ant = new(Options.Create(_cParams), new MockRandom([2], [0.6, 0.7]));

        WeightedGraph wGraph = new(new double[,]
        {
        { 1d, 2d, 5d},
        { 6d, 2d, 3d},
        { 2d, 7d, 1d}
        });

        PheromoneGraph graph = new(wGraph, _pParams, new NullPheromoneGraphVisualiser());

        ant.FindTour(graph);

        Tour tour = new(wGraph);
        tour.Add(2);
        tour.Add(0);
        tour.Add(1);
        for (int i = 0; i < 3; i++)
        {
            Assert.AreEqual(tour.Vertices[i], ant.LastTour.Vertices[i]);
        }
    }
    [TestMethod]
    public void TestFindTour_Exploration()
    {

        AcsAnt ant = new(Options.Create(_cParams), new MockRandom([0], [
            0.99, 0.98,
            0.97, 0.1
            ]));

        WeightedGraph wGraph = new(new double[,]
        {
        { 1d, 2d, 5d},
        { 6d, 2d, 3d},
        { 2d, 7d, 1d}
        });

        PheromoneGraph graph = new(wGraph, _pParams, new NullPheromoneGraphVisualiser());

        ant.FindTour(graph);

        Tour tour = new(wGraph);
        tour.Add(0);
        tour.Add(2);
        tour.Add(1);
        for (int i = 0; i < 3; i++)
        {
            Assert.AreEqual(tour.Vertices[i], ant.LastTour.Vertices[i]);
        }
    }
}
