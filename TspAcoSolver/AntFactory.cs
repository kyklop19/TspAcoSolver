using Microsoft.Extensions.DependencyInjection;

namespace TspAcoSolver
{
    /// <summary>
    /// Interface for factory that creates certain number of instances of ants.
    /// </summary>
    /// <typeparam name="T">Ant class that should be instantiated</typeparam>
    public interface IAntFactory<out T> where T : IAnt
    {
        /// <summary>
        /// Create array of length <c>count</c> with instances of ant <c>T</c>
        /// </summary>
        /// <param name="count">Number of ants in array</param>
        /// <returns>Array of length <c>count</c> with instances of ant <c>T</c></returns>
        public T[] CreateAnts(int count);
    }

    /// <summary>
    /// Factory class for creating array of instances of ant <c>T</c>
    /// </summary>
    /// <typeparam name="T">Type of ant to instantiate</typeparam>
    /// <param name="_serviceProvider">Service provider that provides services for instantiated ants</param>
    public class AntFactory<T>(IServiceProvider _serviceProvider) : IAntFactory<T> where T : IAnt
    {
        /// <summary>
        /// Create array of length <c>count</c> with instances of ant <c>T</c>
        /// where services are provided for ants by <c>_serviceProvider</c>
        /// </summary>
        /// <param name="count">Number of ants in array</param>
        /// <returns>
        /// Array of length <c>count</c> with instances of ant <c>T</c>
        /// </returns>
        public T[] CreateAnts(int count)
        {
            T[] ants = new T[count];
            for (int i = 0; i < count; i++)
            {

                ants[i] = ActivatorUtilities.CreateInstance<T>(_serviceProvider);
                // new (colonyParams, (IRandom)new RandomGen());
            }
            return ants;
        }
    }
    // public class AcsAntFactory(IServiceProvider _serviceProvider) : IAntFactory
    // {
    //     public IAnt[] CreateAnts(int count)
    //     {
    //         AcsAnt[] ants = new AcsAnt[count];
    //         for (int i = 0; i < count; i++)
    //         {
    //             ants[i] = ActivatorUtilities.CreateInstance<AcsAnt>(_serviceProvider);
    //             // new AcsAnt(colonyParams, (IRandom)new RandomGen());
    //         }
    //         return ants;
    //     }
    // }
}