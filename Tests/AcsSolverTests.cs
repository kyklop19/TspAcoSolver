using TspAcoSolver;
using Microsoft.Extensions.DependencyInjection;

namespace Tests;

[TestClass]
public sealed class AcsSolverTests
{

    // ColonyParams _cParams;
    // PheromoneParams _pParams;

    // [TestInitialize]
    // public void TestInitialize()
    // {
    //     _cParams = new();
    //     _cParams.TrailLevelFactor = 2;
    //     _cParams.AttractivenessFactor = 4;

    //     _pParams = new();
    //     _pParams.EvaporationCoef = 0.1;
    //     _pParams.PheromoneAmount = 1d;
    //     _pParams.CalculateInitialPheromoneAmount = true;
    // }

    [TestMethod]
    public void TestSolve()
    {
        ServiceCollection serviceCollection = new();
        serviceCollection.Configure<PheromoneParams>(pParams =>
        {
            pParams.CalculateInitialPheromoneAmount = false;
            pParams.InitialPheromoneAmount = 1;
            pParams.DecayCoef = 0.1;
            pParams.EvaporationCoef = 0.1;
            pParams.PheromoneAmount = 1;
        });
        serviceCollection.AddTransient<IPheromoneGraphVisualiser, NullPheromoneVisualiser>();
        AcsSolver solver = new(
            serviceCollection.BuildServiceProvider(),
            new MockColony(new List<ITour>
            {
                new MockTour(5.5),
                new MockTour(19),
                new MockTour(0),
                new MockTour(-1),
            }),
            new TerminationChecker(new TerminationParams()
            {
                TerminationRule = TerminationRule.Fixed,
                IterationCount = 1
            }),
            new Reinitializer(new ReinitializationParams()
            {
                ReinitializationRule = ReinitializationRule.None
            })
        );
        Assert.AreEqual(-1, solver.Solve(new MockProblem()).Length);
    }

    [TestMethod]
    public void TestSolve_NoToursFound()
    {
        ServiceCollection serviceCollection = new();
        serviceCollection.Configure<PheromoneParams>(pParams =>
        {
            pParams.CalculateInitialPheromoneAmount = false;
            pParams.InitialPheromoneAmount = 1;
            pParams.DecayCoef = 0.1;
            pParams.EvaporationCoef = 0.1;
            pParams.PheromoneAmount = 1;
        });
        serviceCollection.AddTransient<IPheromoneGraphVisualiser, NullPheromoneVisualiser>();
        AcsSolver solver = new(
            serviceCollection.BuildServiceProvider(),
            new MockColony(new List<ITour>
            {
            }),
            new TerminationChecker(new TerminationParams()
            {
                TerminationRule = TerminationRule.Fixed,
                IterationCount = 1
            }),
            new Reinitializer(new ReinitializationParams()
            {
                ReinitializationRule = ReinitializationRule.None
            })
        );
        Assert.IsInstanceOfType<InfiniteTour>(solver.Solve(new MockProblem()));
    }
}
