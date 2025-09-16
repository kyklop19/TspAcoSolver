namespace TspAcoSolver
{
    /// <summary>
    /// Interface for representing traveling salesman problem
    /// </summary>
    public interface IProblem
    {
        /// <summary>
        /// Size/number of cities that given traveling salesman problem contains
        /// </summary>
        public int CityCount { get; }

        /// <summary>
        /// Convert given traveling salesman problem to weighted graph
        /// </summary>
        /// <returns>Weighted graph where vertices are the cities of the problem and edges are paths between them</returns>
        public WeightedGraph ToGraph();
    }

    /// <summary>
    /// Struct representing path between cities in traveling salesman problem
    /// </summary>
    public struct Pathway
    {
        /// <summary>
        /// City from which the path goes
        /// </summary>
        public int From { get; set; }
        /// <summary>
        /// City to which the path comes
        /// </summary>
        public int To { get; set; }
        /// <summary>
        /// The length of the path
        /// </summary>
        public double Dist { get; set; }
        /// <summary>
        /// Construct from all parameters
        /// </summary>
        /// <param name="from">City from which the path goes</param>
        /// <param name="to">City to which the path comes</param>
        /// <param name="dist">The length of the path</param>
        public Pathway(int from, int to, double dist)
        {
            From = from;
            To = to;
            Dist = dist;
        }
    }

    /// <summary>
    /// Class for representing traveling salesman problem where every path between cities is explicitly given
    /// </summary>
    public class TravelingSalesmanProblem : IProblem
    {
        List<Pathway> _pathways = new();
        public int CityCount { get; init; }

        /// <summary>
        /// Construct from list of pathways at its count
        /// </summary>
        /// <param name="vertexCount">Count of pathways</param>
        /// <param name="pathways">List of pathways between cities</param>
        public TravelingSalesmanProblem(int vertexCount, List<Pathway> pathways)
        {
            CityCount = vertexCount;
            _pathways = pathways;
        }

        /// <summary>
        /// Convert given traveling salesman problem to weighted graph
        /// </summary>
        /// <returns>Weighted graph where vertices are the cities of the problem and edges are pathways between them</returns>
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
        /// <summary>
        /// Coordinates of all cities in the problem
        /// </summary>
        public List<double[]> Coords { get; init; }

        /// <summary>
        /// Size/number of cities that given traveling salesman problem contains
        /// </summary>
        public int CityCount { get => Coords.Count; }

        public int Dimension { get; init; }

        /// <summary>
        /// Construct from dimension of space and list of coordinates of cities
        /// </summary>
        /// <param name="dimension">Dimension of space</param>
        /// <param name="coords">List of coordinates of cities</param>
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

        /// <summary>
        /// Convert given traveling salesman problem to weighted graph by finding length of edges between all cities using <c>Distance method</c>.
        /// </summary>
        /// <returns>Weighted graph where vertices are the cities of the problem and edges are paths between them</returns>
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
        /// <summary>
        /// Construct from dimension of space and list of coordinates of cities
        /// </summary>
        /// <param name="dimension">Dimension of space</param>
        /// <param name="coords">List of coordinates of cities</param>
        public EuclidTravelingSalesmanProblem(int dimension, List<double[]> coords) : base(dimension, coords) { }

        /// <summary>
        /// Calculate distance between 2 coordinates in Euclidean space
        /// </summary>
        /// <param name="coord1">Source coordinate</param>
        /// <param name="coord2">Target coordinate</param>
        /// <returns></returns>
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