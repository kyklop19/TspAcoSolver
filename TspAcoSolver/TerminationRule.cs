using Microsoft.Extensions.Options;

namespace TspAcoSolver
{
    /// <summary>
    /// Flags that represent what termination rules should be checked when checking if solver should terminate.
    /// </summary>
    [Flags]
    public enum TerminationRule
    {
        /// <summary>
        /// Enable nothing
        /// </summary>
        None = 0, //TODO: remove
        /// <summary>
        /// Enable rule to check if iteration count has reached set limit.
        /// </summary>
        Fixed = 1,
        /// <summary>
        /// Enable rule to check if minimal solution in single iteration was within percentage of best-so-far solution enough times
        /// </summary>
        WithinPercentage = 2,
    }

    /// <summary>
    /// Interface for checker that checks if solver should terminate solution generation.
    /// </summary>
    public interface ITerminationChecker
    {
        /// <summary>
        /// Check if solver should terminate solution generation.
        /// </summary>
        /// <returns></returns>
        public bool Terminated();

        /// <summary>
        /// Reset the counter that counts how many times minimal solution in single iteration was within percentage of best-so-far solution
        /// </summary>
        public void ResetInRowWithinPercentageCount();

        /// <summary>
        /// Best-so-far solution that was found by the solver
        /// </summary>
        public ITour CurrBestTour { get; set; }

        /// <summary>
        /// Minimal solution found in single iteration by solver
        /// </summary>
        public ITour MinimumLengthSolInIter { get; set; }

        /// <summary>
        /// Counter that count number of iterations of solvers solution generation method
        /// </summary>
        public Counter CurrIterationCounter { get; set; }
    }

    /// <summary>
    /// Implementation of <c>ITerminationChecker</c> that checks enabled termination rules and terminates when at least one of them returns <c>true</c>.
    /// </summary>
    public class TerminationChecker : ITerminationChecker
    {
        TerminationParams _tParams;

        /// <summary>
        /// Best-so-far found tour
        /// </summary>
        public ITour CurrBestTour { get; set; }
        /// <summary>
        /// Minimal solution found in last iteration
        /// </summary>
        public ITour MinimumLengthSolInIter { get; set; }
        Counter _inRowWithinPercentageCounter = new();

        /// <summary>
        /// Counter that counts iteration in solution generation
        /// </summary>

        public Counter CurrIterationCounter { get; set; }

        /// <summary>
        /// Function that checks one termination rule
        /// </summary>
        /// <returns>True if according to the rule the solver should terminate</returns>
        delegate bool CheckFunc();

        /// <summary>
        /// Termination rule where <c>rule</c> acts as identifier and <c>func</c> checks the termination rule.
        /// </summary>
        /// <param name="rule">Enum identifier of the rule</param>
        /// <param name="func">Function that checks if according to the rule the solver should terminate</param>
        record Rule(TerminationRule rule, CheckFunc func);

        List<Rule> _rules = new();

        /// <summary>
        /// Construct from options with parameters that influence when to terminate
        /// </summary>
        /// <param name="tParamsOpt">Options with parameters that influence when to terminate</param>
        public TerminationChecker(IOptions<TerminationParams> tParamsOpt) : this(tParamsOpt.Value) { }

        /// <summary>
        /// Construct from parameters that influence when to terminate
        /// </summary>
        /// <param name="tParams">Parameters that influence when to terminate</param>
        public TerminationChecker(TerminationParams tParams)
        {
            _tParams = tParams;
            _rules.Add(new Rule(TerminationRule.Fixed, ReachedIterationCount));
            _rules.Add(new Rule(TerminationRule.WithinPercentage, ReachedInRowCountWithinPercentage));
        }

        /// <summary>
        /// Reset the counter that counts how many times minimal solution in single iteration was within percentage of best-so-far solution
        /// </summary>
        public void ResetInRowWithinPercentageCount()
        {
            _inRowWithinPercentageCounter.Reset();
        }

        /// <summary>
        /// Check if count of iterations, where minimal solution in single iteration was within percentage of best-so-far solution, has reached the limit
        /// </summary>
        /// <returns><c>true</c> if count of iterations, where minimal solution in single iteration was within percentage of best-so-far solution, has reached the limit else <c>false</c></returns>
        bool ReachedInRowCountWithinPercentage()
        {
            double ceilingLength = CurrBestTour.Length * (1 + (((double)_tParams.CeilingPercentage) / 100));

            if (MinimumLengthSolInIter.Length <= ceilingLength)
            {
                _inRowWithinPercentageCounter.Inc();
            }
            else
            {
                //TODO: think about removing
                // ResetInRowWithinPercentageCount();
            }
            return _inRowWithinPercentageCounter.Value >= _tParams.InRowTerminationCount;
        }

        /// <summary>
        /// Checks if the count of iterations of solver's solution generating function has reached the limit
        /// </summary>
        /// <returns><c>true</c> if the count of iterations of solver's solution generating function has reached the limit else <c>false</c></returns>
        bool ReachedIterationCount()
        {
            return CurrIterationCounter.Value >= _tParams.IterationCount;
        }

        /// <summary>
        /// For each rule check if according to it the solver should terminate. If at least one return <c>true</c> return also <c>true</c>.
        /// </summary>
        /// <returns><c>true</c> if at least one of the rules returns <c>true</c> else <c>false</c></returns>
        /// <exception cref="Exception">Thrown when none of termination rules are enabled.</exception>
        public bool Terminated()
        {
            bool terminated = false;
            if (_tParams.TerminationRule == TerminationRule.None)
            {
                throw new Exception("No termination rule chosen.");
            }
            foreach (Rule rule in _rules)
            {
                if (((TerminationRule)_tParams.TerminationRule).HasFlag(rule.rule))
                {
                    terminated = terminated || rule.func();
                    if (terminated) break;
                }
            }
            return terminated;
        }
    }
}