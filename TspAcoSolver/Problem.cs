namespace TspAcoSolver
{
    public interface IProblem
    {
        public WeightedGraph ToGraph();
    }

    public struct Pathway
    {
        public int From { get; set; }
        public int To { get; set; }
        public double Dist { get; set; }
        public Pathway(int from, int to, double dist)
        {
            From = from;
            To = to;
            Dist = dist;
        }
    }

    public class TravelingSalesmanProblem : IProblem
    {
        List<Pathway> _pathways = new();
        public int VertexCount { get; init; }

        public TravelingSalesmanProblem(int vertexCount, List<Pathway> pathways)
        {
            VertexCount = vertexCount;
            _pathways = pathways;
        }
        public WeightedGraph ToGraph()
        {
            double[,] adjMat = new double[VertexCount, VertexCount];
            for (int i = 0; i < VertexCount; i++)
            {
                for (int j = 0; j < VertexCount; j++)
                {
                    adjMat[i, j] = 0;
                }
            }
            foreach (Pathway pathway in _pathways)
            {
                adjMat[pathway.From, pathway.To] = pathway.Dist;
            }
            return new WeightedGraph(adjMat);
        }
    }

    public abstract class SpaceTravelingSalesmanProblem : IProblem
    {
        public List<double[]> Coords { get; init; }

        public int Size { get => Coords.Count; }

        public int Dimension { get; init; }

        public SpaceTravelingSalesmanProblem(int dimension, List<double[]> coords)
        {
            Dimension = dimension;
            this.Coords = coords;
        }
        public abstract double Distance(double[] coord1, double[] coord2);
        public WeightedGraph ToGraph()
        {
            double[,] adjMat = new double[Size, Size];
            for (int i = 0; i < Size; i++)
            {
                for (int j = 0; j < Size; j++)
                {
                    adjMat[i, j] = Distance(Coords[i], Coords[j]);
                }
            }
            return new WeightedGraph(adjMat);
        }
    }

    public class EuclidTravelingSalesmanProblem : SpaceTravelingSalesmanProblem
    {

        public EuclidTravelingSalesmanProblem(int dimension, List<double[]> coords) : base(dimension, coords){}
        public override double Distance(double[] coord1, double[] coord2)
        {
            double sum = 0;
            for (int i = 0; i < Dimension; i++)
            {
                sum += Math.Pow(coord1[i] - coord2[i], 2);
            }
            return Math.Sqrt(sum);
        }
    }
}