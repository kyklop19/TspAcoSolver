namespace TspAcoSolver
{
    public struct Nbr
    {
        public int Vertex { get; set; }
        public double Dist { get; set; }

        public Nbr(int vertex, double dist)
        {
            Vertex = vertex;
            Dist = dist;
        }
    }
    public class Graph
    {

    }

    public class WeightedGraph : Graph
    {
        public int VertexCount { get; init; }
        public List<Nbr>[] AdjList { get; init; }

        public WeightedGraph(List<Nbr>[] adjList)
        {
            AdjList = adjList;
            VertexCount = AdjList.Length;
        }

        public List<Nbr> Nbrs(int vertex)
        {
            return AdjList[vertex];
        }
    }

    public class PheromoneGraph : WeightedGraph
    {
        public double[,] Pheromones { get; set; }
        public double EvaporationCoef { get; init; }
        public PheromoneGraph(List<Nbr>[] adjList, double evaporationCoef) : base(adjList)
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
        public PheromoneGraph(WeightedGraph graph, double evaporationCoef) : this(graph.AdjList, evaporationCoef) { }

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

    public interface ITour
    {
        public double Length { get; }

        public void Add(Nbr nbr);
    }

    public class Tour : ITour
    {
        double _length = 0;

        public double Length { get => _length; }
        public List<int> Vertices { get; init; }


        public Tour()
        {
            Vertices = new();
        }

        public void Add(Nbr nbr)
        {
            _length += nbr.Dist;
            Vertices.Add(nbr.Vertex);
        }

    }
    public class InfiniteTour : ITour
    {
        public double Length { get => Double.PositiveInfinity; }

        public void Add(Nbr nbr) { }
    }
}