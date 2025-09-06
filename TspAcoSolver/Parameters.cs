using System.Reflection;
using YamlDotNet.Core;
using YamlDotNet.Core.Tokens;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace TspAcoSolver
{
    public class FlagsConverter : IYamlTypeConverter
    {
        public bool Accepts(Type type)
        {
            return type.IsEnum && type.GetCustomAttribute<FlagsAttribute>() != null;
        }

        public object ReadYaml(YamlDotNet.Core.IParser parser, Type type, ObjectDeserializer deserializer)
        {
            if (parser.TryConsume<YamlDotNet.Core.Events.Scalar>(out YamlDotNet.Core.Events.Scalar scalar))
            {

                string[] values = scalar.Value.Split("|", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

                object result = Enum.ToObject(type, 0);
                foreach (string value in values)
                {
                    if (Enum.TryParse(type, value, true, out var enumValue))
                    {
                        result = (dynamic)result | (dynamic)enumValue;
                    }
                }
                return result;
            }
            else
            {
                throw new YamlException("Invalid flag format");
            }

        }

        public void WriteYaml(IEmitter emitter, object? value, Type type, ObjectSerializer serializer)
        {
            emitter.Emit(new YamlDotNet.Core.Events.Scalar(value.ToString()));
        }
    }
    public class Config
    {
        public SolvingParams Read(string path)
        {
            using System.IO.StreamReader r = new(path);
            IDeserializer deserializer = new DeserializerBuilder()
                .WithNamingConvention(UnderscoredNamingConvention.Instance)  // see height_in_inches in sample yml
                .WithTypeConverter(new FlagsConverter())
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

        public override string ToString()
        {
            return $"""
                    EvaporationCoef: {EvaporationCoef}
                    DecayCoef: {DecayCoef}
                    CalculateInitialPheromoneAmount: {CalculateInitialPheromoneAmount}
                    InitialPheromoneAmount: {InitialPheromoneAmount}
                    PheromoneAmount: {PheromoneAmount}
                    """;
        }
    }
    public class ColonyParams
    {
        public int AntCount { get; set; }
        public int ThreadCount { get; set; }
        public double TrailLevelFactor { get; set; }
        public double AttractivenessFactor { get; set; }
        public double ExploProportionConst { get; set; }

        public override string ToString()
        {
            return $"""
                    AntCount: {AntCount}
                    ThreadCount: {ThreadCount}
                    TrailLevelFactor: {TrailLevelFactor}
                    AttractivenessFactor: {AttractivenessFactor}
                    ExploProportionConst: {ExploProportionConst}
                    """;
        }
    }
    public class TerminationParams
    {
        public TerminationRule TerminationRule { get; set; }
        public int IterationCount { get; set; }
        public double CeilingPercentage { get; set; }
        public int InRowTerminationCount { get; set; }

        public override string ToString()
        {
            return $"""
                    TerminationRule: {TerminationRule}
                    IterationCount: {IterationCount}
                    CeilingPercentage: {CeilingPercentage}
                    InRowTerminationCount: {InRowTerminationCount}
                    """;
        }
    }

    public class SolvingParams
    {
        public string Algorithm { get; set; }
        public PheromoneParams PheromoneParams { get; set; }
        public TerminationParams TerminationParams { get; set; }
        public ColonyParams ColonyParams { get; set; }

        public override string ToString()
        {
            return $"""
                    Algorithm: {Algorithm}
                    PheromoneParams:
                        {PheromoneParams.ToString().Replace("\n", "\n    ")}
                    TerminationParams:
                        {TerminationParams.ToString().Replace("\n", "\n    ")}
                    ColonyParams:
                        {ColonyParams.ToString().Replace("\n", "\n    ")}
                    """;
        }
    }
}