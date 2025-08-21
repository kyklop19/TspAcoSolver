namespace TspAcoSolver
{
    public abstract class AntBase
    {
        public double TrailLevelFactor { get; init; }
        public double AttractivenessFactor { get; init; }
        public double ExploProportionConst { get; init; }
        public int CurrVertex { get; set; }
        public Tour LastTour { get; set; }

        public AntBase(ColonyParams colonyParams)
        {
            TrailLevelFactor = colonyParams.TrailLevelFactor;
            AttractivenessFactor = colonyParams.AttractivenessFactor;
            ExploProportionConst = colonyParams.ExploProportionConst;
        }

        public void FindTour(PheromoneGraph graph)
        {
            LastTour = new(graph);

            Random rnd = new Random();
            CurrVertex = rnd.Next(0, graph.VertexCount);
            LastTour.Add(CurrVertex);

            while (!LastTour.HasAllVerteces() && !LastTour.HasDeadEnd())
            {
                CurrVertex = ChooseNbr(graph, LastTour.NextPossibleVertices());
                LastTour.Add(CurrVertex);
            }
        }

        protected double ScoreEdge(PheromoneGraph graph, int nbr)
        {
            double attractiveness = 1 / graph.Weight(CurrVertex, nbr);
            double trailLevel = graph.Pheromones[CurrVertex, nbr];
            double score = Math.Pow(trailLevel, TrailLevelFactor) * Math.Pow(attractiveness, AttractivenessFactor);
            return score;
        }

        protected int ChooseRandomNbr(PheromoneGraph graph, List<int> unvisited_nbrs)
        {
            double sum = 0;

            double[] probabilities = new double[unvisited_nbrs.Count];
            for (int i = 0; i < unvisited_nbrs.Count; i++)
            {
                double score = ScoreEdge(graph, unvisited_nbrs[i]);
                probabilities[i] = score;
                sum += score;
            }
            for (int i = 0; i < unvisited_nbrs.Count; i++)
            {
                probabilities[i] /= sum;
                if (i != 0)
                {
                    probabilities[i] += probabilities[i - 1];
                }
            }

            Random rnd = new();
            double rndNum = rnd.NextDouble();
            int index = 0;
            while (index != probabilities.Length - 1 && rndNum > probabilities[index]) index++;

            // System.Console.WriteLine($"Rnd: {rndNum}");
            // foreach (double prob in probabilities)
            // {
            //     System.Console.Write($"{prob} ");
            // }
            // System.Console.WriteLine($"Index: {index}");


            return unvisited_nbrs[index];
        }

        protected abstract int ChooseNbr(PheromoneGraph graph, List<int> unvisited_nbrs);
    }

    public class AsAnt : AntBase
    {
        public AsAnt(ColonyParams colonyParams)
            : base(colonyParams) { }
        protected override int ChooseNbr(PheromoneGraph graph, List<int> unvisited_nbrs)
        {
            return ChooseRandomNbr(graph, unvisited_nbrs);
        }
    }

    public class AcsAnt : AntBase
    {
        public AcsAnt(ColonyParams colonyParams)
            : base(colonyParams) { }

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
            Random rnd = new();
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