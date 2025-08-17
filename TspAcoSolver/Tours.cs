namespace TspAcoSolver
{
    public interface ITour
    {
        public double Length { get; }

        public void Add(int vertex);
    }

    public class Tour : ITour
    {
        double _length = 0;

        public double Length
        {
            get
            {
                if (IsValid())
                {
                    return _length + _graph.Weight(Vertices[Vertices.Count-1], Vertices[0]);
                }
                else
                {
                    throw new Exception();
                }
            }
        }
        public List<int> Vertices { get; init; }

        WeightedGraph _graph;

        HashSet<int> unvisitedVertices = new();

        public Tour(WeightedGraph graph)
        {
            Vertices = new();
            _graph = graph;

            for (int i = 0; i < _graph.VertexCount; i++)
            {
                unvisitedVertices.Add(i);
            }
        }

        public void Add(int vertex)
        {
            if (Vertices.Count != 0)
            {
                if (!NextPossibleVertices().Contains(vertex))
                {
                    throw new Exception(); //TODO: refactor
                }
                _length += _graph.Weight(Vertices[Vertices.Count - 1], vertex);
            }
            unvisitedVertices.Remove(vertex);
            Vertices.Add(vertex);
        }

        public List<int> NextPossibleVertices() //TODO: store in atr
        {
            List<int> nbrs = _graph.Nbrs(Vertices[Vertices.Count - 1]);
            List<int> unvisited_nbrs = new List<int>();
            foreach (int nbr in nbrs)
            {
                if (unvisitedVertices.Contains(nbr))
                {
                    unvisited_nbrs.Add(nbr);
                }
            }
            return unvisited_nbrs;
        }

        public bool HasDeadEnd()
        {
            return NextPossibleVertices().Count == 0;
        }

        public bool HasAllVerteces()
        {
            return Vertices.Count == _graph.VertexCount;
        }


        public bool CanConnectStartAndFinish()
        {
            return _graph.Weight(Vertices[Vertices.Count - 1], Vertices[0]) != 0;
        }

        public bool IsValid()
        {
            return HasAllVerteces() && CanConnectStartAndFinish();
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