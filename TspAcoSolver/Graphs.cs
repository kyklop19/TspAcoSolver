using Microsoft.Extensions.Options;

namespace TspAcoSolver
{
    /// <summary>
    /// Class for representing basic directed graph from graph theory
    /// </summary>
    public class Graph
    {
        /// <summary>
        /// Number of vertices the graph contains
        /// </summary>
        public int VertexCount { get; init; }
        /// <summary>
        /// Adjacency list of all vertices
        /// </summary>
        public List<int>[] AdjList { get; init; }

        /// <summary>
        /// Construct graph from <c>adjList</c> adjacency list
        /// </summary>
        /// <param name="adjList">Adjacency list of graph that this class will represent</param>
        public Graph(List<int>[] adjList)
        {
            AdjList = adjList;
            VertexCount = AdjList.Length;
        }
        /// <summary>
        /// Construct graph from <c>adjMat</c> adjacency matrix
        /// </summary>
        /// <param name="adjMat">Adjacency matrix of graph that this class will represent</param>
        public Graph(double[,] adjMat)
        {
            List<int>[] adjList = new List<int>[adjMat.GetLength(0)];
            for (int i = 0; i < adjMat.GetLength(0); i++)
            {
                adjList[i] = new();
                for (int j = 0; j < adjMat.GetLength(1); j++)
                {

                    if (!Double.IsNaN(adjMat[i, j]))
                    {
                        adjList[i].Add(j);
                    }
                }
            }

            AdjList = adjList;
            VertexCount = AdjList.Length;
        }

        /// <summary>
        /// Return all neighboring vertices of <c>vertex</c>
        /// </summary>
        /// <param name="vertex">Vertex to which to find all its neighbors</param>
        /// <returns>All neighboring vertices of <c>vertex</c></returns>
        public List<int> Nbrs(int vertex)
        {
            return AdjList[vertex];
        }
    }

    /// <summary>
    /// Class for representing directed graph from graph theory where each edge has stored weight as double
    /// </summary>
    public class WeightedGraph : Graph
    {
        /// <summary>
        /// Matrix $a$ where each member $a_{ij}$ is weight of edge from $i$ to $j$
        /// </summary>
        public double[,] Weights { get; init; }

        /// <summary>
        /// Construct graph from <c>adjMat</c> adjacency/distance matrix $a$ where each member $a_{ij}$ is weight of edge from $i$ to $j$
        /// </summary>
        /// <param name="adjMat">Adjacency/distance matrix of graph that this class will represent</param>
        public WeightedGraph(double[,] adjMat) : base(adjMat)
        {
            Weights = adjMat;
        }

        /// <summary>
        /// Return weight of edge from <c>from</c> to <c>to</c>
        /// </summary>
        /// <param name="from">Source vertex of edge whose weight should be returned</param>
        /// <param name="to">Target vertex of edge whose weight should be returned</param>
        /// <returns>Weight of edge from <c>from</c> to <c>to</c></returns>
        public double Weight(int from, int to)
        {
            return Weights[from, to];
        }
    }

    /// <summary>
    /// Class for dynamically representing directed graph from graph theory where each edge has stored weight and pheromone level both as double
    /// </summary>
    public class PheromoneGraph : WeightedGraph
    {
        IPheromoneGraphVisualiser _visualiser;

        /// <summary>
        /// Matrix $a$ where each member $a_{ij}$ is pheromone level of edge from $i$ to $j$
        /// </summary>
        public double[,] Pheromones { get; set; }

        /// <summary>
        /// Parameter influencing how many pheromones are evaporated/deleted during update
        /// </summary>
        public double EvaporationCoef { get; init; }
        /// <summary>
        /// Number of pheromones that are set for all edges during initialization
        /// </summary>
        public double InitialPheromoneAmount { get; init; }
        /// <summary>
        /// Parameter influencing how many pheromones are deposited during update
        /// </summary>
        public double PheromoneAmount { get; init; }
        /// <summary>
        /// Parameter influencing how many pheromones are decayed/deleted during update
        /// </summary>
        public double DecayCoef { get; init; }

        double minimumPheromoneAmount;

        /// <summary>
        /// Construct from adjacency matrix, parameters influencing pheromone updates and visualizer
        /// </summary>
        /// <param name="adjMat">Adjacency/distance matrix of graph that this class will represent</param>
        /// <param name="pheromoneParams">Parameters that influence how pheromones are updated</param>
        /// <param name="visualiser">Visualizer that visualizes this graph</param>
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
        /// <summary>
        /// Construct from weighted graph, options with parameters influencing pheromone updates and visualizer
        /// </summary>
        /// <param name="graph">Weighted graph that this class will represent with its pheromones</param>
        /// <param name="pheromoneParamsOpt">Options with parameters that influence how pheromones are updated</param>
        /// <param name="visualiser">Visualizer that visualizes this graph</param>
        public PheromoneGraph(WeightedGraph graph, IOptions<PheromoneParams> pheromoneParamsOpt, IPheromoneGraphVisualiser visualiser) : this(graph.Weights, pheromoneParamsOpt.Value, visualiser) { }
        /// <summary>
        /// Construct from weighted graph, options with parameters influencing pheromone updates and visualizer
        /// </summary>
        /// <param name="graph">Weighted graph that this class will represent with its pheromones</param>
        /// <param name="pheromoneParams">Parameters that influence how pheromones are updated</param>
        /// <param name="visualiser">Visualizer that visualizes this graph</param>
        public PheromoneGraph(WeightedGraph graph, PheromoneParams pheromoneParams, IPheromoneGraphVisualiser visualiser) : this(graph.Weights, pheromoneParams, visualiser) { }

        /// <summary>
        /// Update pheromones on all edges that are part of <c>solution</c>.
        ///
        /// This update method is part of the Ant Colony System algorithm.
        /// </summary>
        /// <param name="solution">Tour on which all edges have their pheromones updated</param>
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

        /// <summary>
        /// Update pheromones on all edges. On all of them evaporate/delete
        /// pheromones depending on the parameters. Those that are part of any
        /// of the tours in <c>solutions</c> have also some pheromones deposited
        /// on them depending on the parameters.
        /// <br/>
        /// This update method is part of the Ant Colony System algorithm.
        /// </summary>
        /// <param name="solutions">Tours on which to deposit some amount of pheromones</param>
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

        /// <summary>
        /// Update pheromones on all edges. On all of them evaporate/delete
        /// pheromones depending on the parameters. Those that are part of any
        /// of the tours in <c>solutions</c> have also some pheromones deposited
        /// on them depending on the parameters.
        /// <br/>
        /// This update method is part of the Ant System algorithm.
        /// </summary>
        /// <param name="solutions">Tours on which to deposit some amount of pheromones</param>
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
                    Pheromones[i, j] = Math.Max((1 - EvaporationCoef) * Pheromones[i, j] + pheromoneChange[i, j], minimumPheromoneAmount);
                }
            }

            _visualiser.Refresh();
        }

        /// <summary>
        /// Set for each edges its pheromone level to the <c>InitialPheromoneAmount</c>.
        /// </summary>
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