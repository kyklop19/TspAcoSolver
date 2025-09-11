using ScottPlot.Colormaps;
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
        _pParams.CalculateInitialPheromoneAmount = false;

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
    [TestMethod]
    public void TestUpdateGloballyPheromones()
    {
        Tour tour = new(_wGraph);
        tour.Add(0);
        tour.Add(1);

        _pGraph.UpdateGloballyPheromones(new List<ITour>() { tour });

        CollectionAssert.AreEqual(new double[,]{
            { 5 , 5.25},
            { 5.25, 5 }
        },_pGraph.Pheromones);

    }

    [TestMethod]
    public void TestUpdateLocallyPheromones_Disturbed()
    {
        Tour tour = new(_wGraph);
        tour.Add(0);
        tour.Add(1);

        _pGraph.UpdateGloballyPheromones(new List<ITour>() { tour });

        _pGraph.UpdateLocallyPheromones(tour);

        double[,] expected = new double[,]{
            { 5 , 5.725},
            { 5.725, 5 }
        };

        for (int i = 0; i < 2; i++)
        {
            for (int j = 0; j < 2; j++)
            {
                Assert.AreEqual(expected[i,j], _pGraph.Pheromones[i, j], 0.000000000000001);
            }
        }

    }
    [TestMethod]
    public void TestUpdateLocallyPheromones_Undisturbed()
    {
        Tour tour = new(_wGraph);
        tour.Add(0);
        tour.Add(1);

        _pGraph.UpdateLocallyPheromones(tour);

        double[,] expected = new double[,]{
            { 10 , 10 },
            { 10 , 10 }
        };

        for (int i = 0; i < 2; i++)
        {
            for (int j = 0; j < 2; j++)
            {
                Assert.AreEqual(expected[i,j], _pGraph.Pheromones[i, j], 0.000000000000001);
            }
        }

    }
}
