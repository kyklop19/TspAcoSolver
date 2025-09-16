namespace TspAcoSolver
{
    /// <summary>
    /// Interface of a random number generator
    /// </summary>
    public interface IRandom
    {
        /// <summary>
        /// Generate random integer between <c>minValue</c> (included) and <c>maxValue</c> (excluded).
        /// </summary>
        /// <param name="minValue">Integer that is equal or less than any generated integer</param>
        /// <param name="maxValue">Integer that is greater than any generated integer</param>
        /// <returns>Random integer between <c>minValue</c> (included) and <c>maxValue</c> (excluded)</returns>
        public int Next(int minValue, int maxValue);
        /// <summary>
        /// Generate random double that is greater or equal to 0 and less than 1.
        /// </summary>
        /// <returns>Random double that is greater or equal to 0 and less than 1</returns>
        public double NextDouble();

    }

    /// <summary>
    /// Wrapper class around the build-in <c>System.Random</c> random generator that implements the <c>IRandom</c> interface.
    /// </summary>
    public class RandomGen : IRandom
    {
        Random rnd = new();

        /// <summary>
        /// Generate random integer between <c>minValue</c> (included) and <c>maxValue</c> (excluded).
        ///
        /// Wrapper method around the <c>System.Random.Next()</c> method.
        /// </summary>
        /// <param name="minValue">Integer that is equal or less than any generated integer</param>
        /// <param name="maxValue">Integer that is greater than any generated integer</param>
        /// <returns>Random integer between <c>minValue</c> (included) and <c>maxValue</c> (excluded)</returns>
        public int Next(int minValue, int maxValue)
        {
            return rnd.Next(minValue, maxValue);
        }

        /// <summary>
        /// Generate random double that is greater or equal to 0 and less than 1.
        ///
        /// Wrapper method around the <c>System.Random.NextDouble()</c> method.
        /// </summary>
        /// <returns>Random double that is greater or equal to 0 and less than 1</returns>
        public double NextDouble()
        {
            return rnd.NextDouble();
        }

    }

    /// <summary>
    /// Class containing utility functions that use random number generator.
    /// </summary>
    public class RandomFuncs
    {
        IRandom _rnd;

        /// <summary>
        /// Constructs the class with <c>rnd</c> random number generator.
        /// </summary>
        /// <param name="rnd">Random number generator</param>
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

    /// <summary>
    /// Class for keeping count of something
    /// </summary>
    public class Counter
    {
        int _value = 0;

        /// <summary>
        /// Get the current count of the counter.
        /// </summary>
        public int Value { get => _value; }

        /// <summary>
        /// Reset the count back to 0.
        /// </summary>
        public void Reset()
        {
            _value = 0;
        }

        /// <summary>
        /// Increase the count by 1.
        /// </summary>
        public void Inc() => _value++;

        /// <summary>
        /// Decrease the count by 1.
        /// </summary>
        public void Dec() => _value--;

        /// <summary>
        /// Return the count in formatted string.
        /// </summary>
        /// <returns>String in form: "Count: <count>"</returns>
        public override string ToString() => $"Count: {Value}";
    }
}