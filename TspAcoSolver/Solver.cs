using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace TspAcoSolver
{
    public class Config
    {
        public SolvingParams Read(string path)
        {
            using System.IO.StreamReader r = new(@"..\..\..\..\data\default_config.yaml");
            IDeserializer deserializer = new DeserializerBuilder()
                .WithNamingConvention(UnderscoredNamingConvention.Instance)  // see height_in_inches in sample yml
                .Build();

            SolvingParams p = deserializer.Deserialize<SolvingParams>(r.ReadToEnd());
            return p;
        }
    }

    public class PheromoneParams
    {
        public double EvaporationCoef { get; set; }
        public double DecayCoef { get; set; }
        public double InitialPheromoneAmount { get; set; }
        public double PheromoneAmount { get; set; }
    }
    public class ColonyParams
    {
        public int AntCount { get; set; }
        public int ThreadCount { get; set; }
        public double TrailLevelFactor { get; set; }
        public double AttractivenessFactor { get; set; }
        public double ExploProportionConst { get; set; }
    }
    public class TerminationParams
    {
        public string TerminationRule { get; set; }
        public int IterationCount { get; set; }
        public double CeilingPercentage { get; set; }
        public int InRowTerminationCount { get; set; }
    }

    public class SolvingParams
    {
        public PheromoneParams PheromoneParams { get; set; }
        public TerminationParams TerminationParams { get; set; }
        public ColonyParams ColonyParams { get; set; }
    }

    delegate bool TerminationRule();
    public class Solver
    {
        IProblem problem;
        SolvingParams sParams;

        PheromoneGraph Graph { get; init; }
        AsColony AntColony { get; init; }

        ITour _currBestTour = new InfiniteTour();

        int _currIterationCount = 0;
        public int CurrIterationCount { get => _currIterationCount; }

        ITour _minimumLengthSolInIter = new InfiniteTour();
        int _inRowWithinPercentageCount = 0;

        TerminationRule _terminated;

        public Solver(IProblem problem, SolvingParams sParams)
        {
            this.problem = problem;
            this.sParams = sParams;

            Graph = new PheromoneGraph(this.problem.ToGraph(), this.sParams.PheromoneParams);

            AntColony = new AsColony(this.sParams.ColonyParams);

            switch (this.sParams.TerminationParams.TerminationRule)
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
            double ceilingLength = _currBestTour.Length * (1 + (sParams.TerminationParams.CeilingPercentage / 100));


            if (_minimumLengthSolInIter.Length <= ceilingLength)
            {
                _inRowWithinPercentageCount++;
            }
            else
            {
                _inRowWithinPercentageCount = 0;
            }
            return _inRowWithinPercentageCount >= sParams.TerminationParams.InRowTerminationCount;
        }

        bool ReachedIterationCount()
        {
            return _currIterationCount == sParams.TerminationParams.IterationCount;
        }

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

            return solutions;
        }

        public ITour Solve()
        {
            _currBestTour = new InfiniteTour();

            _currIterationCount = 0;
            List<Tour> solutions = new();
            while (!_terminated())
            {
                solutions = AntColony.GenerateSolutions(Graph);
                solutions = PostprocessSolutions(solutions);
                Graph.UpdatePheromonesOnWholeGraph(solutions);

                _currIterationCount++;
            }
            return _currBestTour;
        }

    }
}