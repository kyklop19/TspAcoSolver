namespace TspAcoSolver
{
    delegate bool TerminationRule();
    public abstract class SolverBase
    {
        IProblem _problem;
        protected SolvingParams _sParams;

        protected PheromoneGraph Graph { get; init; }
        protected ColonyBase AntColony { get; init; }

        ITour _currBestTour = new InfiniteTour();

        int _currIterationCount = 0;
        public int CurrIterationCount { get => _currIterationCount; }

        protected ITour _minimumLengthSolInIter = new InfiniteTour();
        int _inRowWithinPercentageCount = 0;

        TerminationRule _terminated;

        public SolverBase(IProblem problem, SolvingParams sParams)
        {
            _problem = problem;
            _sParams = sParams;


            Graph = new PheromoneGraph(_problem.ToGraph(), _sParams.PheromoneParams);

            if (_sParams.PheromoneParams.CalculateInitialPheromoneAmount)
            {
                Console.WriteLine($"Calculating InitialPheromoneAmount");
                NearestNbrAnt ant = new();
                ant.FindTour(Graph);
                _sParams.PheromoneParams.InitialPheromoneAmount = 1 / (problem.CityCount * ant.LastTour.Length);
                Console.WriteLine($"InitialPheromoneAmount: {_sParams.PheromoneParams.InitialPheromoneAmount}");

            }

            switch (this._sParams.TerminationParams.TerminationRule)
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

        protected abstract void UpdatePheromones(List<Tour> solutions);

        public ITour Solve()
        {
            _currBestTour = new InfiniteTour();

            _currIterationCount = 0;
            List<Tour> solutions = new();
            while (!_terminated())
            {
                solutions = AntColony.GenerateSolutions(Graph);
                solutions = PostprocessSolutions(solutions);
                UpdatePheromones(solutions);

                _currIterationCount++;
            }
            return _currBestTour;
        }

    }

    public class AsSolver : SolverBase
    {
        public AsSolver(IProblem problem, SolvingParams sParams) : base(problem, sParams)
        {
            AntColony = new AsColony(this._sParams.ColonyParams);
        }

        protected override List<Tour> FilterSolutions(List<Tour> solutions)
        {
            return solutions;
        }

        protected override void UpdatePheromones(List<Tour> solutions)
        {
            Graph.UpdatePheromonesOnWholeGraph(solutions);
        }
    }
    public class AcsSolver : SolverBase
    {
        public AcsSolver(IProblem problem, SolvingParams sParams) : base(problem, sParams)
        {
            AntColony = new AcsColony(this._sParams.ColonyParams);
        }

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
            Console.WriteLine($"Global update");

            Graph.UpdateGloballyPheromones(solutions);
        }
    }
}