using Microsoft.Extensions.Options;

namespace TspAcoSolver
{
    public class Graph
    {
        public int VertexCount { get; init; }
        public List<int>[] AdjList { get; init; }

        public Graph(List<int>[] adjList)
        {
            AdjList = adjList;
            VertexCount = AdjList.Length;
        }
        public Graph(double[,] adjMat)
        {
            List<int>[] adjList = new List<int>[adjMat.GetLength(0)];
            for (int i = 0; i < adjMat.GetLength(0); i++)
            {
                adjList[i] = new();
                for (int j = 0; j < adjMat.GetLength(1); j++)
                {
                    if (adjMat[i, j] != 0)
                    {
                        adjList[i].Add(j);
                    }
                }
            }

            AdjList = adjList;
            VertexCount = AdjList.Length;
        }

        public List<int> Nbrs(int vertex)
        {
            return AdjList[vertex];
        }
    }

    public class WeightedGraph : Graph
    {
        public double[,] Weights { get; init; }
        public WeightedGraph(double[,] adjMat) : base(adjMat)
        {
            Weights = adjMat;
        }

        public double Weight(int from, int to)
        {
            return Weights[from, to];
        }
    }

    public class PheromoneGraph : WeightedGraph
    {
        IPheromoneGraphVisualiser _visualiser;
        public double[,] Pheromones { get; set; }
        public double EvaporationCoef { get; init; }
        public double InitialPheromoneAmount { get; init; }
        public double PheromoneAmount { get; init; }
        public double DecayCoef { get; init; }

        double minimumPheromoneAmount;
        public PheromoneGraph(double[,] adjMat, PheromoneParams pheromoneParams, IPheromoneGraphVisualiser visualiser) : base(adjMat)
        {
            if ((bool)pheromoneParams.CalculateInitialPheromoneAmount)
            {
                Console.WriteLine($"Calculating InitialPheromoneAmount");
                NearestNbrAnt ant = new((IRandom)new RandomGen());
                ant.FindTour(this);
                pheromoneParams.InitialPheromoneAmount = 1 / (VertexCount * ant.LastTour.Length);
                Console.WriteLine($"InitialPheromoneAmount: {pheromoneParams.InitialPheromoneAmount}");

            }

            InitialPheromoneAmount = (double)pheromoneParams.InitialPheromoneAmount;
            EvaporationCoef = (double)pheromoneParams.EvaporationCoef;
            DecayCoef = (double)pheromoneParams.DecayCoef;
            PheromoneAmount = (double)pheromoneParams.PheromoneAmount;

            minimumPheromoneAmount = InitialPheromoneAmount / 1000;

            Pheromones = new double[VertexCount, VertexCount];
            for (int i = 0; i < VertexCount; i++) //TODO: replace with reinitialize
            {
                for (int j = 0; j < VertexCount; j++)
                {
                    Pheromones[i, j] = InitialPheromoneAmount;
                }
            }
            _visualiser = visualiser;
            visualiser.SetGraph(this);

        }
        public PheromoneGraph(WeightedGraph graph, IOptions<PheromoneParams> pheromoneParamsOpt, IPheromoneGraphVisualiser visualiser) : this(graph.Weights, pheromoneParamsOpt.Value, visualiser) { }
        public PheromoneGraph(WeightedGraph graph, PheromoneParams pheromoneParams, IPheromoneGraphVisualiser visualiser) : this(graph.Weights, pheromoneParams, visualiser) { }

        public void UpdatePheromonesOnEdge(int source, int target)
        {

        }

        public void UpdateLocallyPheromones(ITour solution)
        {
            // Console.WriteLine($"{minimumPheromoneAmount > 0}");

            for (int i = 0; i < solution.Vertices.Count - 1; i++)
            {
                int source = solution.Vertices[i];
                int target = solution.Vertices[i + 1];
                Pheromones[source, target] = Math.Max(((1 - DecayCoef) * Pheromones[source, target]) + (DecayCoef * InitialPheromoneAmount), minimumPheromoneAmount);
            }
            if (solution.IsValid())
            {
                int source = solution.Vertices[solution.Vertices.Count - 1];
                int target = solution.Vertices[0];
                Pheromones[source, target] = Math.Max(((1 - DecayCoef) * Pheromones[source, target]) + (DecayCoef * InitialPheromoneAmount), minimumPheromoneAmount);
                //TODO: refactor
            }

            _visualiser.Refresh();
        }

        public void UpdateGloballyPheromones(List<ITour> solutions) //TODO: refactor
        {
            double[,] pheromoneChange = new double[VertexCount, VertexCount];

            for (int i = 0; i < VertexCount; i++)
            {
                for (int j = 0; j < VertexCount; j++)
                {
                    pheromoneChange[i, j] = 0;
                }
            }

            double updateAmount;
            foreach (ITour sol in solutions)
            {
                updateAmount = PheromoneAmount / sol.Length;
                for (int i = 0; i < sol.Vertices.Count - 1; i++)
                {
                    pheromoneChange[sol.Vertices[i], sol.Vertices[i + 1]] += updateAmount;
                }
                pheromoneChange[sol.Vertices[^1], sol.Vertices[0]] += updateAmount;
            }

            for (int i = 0; i < VertexCount; i++)
            {
                for (int j = 0; j < VertexCount; j++)
                {
                    // if (pheromoneChange[i, j] != 0)
                    // {
                    Pheromones[i, j] = Math.Max(((1 - EvaporationCoef) * Pheromones[i, j]) + (EvaporationCoef * pheromoneChange[i, j]), minimumPheromoneAmount);
                    // }
                    // Console.Write($"{Double.Round(Pheromones[i, j], 1)} ");

                }
                // Console.WriteLine($"");

            }

            _visualiser.Refresh();
        }

        public void UpdatePheromonesOnWholeGraph(List<ITour> solutions)
        {
            double[,] pheromoneChange = new double[VertexCount, VertexCount];

            for (int i = 0; i < VertexCount; i++)
            {
                for (int j = 0; j < VertexCount; j++)
                {
                    pheromoneChange[i, j] = 0;
                }
            }

            double updateAmount;
            foreach (ITour sol in solutions)
            {
                updateAmount = PheromoneAmount / sol.Length;
                for (int i = 0; i < sol.Vertices.Count - 1; i++)
                {
                    pheromoneChange[sol.Vertices[i], sol.Vertices[i + 1]] += updateAmount;
                }
            }

            for (int i = 0; i < VertexCount; i++)
            {
                for (int j = 0; j < VertexCount; j++)
                {
                    Pheromones[i, j] = (1 - EvaporationCoef) * Pheromones[i, j] + pheromoneChange[i, j];
                }
            }

            _visualiser.Refresh();
        }

        public void Reinitialize()
        {
            for (int i = 0; i < VertexCount; i++)
            {
                for (int j = 0; j < VertexCount; j++)
                {
                    Pheromones[i, j] = InitialPheromoneAmount;
                }
            }

            _visualiser.Refresh();
        }
    }
}