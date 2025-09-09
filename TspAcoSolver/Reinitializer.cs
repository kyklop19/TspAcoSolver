using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Options;

namespace TspAcoSolver
{
    [Flags]
    public enum ReinitializationRule
    {
        None = 0,
        Fixed = 1,
        Stagnation = 2,
    }
    public interface IReinitializer
    {
        public Counter StagnationCounter { get; set; }
        public Counter CurrIterationCounter { get; set; }
        public bool TryReinitialize(PheromoneGraph graph);
    }
    public class Reinitializer : IReinitializer
    {
        ReinitializationParams _rParams;
        public Counter StagnationCounter { get; set; }
        public Counter CurrIterationCounter { get; set; }

        delegate bool CheckFunc();
        record Rule(ReinitializationRule rule, CheckFunc func);

        List<Rule> _rules = new();

        public Reinitializer(IOptions<ReinitializationParams> rParamsOpt) : this(rParamsOpt.Value) {}
        public Reinitializer(ReinitializationParams rParams)
        {
            _rParams = rParams;
            _rules.Add(new Rule(ReinitializationRule.Fixed, ReachedFixedIterIncrement));
            _rules.Add(new Rule(ReinitializationRule.Stagnation, ReachedStagnationCeiling));
        }

        bool ReachedStagnationCeiling()
        {
            return StagnationCounter.Value >= _rParams.StagnationCeiling;
        }
        bool ReachedFixedIterIncrement()
        {
            return (CurrIterationCounter.Value % _rParams.IterIncrement) == 0;
        }
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