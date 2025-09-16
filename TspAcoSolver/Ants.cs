using Microsoft.Extensions.Options;

namespace TspAcoSolver
{
    /// <summary>
    /// Interface for ant class that is intended to find a tour in pheromone graph
    /// </summary>
    public interface IAnt
    {
        /// <summary>
        /// Last <c>ITour</c> that was found using <c>FindTour</c>
        /// </summary>
        public ITour LastTour { get; set; }

        /// <summary>
        /// Find <c>ITour</c> using all vertices of <c>graph</c> and store it in <c>LastTour</c>
        /// </summary>
        /// <param name="graph">Graph in which to find <c>ITour</c></param>
        public void FindTour(PheromoneGraph graph);
    }

    /// <summary>
    /// <c>AntBase</c> is abstract class intended for finding tour in given pheromone graph.
    /// </summary>
    public abstract class AntBase : IAnt
    {
        /// <summary>
        /// Vertex at which the ant is currently positioned
        /// </summary>
        public int CurrVertex { get; set; }
        /// <summary>
        /// Last <c>ITour</c> that was found using <c>FindTour</c>
        /// </summary>
        public ITour LastTour { get; set; }

        protected IRandom rnd;

        /// <summary>
        /// Construct using random number generator <c>rnd</c>.
        /// </summary>
        /// <param name="rnd">Random number generator</param>
        public AntBase(IRandom rnd)
        {
            this.rnd = rnd;
        }

        /// <summary>
        /// Travel the <c>graph</c> and try to find near optimal tour
        /// </summary>
        /// <param name="graph">Pheromone graph in which the ant tries to find the tour</param>
        public void FindTour(PheromoneGraph graph)
        {
            LastTour = new Tour(graph);

            CurrVertex = rnd.Next(0, graph.VertexCount);
            LastTour.Add(CurrVertex);

            while (!LastTour.HasAllVertices() && !LastTour.HasDeadEnd())
            {
                CurrVertex = ChooseNbr(graph, LastTour.NextPossibleVertices());
                LastTour.Add(CurrVertex);
            }
        }
        /// <summary>
        /// Choose next neighbor of <c>CurrVertex</c> from <c>unvisited_nbrs</c> in <c>graph</c>.
        /// </summary>
        /// <param name="graph">Graph that contains the neighbor</param>
        /// <param name="unvisited_nbrs">Unvisited neighbors from which to choose the resulting vertex</param>
        /// <returns>Vertex that is the neighbor of <c>CurrVertex</c></returns>
        protected abstract int ChooseNbr(PheromoneGraph graph, List<int> unvisited_nbrs);
    }

    /// <summary>
    /// <c>NearestNbrAnt</c> is class intended for finding tour in given
    /// pheromone graph by always choosing the closest vertex.
    /// </summary>
    public class NearestNbrAnt : AntBase
    {
        /// <summary>
        /// Construct with random number generator
        /// </summary>
        /// <param name="rnd">Random number generator</param>
        public NearestNbrAnt(IRandom rnd) : base(rnd) { }

        /// <summary>
        /// Choose the nearest neighbor of <c>CurrVertex</c> from <c>unvisited_nbrs</c> in <c>graph</c>.
        /// </summary>
        /// <param name="graph">Graph that contains the neighbor</param>
        /// <param name="unvisited_nbrs">Unvisited neighbors from which to choose the resulting vertex</param>
        /// <returns>Vertex that is the nearest neighbor of <c>CurrVertex</c></returns>
        protected override int ChooseNbr(PheromoneGraph graph, List<int> unvisitedNbrs)
        {
            double nearestNbrDist = graph.Weight(CurrVertex, unvisitedNbrs[0]);
            int nearestNbrIndex = 0;
            for (int i = 1; i < unvisitedNbrs.Count; i++)
            {
                double dist = graph.Weight(CurrVertex, unvisitedNbrs[i]);
                if (dist < nearestNbrDist)
                {
                    nearestNbrDist = dist;
                    nearestNbrIndex = i;
                }
            }
            return unvisitedNbrs[nearestNbrIndex];
        }
    }

    /// <summary>
    /// <c>RandomAntBase</c> is abstract class intended for finding tour in
    /// given pheromone graph. Additionally it's equipped with method for
    /// choosing random neighbor biased by length of edge and its pheromone amount.
    /// </summary>
    public abstract class RandomAntBase : AntBase
    {
        /// <summary>
        /// Constant for how much pheromones influence edge scoring
        /// </summary>
        public double TrailLevelFactor { get; init; }
        /// <summary>
        /// Constant for how much length of edge influences edge scoring
        /// </summary>/
        public double AttractivenessFactor { get; init; }

        /// <summary>
        /// Constant that influences how probable is exploitation version of choosing next neighbor
        /// </summary>
        public double ExploProportionConst { get; init; }

        /// <summary>
        /// Construct with parameters and random number generator
        /// </summary>
        /// <param name="colonyParams">Parameters that influence generation of solutions</param>
        /// <param name="rnd">Random number generator</param>
        public RandomAntBase(IOptions<ColonyParams> colonyParams, IRandom rnd) : base(rnd)
        {
            TrailLevelFactor = (double)colonyParams.Value.TrailLevelFactor;
            AttractivenessFactor = (double)colonyParams.Value.AttractivenessFactor;
            ExploProportionConst = (double)colonyParams.Value.ExploProportionConst;
        }

