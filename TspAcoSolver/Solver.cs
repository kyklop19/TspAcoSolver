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
    public class SolvingParams
    {
        public double EvaporationCoef { get; set; }
        public int AntCount { get; set; }
        public int ThreadCount { get; set; }
        public int PheromoneAmount { get; set; }
        public int IterationCount { get; set; }
        public double TrailLevelFactor { get; set; }
        public double AttractivenessFactor { get; set; }
        public string TerminationRule { get; set; }
        public double CeilingPercentage { get; set; }
        public int InRowTerminationCount { get; set; }
    }

    delegate bool TerminationRule();
    public class Solver
    {
        IProblem problem;
        SolvingParams sParams;

        PheromoneGraph Graph { get; init; }
        Colony AntColony { get; init; }

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

            Graph = new PheromoneGraph(this.problem.ToGraph(), this.sParams.EvaporationCoef);

            AntColony = new Colony(this.sParams.AntCount, this.sParams.ThreadCount ,this.sParams.TrailLevelFactor, this.sParams.AttractivenessFactor);

            switch (this.sParams.TerminationRule)
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
            double ceilingLength = _currBestTour.Length * (1 + (sParams.CeilingPercentage / 100));


            if (_minimumLengthSolInIter.Length <= ceilingLength)
            {
                _inRowWithinPercentageCount++;
            }
            else
            {
                _inRowWithinPercentageCount = 0;
            }
            return _inRowWithinPercentageCount >= sParams.InRowTerminationCount;
        }

        bool ReachedIterationCount()
        {
            return _currIterationCount == sParams.IterationCount;
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

        void UpdatePheromones(List<Tour> solutions)
        {
            double[,] pheromoneChange = new double[Graph.VertexCount, Graph.VertexCount];

            for (int i = 0; i < Graph.VertexCount; i++)
            {
                for (int j = 0; j < Graph.VertexCount; j++)
                {
                    pheromoneChange[i, j] = 0;
                }
            }
            double updateAmount;
            foreach (Tour sol in solutions)
            {
                updateAmount = sParams.PheromoneAmount / sol.Length;
                for (int i = 0; i < sol.Vertices.Count - 1; i++)
                {
                    pheromoneChange[sol.Vertices[i], sol.Vertices[i + 1]] += updateAmount;
                }
            }

            Graph.UpdatePheromones(pheromoneChange);
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
                UpdatePheromones(solutions);

                _currIterationCount++;
            }
            return _currBestTour;
        }

    }
}