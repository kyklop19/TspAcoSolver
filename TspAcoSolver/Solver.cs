using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace TspAcoSolver
{
    /// <summary>
    /// Interface for classes that solve implementation of <c>IProblem</c>
    /// </summary>
    public interface ISolver
    {
        /// <summary>
        /// Count of the iterations made during solution generation
        /// </summary>
        public int CurrIterationCount { get; }

        /// <summary>
        /// Try to find solution <c>ITour</c> whose length is as close as possible to the length of the optimal solution
        /// </summary>
        /// <param name="problem">Problem for which to find solution</param>
        /// <returns>Solution <c>ITour</c> whose length is as close as possible to the length of the optimal solution</returns>
        public ITour Solve(IProblem problem);
    }

    /// <summary>
    /// Abstract class that implements common solving approach of Ant colony optimization algorithms
    /// </summary>
    /// <param name="_serviceProvider">Provider that provides services for dependency injection</param>
    public abstract class SolverBase(IServiceProvider _serviceProvider) : ISolver
    {
        IProblem _problem;

        protected PheromoneGraph Graph;
        protected IColony AntColony { get; init; }

        ITour _currBestTour = new InfiniteTour();

        Counter _currIterationCounter = new();
        Counter _stagnationCounter = new();

        /// <summary>
        /// Count of the iterations made during solution generation
        /// </summary>
        public int CurrIterationCount { get => _currIterationCounter.Value; }

        protected ITour _minimumLengthSolInIter = new InfiniteTour();

        ITerminationChecker _terminationChecker;
        IReinitializer _reinitializer;

        /// <summary>
        /// Construct solver with colony, termination checker and reinitializer
        /// </summary>
        /// <param name="serviceProvider">Provider that provides services for dependency injection</param>
        /// <param name="colony">Colony that generates solutions</param>
        /// <param name="terminationChecker">Checker that checks if solution generation should terminate</param>
        /// <param name="reinitializer">Reinitializer that tries to reinitilize pheromone values if conditions are met</param>
        public SolverBase(IServiceProvider serviceProvider, IColony colony, ITerminationChecker terminationChecker, IReinitializer reinitializer) : this(serviceProvider)
        {
            AntColony = colony;

            _terminationChecker = terminationChecker;
            _terminationChecker.CurrBestTour = _currBestTour;
            _terminationChecker.MinimumLengthSolInIter = _minimumLengthSolInIter;
            _terminationChecker.CurrIterationCounter = _currIterationCounter;

            _reinitializer = reinitializer;
            _reinitializer.StagnationCounter = _stagnationCounter;
            _reinitializer.CurrIterationCounter = _currIterationCounter;
        }

        /// <summary>
        /// Filter solutions such that only for returned solutions is the
        /// <c>PheromoneGraph</c> updated.
        /// </summary>
        /// <param name="solutions">All found solutions</param>
        /// <returns>
        /// Solutions for which the <c>PheromoneGraph</c> should be updated
        /// </returns>
        protected abstract List<ITour> FilterSolutions(List<ITour> solutions);

        /// <summary>
        /// Find minimal solution in last iteration, update best-so-far solution and filter solutions
        /// </summary>
        /// <param name="solutions">Solutions to post process</param>
        /// <returns>Filtered solutions to be updated in graph</returns>
        List<ITour> PostprocessSolutions(List<ITour> solutions)
        {
            _minimumLengthSolInIter = new InfiniteTour();
            foreach (ITour sol in solutions)
            {
                // Console.WriteLine(sol);

                Console.WriteLine($" Lenght: {sol.Length}");

                if (sol.Length < _minimumLengthSolInIter.Length)
                {
                    _minimumLengthSolInIter = sol;
                    _terminationChecker.MinimumLengthSolInIter = _minimumLengthSolInIter;
                }
            }
            if (_minimumLengthSolInIter.Length < _currBestTour.Length)
            {
                Console.WriteLine($"Found better tour");

                _currBestTour = _minimumLengthSolInIter;
                _terminationChecker.CurrBestTour = _currBestTour;
                _terminationChecker.ResetInRowWithinPercentageCount();
                Console.WriteLine($"Best: {_currBestTour.Length}");

                _stagnationCounter.Reset();
            }
            else
            {
                _stagnationCounter.Inc();
            }

            return FilterSolutions(solutions);
        }

        /// <summary>
        /// Call update method of <c>PheromoneGraph</c> to update it with
        /// filtered solutions.
        /// </summary>
        /// <param name="solutions">
        /// Filtered solutions for which the <c>PheromoneGraph</c> should be
        /// updated
        /// </param>
        protected abstract void UpdatePheromones(List<ITour> solutions);

        /// <summary>
        /// Try to find tour of the problem with length as close as possible to
        /// length of the optimal tour
        /// </summary>
        /// <returns><c>Tour</c> of the problem with length as close as possible to
        /// length of the optimal tour or <c>InfiniteTour</c> if no
        /// tours are found
        /// </returns>
        public ITour Solve(IProblem problem)
        {
            _problem = problem;
            Graph = ActivatorUtilities.CreateInstance<PheromoneGraph>(_serviceProvider, _problem.ToGraph());
            _currBestTour = new InfiniteTour();

            _currIterationCounter.Reset();
            List<ITour> solutions = new();
            //region AlgorithmCore
            while (!_terminationChecker.Terminated())
            {
                solutions = AntColony.GenerateSolutions(Graph);
                solutions = PostprocessSolutions(solutions);
                UpdatePheromones(solutions);

                _currIterationCounter.Inc();
                Console.WriteLine($"{_currIterationCounter}");

                _reinitializer.TryReinitialize(Graph);
            }
            //endregion
            return _currBestTour;
        }

    }
    /// <summary>
    /// <c>AsSolver</c> is class for solving the traveling salesman problem using the Ant System algorithm.
    /// </summary>
    public class AsSolver : SolverBase
    {
        /// <summary>
        /// Construct solver with colony, termination checker and reinitializer
        /// </summary>
        /// <param name="serviceProvider">Provider that provides services for dependency injection</param>
        /// <param name="colony">Colony that generates solutions</param>
        /// <param name="terminationChecker">Checker that checks if solution generation should terminate</param>
        /// <param name="reinitializer">Reinitializer that tries to reinitilize pheromone values if conditions are met</param>
        public AsSolver(IServiceProvider serviceProvider, IColony colony, ITerminationChecker terminationChecker, IReinitializer reinitializer) : base(serviceProvider, colony, terminationChecker, reinitializer) { }

        /// <summary>
        /// Filter solution so that for all solution the pheromones in the graph are updated
        /// </summary>
        /// <param name="solutions">Found solutions by the colony</param>
        /// <returns>Solutions that should have their edge's pheromones updated</returns>
        protected override List<ITour> FilterSolutions(List<ITour> solutions)
        {
            return solutions;
        }

        /// <summary>
        /// For all solution update their edge's pheromone levels and evaporate pheromones in whole graph
        /// </summary>
        /// <param name="solutions">Solution for which to update all their edge's pheromone levels</param>
        protected override void UpdatePheromones(List<ITour> solutions)
        {
            Graph.UpdatePheromonesOnWholeGraph(solutions);
        }
    }

    /// <summary>
    /// <c>AcsSolver</c> is class for solving the traveling salesman problem using the Ant Colony System algorithm.
    /// </summary>
    public class AcsSolver : SolverBase
    {
        /// <summary>
        /// Construct solver with colony, termination checker and reinitializer
        /// </summary>
        /// <param name="serviceProvider">Provider that provides services for dependency injection</param>
        /// <param name="colony">Colony that generates solutions</param>
        /// <param name="terminationChecker">Checker that checks if solution generation should terminate</param>
        /// <param name="reinitializer">Reinitializer that tries to reinitilize pheromone values if conditions are met</param>
        public AcsSolver(IServiceProvider serviceProvider, IColony colony, ITerminationChecker terminationChecker, IReinitializer reinitializer) : base(serviceProvider, colony, terminationChecker, reinitializer) { }

        /// <summary>
        /// Filter solutions so that only minimal solution in this iteration has its edge's pheromones levels updated
        /// </summary>
        /// <param name="solutions">All found solution found by colony</param>
        /// <returns>Minimal solution in this iteration</returns>
        protected override List<ITour> FilterSolutions(List<ITour> solutions)
        {
            List<ITour> res = new();
            if (_minimumLengthSolInIter is Tour)
            {
                res.Add((Tour)_minimumLengthSolInIter);
            }
            return res;
        }

        /// <summary>
        /// For each edge in minimal solution in this iteration update its pheromone amount
        /// </summary>
        /// <param name="solutions">List with only minimal solution in this iteration</param>
        protected override void UpdatePheromones(List<ITour> solutions)
        {
            // Console.WriteLine($"Global update");

            Graph.UpdateGloballyPheromones(solutions);
        }
    }
}