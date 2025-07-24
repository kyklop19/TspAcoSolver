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
    }
    public class Solver
    {
        IProblem problem;
        SolvingParams sParams;

        PheromoneGraph Graph { get; init; }
        Ant[] AntColony { get; init; }

        ITour currBestTour = new InfiniteTour();

        int iterationCount = 0;

        public Solver(IProblem problem, SolvingParams sParams)
        {
            this.problem = problem;
            this.sParams = sParams;

            Graph = new PheromoneGraph(this.problem.ToGraph(), this.sParams.EvaporationCoef);

            AntColony = new Ant[this.sParams.AntCount];
            for (int i = 0; i < this.sParams.AntCount; i++)
            {
                AntColony[i] = new Ant();
            }
        }

        bool IsTerminating()
        {
            return iterationCount == 100;
        }
        public ITour Solve()
        {
            currBestTour = new InfiniteTour();

            Tour currTour;
            while (!IsTerminating())
            {
                foreach (Ant ant in AntColony)
                {
                    ant.FindTour(Graph); //TODO: check if valid tour
                    currTour = ant.LastTour;
                    foreach (int ver in currTour.Vertices)
                    {
                        Console.Write($"{ver} ");
                    }
                    Console.WriteLine($" Lenght: {currTour.Length}");

                    if (currTour.Vertices.Count != Graph.AdjList.Length) continue;
                    Console.WriteLine($"Found valid tour");
                    Console.WriteLine($"Best: {currBestTour.Length}");

                    if (currTour.Length < currBestTour.Length)
                    {
                        Console.WriteLine($"Found better tour");

                        currBestTour = currTour;
                    }
                }
                iterationCount++;
            }
            return currBestTour;
        }

    }
}