using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Options;

namespace TspAcoSolver
{
    /// <summary>
    /// Flags representing which reinitialization rules to check during reinitialization
    /// </summary>
    [Flags]
    public enum ReinitializationRule
    {
        /// <summary>
        /// No rules enabled
        /// </summary>
        None = 0,
        /// <summary>
        /// Enable fixed reinitialization rule
        /// </summary>
        Fixed = 1,
        /// <summary>
        /// Enable stagnation reinitialization rule
        /// </summary>
        Stagnation = 2,
    }

    /// <summary>
    /// Interface for classes that according to some rules reinitialize pheromones in given graph
    /// </summary>
    public interface IReinitializer
    {
        /// <summary>
        /// Counter that counts how many times in row there hasn't been found better solution in single iteration in solution generation
        /// </summary>
        public Counter StagnationCounter { get; set; }

        /// <summary>
        /// Counter that counts how many iterations have happened in solution generation
        /// </summary>
        public Counter CurrIterationCounter { get; set; }

        /// <summary>
        /// Reinitialize pheromones in <c>graph</c> if at least one reinitialization rule returns <c>true</c>
        /// </summary>
        /// <param name="graph">Pheromone graph where pheromones should be reinitialized</param>
        /// <returns><c>true</c> if at least one reinitialization rule returns <c>true</c></returns>
        public bool TryReinitialize(PheromoneGraph graph);
    }

    /// <summary>
    /// Class for reinitializing pheromones in given graph according to these rules:
    /// <list type="bullet">
    /// <item>fixed – reinitialize after fixed number of iterations</item>
    /// <item>stagnation – reinitialize after number of iterations with no improvement</item>
    /// </list>
    /// </summary>
    public class Reinitializer : IReinitializer
    {
        /// <summary>
        /// Parameters for which rules to apply and constants for these rules
        /// </summary>
        ReinitializationParams _rParams;
        /// <summary>
        /// Counter that counts how many times in row there hasn't been found better solution in single iteration in solution generation
        /// </summary>
        public Counter StagnationCounter { get; set; }

        /// <summary>
        /// Counter that counts how many iterations have happened in solution generation
        /// </summary>
        public Counter CurrIterationCounter { get; set; }

        /// <summary>
        /// Function that checks given rule
        /// </summary>
        /// <returns><c>true</c> if according to the rule solver should reinitialize</returns>
        delegate bool CheckFunc();
        /// <summary>
        /// Record representing reinitialization rule
        /// </summary>
        /// <param name="rule">Rule identifier</param>
        /// <param name="func">Function that checks given rule</param>
        record Rule(ReinitializationRule rule, CheckFunc func);

        List<Rule> _rules = new();

        /// <summary>
        /// Construct from options with parameter that influence when to reinitialize
        /// </summary>
        /// <param name="rParamsOpt">Options with parameter that influence when to reinitialize</param>
        public Reinitializer(IOptions<ReinitializationParams> rParamsOpt) : this(rParamsOpt.Value) { }

        /// <summary>
        /// Construct from parameter that influence when to reinitialize
        /// </summary>
        /// <param name="rParams">Parameter that influence when to reinitialize</param>
        public Reinitializer(ReinitializationParams rParams)
        {
            _rParams = rParams;
            _rules.Add(new Rule(ReinitializationRule.Fixed, ReachedFixedIterIncrement));
            _rules.Add(new Rule(ReinitializationRule.Stagnation, ReachedStagnationCeiling));
        }

        /// <summary>
        /// Check if count of iterations in a row with no improvement has reached its limit
        /// </summary>
        /// <returns><c>true</c> if count of iterations in a row with no improvement has reached its limit else <c>false</c></returns>
        bool ReachedStagnationCeiling()
        {
            return StagnationCounter.Value >= _rParams.StagnationCeiling;
        }

        /// <summary>
        /// Check if count of iterations without reinitialization has reached its limit
        /// </summary>
        /// <returns><c>true</c> if count of iterations without reinitialization has reached its limit else <c>false</c></returns>
        bool ReachedFixedIterIncrement()
        {
            return (CurrIterationCounter.Value % _rParams.IterIncrement) == 0;
        }

        /// <summary>
        /// Reinitialize pheromones in <c>graph</c> if at least one of the enabled rules has returned <c>true</c>
        /// </summary>
        /// <param name="graph">Pheromone graph to be reinitialized</param>
        /// <returns><c>true</c> if at least one of the enabled rules has returned <c>true</c> else <c>false</c></returns>
        public bool TryReinitialize(PheromoneGraph graph)
        {
            bool reinitialized = false;
            if (_rParams.ReinitializationRule != ReinitializationRule.None)
            {
                foreach (Rule rule in _rules)
                {
                    if (((ReinitializationRule)_rParams.ReinitializationRule).HasFlag(rule.rule))
                    {
                        reinitialized = reinitialized || rule.func();
                        if (reinitialized)
                        {
                            graph.Reinitialize();
                            StagnationCounter.Reset();
                            break;
                        }
                    }
                }
            }
            return reinitialized;
        }

    }
}