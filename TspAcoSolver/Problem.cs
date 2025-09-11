namespace TspAcoSolver
{
    public interface IProblem
    {
        public int CityCount { get; }
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
        public int CityCount { get; init; }

        public TravelingSalesmanProblem(int vertexCount, List<Pathway> pathways)
        {
            CityCount = vertexCount;
            _pathways = pathways;
        }
        public WeightedGraph ToGraph()
        {
            double[,] adjMat = new double[CityCount, CityCount];
            for (int i = 0; i < CityCount; i++)
            {
                for (int j = 0; j < CityCount; j++)
                {
                    adjMat[i, j] = Double.NaN;
                }
            }
            foreach (Pathway pathway in _pathways)
            {
                adjMat[pathway.From, pathway.To] = pathway.Dist;
            }
            return new WeightedGraph(adjMat);
        }
    }

    /// <summary>
    /// Abstract class representing traveling salesman problem which is specified
    /// by providing coordinates of each city. Lengths of edges of graph are calculated
    /// using <c>Distance</c> method.
    /// </summary>
    public abstract class SpaceTravelingSalesmanProblem : IProblem
    {
        public List<double[]> Coords { get; init; }

        public int CityCount { get => Coords.Count; }

        public int Dimension { get; init; }

        public SpaceTravelingSalesmanProblem(int dimension, List<double[]> coords)
        {
            Dimension = dimension;
            this.Coords = coords;
        }
        /// <summary>
        /// Calculate distance from <c>coord1</c> to <c>coord2</c>
        /// </summary>
        /// <param name="coord1">Start coordinate</param>
        /// <param name="coord2">Finish coordinate</param>
        /// <returns>Distance from <c>coord1</c> to <c>coord2</c></returns>
        public abstract double Distance(double[] coord1, double[] coord2);
        public WeightedGraph ToGraph()
        {
            double[,] adjMat = new double[CityCount, CityCount];
            for (int i = 0; i < CityCount; i++)
            {
                for (int j = 0; j < CityCount; j++)
                {
                    adjMat[i, j] = Distance(Coords[i], Coords[j]);
                }
            }
            return new WeightedGraph(adjMat);
        }
    }

    /// <summary>
    /// Class representing traveling salesman problem which is specified by
    /// providing coordinates of each city in Euclidean n-space.
    /// </summary>
    public class EuclidTravelingSalesmanProblem : SpaceTravelingSalesmanProblem
    {

        public EuclidTravelingSalesmanProblem(int dimension, List<double[]> coords) : base(dimension, coords) { }
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