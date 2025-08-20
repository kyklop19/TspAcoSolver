namespace TspAcoSolver
{
    public class Ant
    {
        public double TrailLevelFactor { get; init; }
        public double AttractivenessFactor { get; init; }
        public int CurrVertex { get; set; }
        public Tour LastTour { get; set; }

        public Ant(double trailLevelFactor, double attractivenessFactor)
        {
            TrailLevelFactor = trailLevelFactor;
            AttractivenessFactor = attractivenessFactor;
        }

        public void FindTour(PheromoneGraph graph)
        {
            LastTour = new(graph);

            Random rnd = new Random();
            int firstVertex = rnd.Next(0, graph.VertexCount);
            CurrVertex = firstVertex;
            LastTour.Add(CurrVertex);

            while (!LastTour.HasAllVerteces() && !LastTour.HasDeadEnd())
            {
                CurrVertex = ChooseNbr(graph, LastTour.NextPossibleVertices());
                LastTour.Add(CurrVertex);
            }
        }

        double ScoreEdge(PheromoneGraph graph, int nbr)
        {
            double attractiveness = 1 / graph.Weight(CurrVertex, nbr);
            double trailLevel = graph.Pheromones[CurrVertex, nbr];
            double score = Math.Pow(trailLevel, TrailLevelFactor) * Math.Pow(attractiveness, AttractivenessFactor);
            return score;
        }

        int ChooseNbr(PheromoneGraph graph, List<int> unvisited_nbrs)
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
    }
}