        /// <summary>
        /// Calculate score of edge from <c>CurrVertex</c> to <c>nbr</c>
        /// where score has higher value with shorter edges and higher amount
        /// of pheromones.
        /// </summary>
        /// <param name="graph">Pheromone graph in which to calculate score of its edge</param>
        /// <param name="nbr">Vertex to which goes from <c>CurrVertex</c> the edge</param>
        /// <returns>Score of the edge based on its length and pheromone amount</returns>
        protected double ScoreEdge(PheromoneGraph graph, int nbr)
        {
            double attractiveness = 1 / graph.Weight(CurrVertex, nbr);
            double trailLevel = graph.Pheromones[CurrVertex, nbr];
            double score = Math.Pow(trailLevel, TrailLevelFactor) * Math.Pow(attractiveness, AttractivenessFactor);
            return score;
        }

        /// <summary>
        /// Randomly choose one of the <c>unvisited_nbrs</c> from <c>graph</c>
        /// biased towards neighbors with higher score that was calculated using
        /// the <c>ScoreEdge</c> method
        /// </summary>
        /// <param name="graph">Graph in which the neighbors are located</param>
        /// <param name="unvisited_nbrs">Neighboring vertices from which to
        /// choose the resulting vertex</param>
        /// <returns>
        /// One randomly chosen vertex from <c>unvisited_nbrs</c> biased towards
        /// vertices with higher score
        /// </returns>
        protected int ChooseRandomNbr(PheromoneGraph graph, List<int> unvisited_nbrs)
        {
            double[] scores = new double[unvisited_nbrs.Count];
            for (int i = 0; i < unvisited_nbrs.Count; i++)
            {
                double score = ScoreEdge(graph, unvisited_nbrs[i]);
                scores[i] = score;
            }

            int index = new RandomFuncs(rnd).ChooseWeightBiased(scores);
            return unvisited_nbrs[index];
        }

    }

    /// <summary>
    /// Class intended for finding tour in given pheromone graph using Ant System algorithm.
    /// </summary>
    public class AsAnt : RandomAntBase
    {
        /// <summary>
        /// Construct from options with parameters influencing how tour is found
        /// and random number generator
        /// </summary>
        /// <param name="colonyParamsOpt">Options with parameters influencing how tour is found</param>
        /// <param name="rnd">Random number generator</param>
        public AsAnt(IOptions<ColonyParams> colonyParamsOpt, IRandom rnd)
            : base(colonyParamsOpt, rnd) { }

        /// <summary>
        /// Choose next neighboring vertex from <c>unvisitedNbrs</c> in <c>graph</c> randomly biased towards vertices whose edges have higher scores.
        /// </summary>
        /// <param name="graph">Graph in which to find neighbor</param>
        /// <param name="unvisitedNbrs">Neighboring vertices from which to choose</param>
        /// <returns>Next neighboring vertex from <c>unvisitedNbrs</c> in <c>graph</c></returns>
        protected override int ChooseNbr(PheromoneGraph graph, List<int> unvisitedNbrs)
        {
            return ChooseRandomNbr(graph, unvisitedNbrs);
        }
    }

    /// <summary>
    /// Class intended for finding tour in given pheromone graph using Ant Colony System algorithm.
    /// </summary>
    public class AcsAnt : RandomAntBase
    {
        /// <summary>
        /// Construct from options with parameters influencing how tour is found
        /// and random number generator
        /// </summary>
        /// <param name="colonyParamsOpt">Options with parameters influencing how tour is found</param>
        /// <param name="rnd">Random number generator</param>
        public AcsAnt(IOptions<ColonyParams> colonyParamsOpt, IRandom rnd)
            : base(colonyParamsOpt, rnd) { }

        /// <summary>
        /// Choose next neighboring vertex from <c>unvisitedNbrs</c> in <c>graph</c> that has the highest score.
        /// </summary>
        /// <param name="graph">Graph in which to find neighbor</param>
        /// <param name="unvisitedNbrs">Neighboring vertices from which to choose</param>
        /// <returns>Next neighboring vertex from <c>unvisitedNbrs</c> in <c>graph</c></returns>
        int ChooseBestNbr(PheromoneGraph graph, List<int> unvisitedNbrs)
        {
            double bestNbrScore = ScoreEdge(graph, unvisitedNbrs[0]);
            int bestNbrIndex = 0;
            for (int i = 1; i < unvisitedNbrs.Count; i++)
            {
                double score = ScoreEdge(graph, unvisitedNbrs[i]);
                if (score > bestNbrScore)
                {
                    bestNbrScore = score;
                    bestNbrIndex = i;
                }
            }
            return unvisitedNbrs[bestNbrIndex];
        }

        /// <summary>
        /// Choose next neighboring vertex from <c>unvisitedNbrs</c> in <c>graph</c>. Randomly choose if choice is made in exploitation or exploration manner.
        /// <list type="bullet">
        /// <item>
        /// exploitation – choose vertex whose edge has the highest score
        /// </item>
        /// <item>
        /// exploration – choose vertex randomly biased towards ones whose edges have higher scores
        /// </item>
        /// </list>
        /// </summary>
        /// <param name="graph">Graph in which to find neighbor</param>
        /// <param name="unvisitedNbrs">Neighboring vertices from which to choose</param>
        /// <returns>Next neighboring vertex from <c>unvisitedNbrs</c> in <c>graph</c></returns>
        protected override int ChooseNbr(PheromoneGraph graph, List<int> unvisitedNbrs)
        {
            // Console.WriteLine($"Proportional rule");

            double rndNum = rnd.NextDouble();
            if (rndNum <= ExploProportionConst)
            {
                return ChooseBestNbr(graph, unvisitedNbrs);
            }
            else
            {
                return ChooseRandomNbr(graph, unvisitedNbrs);
            }
        }
    }
}