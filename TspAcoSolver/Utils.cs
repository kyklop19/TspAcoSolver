namespace TspAcoSolver
{
    public interface IRandom
    {
        public int Next(int minValue, int maxValue);
        public double NextDouble();

    }
    public class RandomGen : IRandom
    {
        Random rnd = new();
        public int Next(int minValue, int maxValue)
        {
            return rnd.Next(minValue, maxValue);
        }
        public double NextDouble()
        {
            return rnd.NextDouble();
        }

    }

    public class RandomFuncs
    {
        IRandom _rnd;
        public RandomFuncs(IRandom rnd)
        {
            _rnd = rnd;
        }
        /// <summary>
        /// Choose randomly index of item from the <c>arr</c> where the
        /// probability of choosing certain item is based on the value of the
        /// item. The probability is value of the item divided by sum of all
        /// values in <c>arr</c>.
        /// </summary>
        /// <param name="arr">Array with weights based on which the index is chosen.</param>
        /// <returns>Randomly chosen index of the <c>arr</c>.</returns>
        public int ChooseWeightBiased(double[] arr)
        {
            double sum = arr.Sum();
            for (int i = 0; i < arr.Length; i++)
            {
                arr[i] /= sum;
                if (i != 0)
                {
                    arr[i] += arr[i - 1];
                }
            }

            double rndNum = _rnd.NextDouble();
            int index = 0;
            while (index != arr.Length - 1 && rndNum >= arr[index]) index++;
            return index;
        }
    }
}