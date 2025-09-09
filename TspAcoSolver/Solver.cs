using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace TspAcoSolver
{
    public interface ISolver
    {
        public int CurrIterationCount { get; }
        public ITour Solve(IProblem problem);
    }
    public abstract class SolverBase(IServiceProvider _serviceProvider) : ISolver
    {
        IProblem _problem;
        protected SolvingParams _sParams;

        protected PheromoneGraph Graph;
        protected IColony AntColony { get; init; }

        ITour _currBestTour = new InfiniteTour();

        Counter _currIterationCounter = new();
        Counter _stagnationCounter = new();
        public int CurrIterationCount { get => _currIterationCounter.Value; }

        protected ITour _minimumLengthSolInIter = new InfiniteTour();

        ITerminationChecker _terminationChecker;
        IReinitializer _reinitializer;

        public SolverBase(IServiceProvider serviceProvider, IColony colony, ITerminationChecker terminationChecker, IReinitializer reinitializer, IOptions<SolvingParams> sParams) : this(serviceProvider)
        {
            AntColony = colony;
            _sParams = sParams.Value;

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
        protected abstract List<Tour> FilterSolutions(List<Tour> solutions);

        List<Tour> PostprocessSolutions(List<Tour> solutions)
        {
            _minimumLengthSolInIter = new InfiniteTour();
            foreach (Tour sol in solutions)
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
        protected abstract void UpdatePheromones(List<Tour> solutions);

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
            Graph = ActivatorUtilities.CreateInstance<PheromoneGraph>(_serviceProvider, _problem.ToGraph(), _sParams.PheromoneParams); //TODO: DI pParams and remove DI sParams
            _currBestTour = new InfiniteTour();

            _currIterationCounter.Reset();
            List<Tour> solutions = new();
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
        public AsSolver(IServiceProvider serviceProvider, IColony colony, ITerminationChecker terminationChecker, IReinitializer reinitializer, IOptions<SolvingParams> sParams) : base(serviceProvider, colony, terminationChecker, reinitializer, sParams){}

        protected override List<Tour> FilterSolutions(List<Tour> solutions)
        {
            return solutions;
        }

        protected override void UpdatePheromones(List<Tour> solutions)
        {
            Graph.UpdatePheromonesOnWholeGraph(solutions);
        }
    }

    /// <summary>
    /// <c>AcsSolver</c> is class for solving the traveling salesman problem using the Ant Colony System algorithm.
    /// </summary>
    public class AcsSolver : SolverBase
    {
        public AcsSolver(IServiceProvider serviceProvider, IColony colony, ITerminationChecker terminationChecker, IReinitializer reinitializer, IOptions<SolvingParams> sParams) : base(serviceProvider, colony, terminationChecker, reinitializer, sParams){}

        protected override List<Tour> FilterSolutions(List<Tour> solutions)
        {
            List<Tour> res = new();
            if (_minimumLengthSolInIter is Tour)
            {
                res.Add((Tour)_minimumLengthSolInIter);
            }
            return res;
        }

        protected override void UpdatePheromones(List<Tour> solutions)
        {
            // Console.WriteLine($"Global update");

            Graph.UpdateGloballyPheromones(solutions);
        }
    }
}