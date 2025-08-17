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
        public PheromoneGraph(double[,] adjMat, double evaporationCoef) : base(adjMat)
        {
            const double StartingPheromoneCount = 100;
            Pheromones = new double[VertexCount, VertexCount];
            for (int i = 0; i < VertexCount; i++)
            {
                for (int j = 0; j < VertexCount; j++)
                {
                    Pheromones[i, j] = StartingPheromoneCount;
                }
            }

            EvaporationCoef = evaporationCoef;
        }
        public PheromoneGraph(WeightedGraph graph, double evaporationCoef) : this(graph.Weights, evaporationCoef) { }

        public void UpdatePheromones(double[,] pheromoneChange)
        {
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