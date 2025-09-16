using TspAcoSolver;

namespace Tests;

[TestClass]
public sealed class AsColonyTests
{
    [TestMethod]
    public void TestGenerateSolutions()
    {
        AsColony colony = new(new MockAntFactory<MockAnt>(new MockAnt[]{
            new MockAnt(new ITour[]{
                new MockTour(178)
            }),
            new MockAnt(new ITour[]{
                new MockTour(96)
            }),
        }), new ColonyParams()
        {
            ThreadCount = 1,
            AntCount = 2,
        });

        List<ITour> sols = colony.GenerateSolutions(new PheromoneGraph(new double[,] { { } }, new PheromoneParams()
        {
            CalculateInitialPheromoneAmount = false,
            InitialPheromoneAmount = 1,
            DecayCoef = 0.1,
            EvaporationCoef = 0.1,
            PheromoneAmount = 1,
        }, new NullPheromoneGraphVisualiser()));

        Assert.AreEqual(178, sols[0].Length);
        Assert.AreEqual(96, sols[1].Length);
    }
}
