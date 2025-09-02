using TspAcoSolver;

namespace Tests
{
    public class MockRandom : IRandom
    {
        int[] _mockedInts;
        int _intIndex = 0;
        double[] _mockedDoubles;
        int _doubleIndex = 0;
        public MockRandom(int[] mockedInts) : this(mockedInts, new double[0]) {}
        public MockRandom(double[] mockedDoubles) : this(new int[0], mockedDoubles){}

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
}