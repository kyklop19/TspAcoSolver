namespace TspAcoSolver
{
    /// <summary>
    /// Interface for classes representing tour in traveling salesman problem
    /// </summary>
    public interface ITour
    {
        /// <summary>
        /// Length of tour in the problem
        /// </summary>
        public double Length { get; }

        /// <summary>
        /// Add <c>vertex</c> to the tour
        /// </summary>
        /// <param name="vertex">Vertex to be next in order in the tour</param>
        public void Add(int vertex);
    }

    public class IncompleteTourException : Exception
    {
        public IncompleteTourException()
        {
        }

        public IncompleteTourException(string message)
            : base(message)
        {
        }

        public IncompleteTourException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
    public class DuplicateVertexException : Exception
    {
        public DuplicateVertexException()
        {
        }

        public DuplicateVertexException(string message)
            : base(message)
        {
        }

        public DuplicateVertexException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }

    /// <summary>
    /// Represents tour in a graph of tsp
    /// </summary>
    public class Tour : ITour
    {
        double _length = 0;

        /// <summary>
        /// Length of complete tour
        /// </summary>
        /// <value>Sum of lengths of all edges contained in the tour</value>
        /// <exception cref="IncompleteTourException">Thrown when the tour is not complete</exception>
        public double Length
        {
            get
            {
                if (IsValid())
                {
                    return _length + _graph.Weight(Vertices[Vertices.Count - 1], Vertices[0]);
                }
                else
                {
                    throw new IncompleteTourException("Length of incomplete tour is not defined");
                }
            }
        }

        /// <summary>
        /// All vertices in tour
        /// </summary>
        /// <value>List of vertices that are contained in tour</value>
        public List<int> Vertices { get; init; }

        WeightedGraph _graph;

        HashSet<int> _unvisitedVertices = new();

        /// <summary>
        /// Construct tour whose vertices are part of the <c>graph</c>
        /// </summary>
        /// <param name="graph">Graph on which the tour is constructed</param>
        public Tour(WeightedGraph graph)
        {
            Vertices = new();
            _graph = graph;

            for (int i = 0; i < _graph.VertexCount; i++)
            {
                _unvisitedVertices.Add(i);
            }
        }

        /// <summary>
        /// Add neighboring vertex of the last added vertex
        /// </summary>
        /// <param name="vertex">Neighboring vertex of the last added vertex</param>
        /// <exception cref="DuplicateVertexException">Thrown when <c>vertex</c> is already part of the tour</exception>
        public void Add(int vertex)
        {
            if (Vertices.Count != 0)
            {
                if (!NextPossibleVertices().Contains(vertex))
                {
                    throw new DuplicateVertexException($"Vertex {vertex} has been already added to tour"); //TODO: refactor
                    //TODO: change duplicate, complete, no possible edge
                }
                _length += _graph.Weight(Vertices[Vertices.Count - 1], vertex);
            }
            _unvisitedVertices.Remove(vertex);
            Vertices.Add(vertex);
        }

        public List<int> NextPossibleVertices() //TODO: store in atr
        {
            List<int> nbrs = _graph.Nbrs(Vertices[Vertices.Count - 1]);
            List<int> unvisited_nbrs = new List<int>();
            foreach (int nbr in nbrs)
            {
                if (_unvisitedVertices.Contains(nbr))
                {
                    unvisited_nbrs.Add(nbr);
                }
            }
            return unvisited_nbrs;
        }

        /// <summary>
        /// Checks if there are any neighboring vertices that could be added
        /// </summary>
        /// <returns>True if there are no vertices that could be added</returns>
        public bool HasDeadEnd()
        {
            return NextPossibleVertices().Count == 0;
        }

        /// <summary>
        /// Check if the tour is complete (contains all vertices of graph)
        /// </summary>
        /// <returns>True if the tour contains all vertices of graph</returns>
        public bool HasAllVertices()
        {
            return Vertices.Count == _graph.VertexCount;
        }

        /// <summary>
        /// Check if there exists edge between first and last added vertices
        /// </summary>
        /// <returns>True if such edge exists</returns>
        public bool CanConnectStartAndFinish()
        {
            return _graph.Weight(Vertices[Vertices.Count - 1], Vertices[0]) != Double.NaN;
        }

        /// <summary>
        /// Checks if the tour is valid (has all vertices and between all of them there are edges)
        /// </summary>
        /// <returns>True if is valid</returns>
        public bool IsValid()
        {
            return HasAllVertices() && CanConnectStartAndFinish();
        }

        public override string ToString()
        {
            string res = "";

            foreach (int vertex in Vertices)
            {
                res += $"{vertex} ";
            }
            return res;
        }

    }
    public class InfiniteTour : ITour
    {
        public double Length { get => Double.PositiveInfinity; }

        public void Add(int vertex) { }
    }
}