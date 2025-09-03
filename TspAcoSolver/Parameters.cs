using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace TspAcoSolver
{
    public class Config
    {
        public SolvingParams Read(string path)
        {
            using System.IO.StreamReader r = new(@"default_config.yaml");
            IDeserializer deserializer = new DeserializerBuilder()
                .WithNamingConvention(UnderscoredNamingConvention.Instance)  // see height_in_inches in sample yml
                .Build();

            SolvingParams p = deserializer.Deserialize<SolvingParams>(r.ReadToEnd());
            return p;
        }
    }

    public class PheromoneParams
    {
        public double EvaporationCoef { get; set; }
        public double DecayCoef { get; set; }
        public bool CalculateInitialPheromoneAmount { get; set; }
        public double InitialPheromoneAmount { get; set; }
        public double PheromoneAmount { get; set; }
    }
    public class ColonyParams
    {
        public int AntCount { get; set; }
        public int ThreadCount { get; set; }
        public double TrailLevelFactor { get; set; }
        public double AttractivenessFactor { get; set; }
        public double ExploProportionConst { get; set; }
    }
    public class TerminationParams
    {
        public string TerminationRule { get; set; }
        public int IterationCount { get; set; }
        public double CeilingPercentage { get; set; }
        public int InRowTerminationCount { get; set; }
    }

    public class SolvingParams
    {
        public string Algorithm { get; set; }
        public PheromoneParams PheromoneParams { get; set; }
        public TerminationParams TerminationParams { get; set; }
        public ColonyParams ColonyParams { get; set; }
    }
}