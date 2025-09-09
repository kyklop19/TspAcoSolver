using Microsoft.Extensions.Options;

namespace TspAcoSolver
{
    [Flags]
    public enum TerminationRule
    {
        None = 0, //TODO: remove
        Fixed = 1,
        WithinPercentage = 2,
    }
    public interface ITerminationChecker
    {
        public bool Terminated();
        public void ResetInRowWithinPercentageCount();
        public ITour CurrBestTour { get; set; }
        public ITour MinimumLengthSolInIter { get; set; }

        public Counter CurrIterationCounter { get; set; }
    }

    public class TerminationChecker : ITerminationChecker
    {
        TerminationParams _tParams;
        public ITour CurrBestTour { get; set; }
        public ITour MinimumLengthSolInIter { get; set; }
        Counter _inRowWithinPercentageCounter = new();

        public Counter CurrIterationCounter { get; set; }

        delegate bool CheckFunc();
        record Rule(TerminationRule rule, CheckFunc func);

        List<Rule> _rules = new();

        public TerminationChecker(IOptions<TerminationParams> tParamsOpt)
        {
            _tParams = tParamsOpt.Value;
            _rules.Add(new Rule(TerminationRule.Fixed, ReachedIterationCount));
            _rules.Add(new Rule(TerminationRule.WithinPercentage, ReachedInRowCountWithinPercentage));
        }

        public void ResetInRowWithinPercentageCount()
        {
            _inRowWithinPercentageCounter.Reset();
        }

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

        bool ReachedIterationCount()
        {
            return CurrIterationCounter.Value == _tParams.IterationCount;
        }

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