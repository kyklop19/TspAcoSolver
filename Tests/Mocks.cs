using TspAcoSolver;

namespace Tests
{
    public class MockRandom : IRandom
    {
        int[] _mockedInts;
        int _intIndex = 0;
        double[] _mockedDoubles;
        int _doubleIndex = 0;
        public MockRandom(int[] mockedInts) : this(mockedInts, new double[0]) { }
        public MockRandom(double[] mockedDoubles) : this(new int[0], mockedDoubles) { }

        public MockRandom(int[] mockedInts, double[] mockedDoubles)
        {
            _mockedInts = mockedInts;
            _mockedDoubles = mockedDoubles;
        }
        public int Next(int minValue, int maxValue)
        {
            return _mockedInts[_intIndex++];
        }
        public double NextDouble()
        {
            return _mockedDoubles[_doubleIndex++];
        }
    }

    public class MockColony : IColony
    {
        List<ITour> _tours;
        public MockColony(List<ITour> tours)
        {
            _tours = tours;
        }
        public List<ITour> GenerateSolutions(PheromoneGraph graph)
        {
            return _tours;
        }
    }

    public class MockTour : ITour
    {
        double _length;
        public MockTour(double length)
        {
            _length = length;
            Vertices = new();
        }

        public double Length { get => _length; }

        public void Add(int vertex) { }

        public List<int> Vertices { get; init; }
    }

    public class MockProblem : IProblem
    {
        public int CityCount { get; }

        public WeightedGraph ToGraph()
        {
            return new WeightedGraph(new double[,] { { } });
        }
    }
}