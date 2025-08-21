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
        public double[,] Pheromones { get; set; }
        public double EvaporationCoef { get; init; }
        public double InitialPheromoneAmount { get; init; }
        public double PheromoneAmount { get; init; }
        public PheromoneGraph(double[,] adjMat, PheromoneParams pheromoneParams) : base(adjMat)
        {
            InitialPheromoneAmount = pheromoneParams.InitialPheromoneAmount;
            EvaporationCoef = pheromoneParams.EvaporationCoef;
            PheromoneAmount = pheromoneParams.PheromoneAmount;

            Pheromones = new double[VertexCount, VertexCount];
            for (int i = 0; i < VertexCount; i++)
            {
                for (int j = 0; j < VertexCount; j++)
                {
                    Pheromones[i, j] = InitialPheromoneAmount;
                }
            }

        }
        public PheromoneGraph(WeightedGraph graph, PheromoneParams pheromoneParams) : this(graph.Weights, pheromoneParams) { }

        public void UpdatePheromonesOnEdge(int source, int target)
        {

        }

        public void UpdatePheromonesOnWholeGraph(List<Tour> solutions)
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
            foreach (Tour sol in solutions)
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
        }
    }
}