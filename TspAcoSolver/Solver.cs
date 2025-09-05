using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace TspAcoSolver
{
    delegate bool TerminationRule();
    public abstract class SolverBase
    {
        IProblem _problem;
        protected SolvingParams _sParams;

        protected PheromoneGraph Graph;
        protected IColony AntColony { get; init; }

        ITour _currBestTour = new InfiniteTour();

        int _currIterationCount = 0;
        public int CurrIterationCount { get => _currIterationCount; }

        protected ITour _minimumLengthSolInIter = new InfiniteTour();
        int _inRowWithinPercentageCount = 0;

        TerminationRule _terminated;

        public SolverBase(IColony colony, IOptions<SolvingParams> sParams)
        {
            AntColony = colony;
            _sParams = sParams.Value;

            switch (_sParams.TerminationParams.TerminationRule)
            {
                case "fixed":
                    _terminated = ReachedIterationCount;
                    break;
                case "within_percentage":
                    _terminated = ReachedInRowCountWithinPercentage;
                    break;
            }
        }

        bool ReachedInRowCountWithinPercentage()
        {
            double ceilingLength = _currBestTour.Length * (1 + (_sParams.TerminationParams.CeilingPercentage / 100));


            if (_minimumLengthSolInIter.Length <= ceilingLength)
            {
                _inRowWithinPercentageCount++;
            }
            else
            {
                _inRowWithinPercentageCount = 0;
            }
            return _inRowWithinPercentageCount >= _sParams.TerminationParams.InRowTerminationCount;
        }

        bool ReachedIterationCount()
        {
            return _currIterationCount == _sParams.TerminationParams.IterationCount;
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
                }
            }
            if (_minimumLengthSolInIter.Length < _currBestTour.Length)
            {
                Console.WriteLine($"Found better tour");

                _currBestTour = _minimumLengthSolInIter;
                _inRowWithinPercentageCount = 0;
                Console.WriteLine($"Best: {_currBestTour.Length}");
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
            Graph = new PheromoneGraph(_problem.ToGraph(), _sParams.PheromoneParams);
            _currBestTour = new InfiniteTour();

            _currIterationCount = 0;
            List<Tour> solutions = new();
            while (!_terminated())
            {
                solutions = AntColony.GenerateSolutions(Graph);
                solutions = PostprocessSolutions(solutions);
                UpdatePheromones(solutions);

                _currIterationCount++;
                Console.WriteLine($"{_currIterationCount}");

                if (_currIterationCount % 100 == 0)
                {
                    Graph.Reinitialize();
                }
            }
            return _currBestTour;
        }

    }
    /// <summary>
    /// <c>AsSolver</c> is class for solving the traveling salesman problem using the Ant System algorithm.
    /// </summary>
    public class AsSolver : SolverBase
    {
        public AsSolver(IColony colony, IOptions<SolvingParams> sParams) : base(colony, sParams){}

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
        public AcsSolver(IColony colony, IOptions<SolvingParams> sParams) : base(colony, sParams){}

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