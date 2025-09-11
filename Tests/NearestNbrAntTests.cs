using TspAcoSolver;
using Microsoft.Extensions.Options;

namespace Tests;

[TestClass]
public sealed class NearestNbrAntTests
{
    PheromoneParams _pParams;

    [TestInitialize]
    public void TestInitialize()
    {
        _pParams = new();
        _pParams.EvaporationCoef = 0.1;
        _pParams.DecayCoef = 0.1;
        _pParams.PheromoneAmount = 1d;
        _pParams.CalculateInitialPheromoneAmount = true;
    }
    [TestMethod]
    public void TestFindTour()
    {

        NearestNbrAnt ant = new(new MockRandom([1]));

        WeightedGraph wGraph = new(new double[,]
        {
        { 1d, 2d, 5d},
        { 6d, 2d, 3d},
        { 2d, 7d, 1d}
        });

        PheromoneGraph graph = new(wGraph, _pParams, new NullPheromoneVisualiser());

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
