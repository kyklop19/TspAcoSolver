using System.Reflection;
using YamlDotNet.Core;
using YamlDotNet.Core.Tokens;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace TspAcoSolver
{
    /// <summary>
    /// Converter used by YAML deserializer to convert string value to enum with <c>[Flags]</c> attribute
    /// </summary>
    public class FlagsConverter : IYamlTypeConverter
    {
        /// <summary>
        /// Checks if <c>type</c> is enum with <c>[Flags]</c> attribute
        /// </summary>
        /// <param name="type">Type to be checked if it corresponds with the type of the converter</param>
        /// <returns><c>true</c> if <c>type</c> is enum with <c>[Flags]</c> attribute</returns>
        public bool Accepts(Type type)
        {
            type = Nullable.GetUnderlyingType(type) ?? type;
            return type.IsEnum && type.GetCustomAttribute<FlagsAttribute>() != null;
        }

        /// <summary>
        /// Read next token and try to convert <c>Scalar</c> to the <c>type</c> (enum with <c>[Flags]</c> attribute)
        /// </summary>
        /// <param name="parser">The parser that is retuning the token</param>
        /// <param name="type">Type to which the token should be converted to</param>
        /// <param name="deserializer">Deserializer being used for reading YAML file</param>
        /// <returns>Value converted from the next <c>Scalar</c> of the type <c>type</c></returns>
        /// <exception cref="YamlException">Thrown when token is not <c>Scalar</c></exception>
        public object ReadYaml(YamlDotNet.Core.IParser parser, Type type, ObjectDeserializer deserializer)
        {
            type = Nullable.GetUnderlyingType(type) ?? type;

            if (parser.TryConsume<YamlDotNet.Core.Events.Scalar>(out YamlDotNet.Core.Events.Scalar scalar))
            {

                string[] values = scalar.Value
                                        .Split("|", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                                        .Select(str => str.Replace("_", ""))
                                        .ToArray();

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
        /// <summary>
        /// Convert <c>value</c> of type <c>type</c> (enum with <c>[Flags]</c> attribute) to <c>Scalar</c> and emit it.
        /// </summary>
        /// <param name="emitter">Emitter that emits the resulting string to the resulting serialization.</param>
        /// <param name="value">Value of type <c>type</c> (enum with <c>[Flags]</c> attribute) that should be converted.</param>
        /// <param name="type">Type (enum with <c>[Flags]</c> attribute) of value</param>
        /// <param name="serializer">Serializer that is used for serializing the value</param>
        public void WriteYaml(IEmitter emitter, object? value, Type type, ObjectSerializer serializer)
        {
            emitter.Emit(new YamlDotNet.Core.Events.Scalar(value.ToString()));
        }
    }

    /// <summary>
    /// Class for reading from YAML configuration file
    /// </summary>
    public class Config
    {
        /// <summary>
        /// Read from YAML file that has path <c>path</c> and try to deserialize it store the parameters in <c>SolvingParams</c> instance
        /// </summary>
        /// <param name="path">Path of the YAML file that should be read</param>
        /// <returns>Instance of <c>SolvingParams</c> in which the parameters of the YAML file should be stored</returns>
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

    /// <summary>
    /// Abstract class representing storage for parameters where each property is parameter
    /// </summary>
    public abstract class Params
    {
        /// <summary>
        /// For each property of <c>otherParams</c> if its value isn't
        /// <c>null</c> replace value of the same property of this instance with
        /// the value from <c>otherParams</c>. If the value is null do nothing.
        /// <br/>
        /// <c>otherParams</c> must be the same type as this instance.
        /// </summary>
        /// <param name="otherParams">Instance with parameters with the same
        /// type as this instance</param>
        /// <exception cref="Exception">Thrown when <c>otherParams</c> is not of
        /// the same type as this instance</exception>
        public void Overwrite(Params otherParams)
        {
            if (this.GetType() != otherParams.GetType())
            {
                throw new Exception();
            }

            PropertyInfo[] properties = otherParams
                                        .GetType()
                                        .GetProperties()
                                        .Where(p => p.GetValue(otherParams) != null)
                                        .ToArray();
            foreach (PropertyInfo property in properties)
            {
                if (property.PropertyType.IsSubclassOf(typeof(Params)))
                {
                    ((Params)(property.GetValue(this))).Overwrite((Params)(property.GetValue(otherParams)));
                }
                else
                {
                    property.SetValue(this, property.GetValue(otherParams));
                }
            }
        }
    }

    /// <summary>
    /// Class for storing parameters that influence <c>PheromoneGraph</c>/updating of pheromones
    /// </summary>
    public class PheromoneParams : Params
    {
        /// <summary>
        /// Parameter influencing what amount of pheromones is evaporated/deleted
        /// </summary>
        public double? EvaporationCoef { get; set; }
        /// <summary>
        /// Parameter influencing what amount of pheromones is decayed/deleted
        /// </summary>
        public double? DecayCoef { get; set; }
        /// <summary>
        /// Parameter influencing if <c>InitialPheromoneAmount</c> should be calculated
        /// </summary>
        public bool? CalculateInitialPheromoneAmount { get; set; }
        /// <summary>
        /// Parameter influencing what amount of pheromones should be set for all edges at initialization of graph
        /// </summary>
        public double? InitialPheromoneAmount { get; set; }
        /// <summary>
        /// Parameter influencing what amount of pheromones should be deposited on edges at update of graph
        /// </summary>
        public double? PheromoneAmount { get; set; }

        /// <summary>
        /// Convert to string.
        /// </summary>
        /// <returns>String with values of all parameters</returns>
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
    /// <summary>
    /// Class for storing parameters that influence how solutions for traveling salesman problem are found
    /// </summary>
    public class ColonyParams : Params
    {
        /// <summary>
        /// Parameter influencing how many ants are used for finding solutions in colony
        /// </summary>
        public int? AntCount { get; set; }
        /// <summary>
        /// Parameter influencing how many threads at most can colony for finding solutions use
        /// </summary>
        public int? ThreadCount { get; set; }
        /// <summary>
        /// Parameter influencing how much pheromones influence choice of next neighboring vertex
        /// </summary>
        public double? TrailLevelFactor { get; set; }
        /// <summary>
        /// Parameter influencing how much length of edge influences choice of next neighboring vertex
        /// </summary>
        public double? AttractivenessFactor { get; set; }
        /// <summary>
        /// Parameter influencing the probability of choosing next neighboring vertex in exploitation/exploration manner
        /// <br/>
        /// Used in Ant Colony System.
        /// </summary>
        public double? ExploProportionConst { get; set; }

        /// <summary>
        /// Convert to string.
        /// </summary>
        /// <returns>String with values of all parameters</returns>
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
    /// <summary>
    /// Class for storing parameters that influence when solver should terminate solution finding
    /// </summary>
    public class TerminationParams : Params
    {
        /// <summary>
        /// Parameter influencing what termination rules are enabled
        /// </summary>
        public TerminationRule? TerminationRule { get; set; }
        /// <summary>
        /// Parameter influencing how many iterations until termination
        /// </summary>
        public int? IterationCount { get; set; }
        /// <summary>
        /// Parameter influencing what is the limit for minimal solution in single iteration to be within percentage of best-so-far solution
        /// </summary>
        public double? CeilingPercentage { get; set; }
        /// <summary>
        /// Parameter influencing how many minimal solutions in single iteration that are within percentage of best-so-far solution are needed for termination
        /// </summary>
        public int? InRowTerminationCount { get; set; }

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
    /// <summary>
    /// Class for storing parameters that influence <c>IReinitializer</c>/reinitialization
    /// </summary>
    public class ReinitializationParams : Params
    {
        /// <summary>
        /// Parameter influencing what reinitialization rules should be enabled
        /// </summary>
        public ReinitializationRule? ReinitializationRule { get; set; }
        /// <summary>
        /// Parameter influencing how many iterations at most are between reinitializations
        /// </summary>
        public int? IterIncrement { get; set; }
        /// <summary>
        /// Parameter influencing how many iterations with stagnation in row at most are permitted until reinitialization
        /// </summary>
        public int? StagnationCeiling { get; set; }
        /// <summary>
        /// Convert to string.
        /// </summary>
        /// <returns>String with values of all parameters</returns>
        public override string ToString()
        {
            return $"""
                    ReinitializationRule: {ReinitializationRule}
                    IterIncrement: {IterIncrement}
                    StagnationCeiling: {StagnationCeiling}
                    """;
        }
    }

    /// <summary>
    /// Class for storing parameters that influence whole <c>ISolver</c>/process of finding solutions
    /// </summary>
    public class SolvingParams : Params
    {
        /// <summary>
        /// Parameter influencing what algorithm to use for solving the traveling salesman problem
        /// </summary>
        public string? Algorithm { get; set; }
        /// <summary>
        /// Parameters influencing pheromone updates
        /// </summary>
        public PheromoneParams PheromoneParams { get; set; }
        /// <summary>
        /// Parameters influencing when to terminate
        /// </summary>
        public TerminationParams TerminationParams { get; set; }
        /// <summary>
        /// Parameters influencing when to reinitialize
        /// </summary>
        public ReinitializationParams ReinitializationParams { get; set; }
        /// <summary>
        /// Parameters influencing how solutions are found
        /// </summary>
        public ColonyParams ColonyParams { get; set; }

        /// <summary>
        /// Convert to string.
        /// </summary>
        /// <returns>String with values of all parameters</returns>
        public override string ToString()
        {
            return $"""
                    Algorithm: {Algorithm}
                    PheromoneParams:
                        {PheromoneParams.ToString().Replace("\n", "\n    ")}
                    TerminationParams:
                        {TerminationParams.ToString().Replace("\n", "\n    ")}
                    ReinitializationParams:
                        {ReinitializationParams.ToString().Replace("\n", "\n    ")}
                    ColonyParams:
                        {ColonyParams.ToString().Replace("\n", "\n    ")}
                    """;
        }
    }
}