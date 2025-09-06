using Microsoft.Extensions.Options;

namespace TspAcoSolver
{
    /// <summary>
    /// Interface for ant class that is intended to find a tour in pheromone graph
    /// </summary>
    public interface IAnt
    {
        public Tour LastTour { get; set; }
        public void FindTour(PheromoneGraph graph);
    }

    /// <summary>
    /// <c>AntBase</c> is abstract class intended for finding tour in given pheromone graph.
    /// </summary>
    public abstract class AntBase : IAnt
    {
        public int CurrVertex { get; set; }
        public Tour LastTour { get; set; }

        protected IRandom rnd;

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
            LastTour = new(graph);

            CurrVertex = rnd.Next(0, graph.VertexCount);
            LastTour.Add(CurrVertex);

            while (!LastTour.HasAllVertices() && !LastTour.HasDeadEnd())
            {
                CurrVertex = ChooseNbr(graph, LastTour.NextPossibleVertices());
                LastTour.Add(CurrVertex);
            }
        }
        protected abstract int ChooseNbr(PheromoneGraph graph, List<int> unvisited_nbrs);
    }

    /// <summary>
    /// <c>NearestNbrAnt</c> is class intended for finding tour in given
    /// pheromone graph by always choosing the closest vertex.
    /// </summary>
    public class NearestNbrAnt : AntBase
    {
        public NearestNbrAnt(IRandom rnd) : base(rnd) { }
        protected override int ChooseNbr(PheromoneGraph graph, List<int> unvisitedNbrs)
        {
            double nearestNbrDist = graph.Weight(CurrVertex, unvisitedNbrs[0]);
            int nearestNbrIndex = 0;
            for (int i = 1; i < unvisitedNbrs.Count; i++)
            {
                double dist = graph.Weight(CurrVertex, unvisitedNbrs[i]);
                if (dist > nearestNbrDist)
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
        public double TrailLevelFactor { get; init; }
        public double AttractivenessFactor { get; init; }
        public double ExploProportionConst { get; init; }

        public RandomAntBase(IOptions<ColonyParams> colonyParams, IRandom rnd) : base(rnd)
        {
            TrailLevelFactor = colonyParams.Value.TrailLevelFactor;
            AttractivenessFactor = colonyParams.Value.AttractivenessFactor;
            ExploProportionConst = colonyParams.Value.ExploProportionConst;
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

    public class AsAnt : RandomAntBase
    {
        public AsAnt(IOptions<ColonyParams> colonyParams, IRandom rnd)
            : base(colonyParams, rnd) { }
        protected override int ChooseNbr(PheromoneGraph graph, List<int> unvisited_nbrs)
        {
            return ChooseRandomNbr(graph, unvisited_nbrs);
        }
    }

    public class AcsAnt : RandomAntBase
    {
        public AcsAnt(IOptions<ColonyParams> colonyParams, IRandom rnd)
            : base(colonyParams, rnd) { }

        int ChooseBestNbr(PheromoneGraph graph, List<int> unvisited_nbrs)
        {
            double bestNbrScore = ScoreEdge(graph, unvisited_nbrs[0]);
            int bestNbrIndex = 0;
            for (int i = 1; i < unvisited_nbrs.Count; i++)
            {
                double score = ScoreEdge(graph, unvisited_nbrs[i]);
                if (score > bestNbrScore)
                {
                    bestNbrScore = score;
                    bestNbrIndex = i;
                }
            }
            return unvisited_nbrs[bestNbrIndex];
        }

        protected override int ChooseNbr(PheromoneGraph graph, List<int> unvisited_nbrs)
        {
            // Console.WriteLine($"Proportional rule");

            double rndNum = rnd.NextDouble();
            if (rndNum <= ExploProportionConst)
            {
                return ChooseBestNbr(graph, unvisited_nbrs);
            }
            else
            {
                return ChooseRandomNbr(graph, unvisited_nbrs);
            }
        }
    }
}