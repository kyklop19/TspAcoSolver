namespace TspAcoSolver
{
    public class Ant
    {
        public int CurrVertex { get; set; }
        public Tour LastTour { get; set; }

        public void FindTour(PheromoneGraph graph)
        {
            LastTour = new();

            Random rnd = new Random();
            CurrVertex = rnd.Next(0, graph.VertexCount);
            LastTour.Add(new Nbr(CurrVertex, 0));
            HashSet<int> unvisited = new();

            for (int i = 0; i < graph.VertexCount; i++)
            {
                unvisited.Add(i);
            }

            bool FoundDeadEnd = false;
            //TODO: add last edge
            Nbr CurrNbr;
            while (unvisited.Count != 0 && !FoundDeadEnd)
            {
                CurrNbr = ChooseNbr(graph, unvisited);
                CurrVertex = CurrNbr.Vertex;
                if (CurrVertex == -1)
                {
                    FoundDeadEnd = true;
                }
                else
                {
                    unvisited.Remove(CurrVertex);
                    LastTour.Add(CurrNbr);
                }
            }
        }

        Nbr ChooseNbr(PheromoneGraph graph, HashSet<int> unvisited)
        {
            double sum = 0;
            List<Nbr> nbrs = graph.Nbrs(CurrVertex);
            List<Nbr> unvisited_nbrs = new List<Nbr>();
            foreach (Nbr nbr in nbrs)
            {
                if (unvisited.Contains(nbr.Vertex))
                {
                    unvisited_nbrs.Add(nbr);
                }
            }
            if (unvisited_nbrs.Count == 0)
            {
                return new Nbr(-1, -1);
            }
            nbrs = unvisited_nbrs;
            double[] probabilities = new double[nbrs.Count];
            for (int i = 0; i < nbrs.Count; i++)
            {
                double attractiveness = 1 / nbrs[i].Dist;
                double trailLevel = graph.Pheromones[CurrVertex, nbrs[i].Vertex];
                double probability = trailLevel * attractiveness;
                probabilities[i] = probability;
                sum += probability;
            }
            for (int i = 0; i < nbrs.Count; i++)
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
            while (index != probabilities.Length-1 && rndNum < probabilities[index]) index++;

            return nbrs[index];
        }
    }
}