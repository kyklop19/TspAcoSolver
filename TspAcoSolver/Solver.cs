using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace TspAcoSolver
{
    public class Config
    {
        public SolvingParams Read(string path)
        {
            using System.IO.StreamReader r = new(@"C:\Users\havel\OneDrive\Dokumenty\Programming\TspAcoSolver\data\default_config.yaml");
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
    }
    public class Solver
    {
        IProblem problem;
        SolvingParams sParams;

        PheromoneGraph Graph { get; init; }
        Colony AntColony { get; init; }

        ITour currBestTour = new InfiniteTour();

        int iterationCount = 0;

        public Solver(IProblem problem, SolvingParams sParams)
        {
            this.problem = problem;
            this.sParams = sParams;

            Graph = new PheromoneGraph(this.problem.ToGraph(), this.sParams.EvaporationCoef);

            AntColony = new Colony(this.sParams.AntCount);
        }

        bool IsTerminating()
        {
            return iterationCount == 100;
        }

        void UpdatePheromones(Tour[] solutions)
        {
            double[,] pheromoneChange = new double[Graph.VertexCount,Graph.VertexCount];

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
                    pheromoneChange[sol.Vertices[i], sol.Vertices[i + 1]] = updateAmount;
                }
            }

            Graph.UpdatePheromones(pheromoneChange);
        }

        public ITour Solve()
        {
            currBestTour = new InfiniteTour();
            while (!IsTerminating())
            {
                Tour[] solutions = AntColony.GenerateSolutions(Graph);

                foreach (Tour sol in solutions)
                {
                    //TODO: check if valid tour
                    foreach (int ver in sol.Vertices)
                    {
                        Console.Write($"{ver} ");
                    }
                    Console.WriteLine($" Lenght: {sol.Length}");

                    if (sol.Vertices.Count != Graph.AdjList.Length) continue;
                    Console.WriteLine($"Found valid tour");
                    Console.WriteLine($"Best: {currBestTour.Length}");

                    if (sol.Length < currBestTour.Length)
                    {
                        Console.WriteLine($"Found better tour");

                        currBestTour = sol;
                    }
                }

                UpdatePheromones(solutions);

                iterationCount++;
            }
            return currBestTour;
        }

    }
}