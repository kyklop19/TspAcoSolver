using Microsoft.VisualBasic.FileIO;

namespace TspAcoSolver
{
    public interface IProblem
    {
        public void Read(string path);
        public WeightedGraph ToGraph();
    }

    struct Pathway
    {
        public int From {get; set;}
        public int To {get; set;}
        public double Dist {get; set;}
        public Pathway(int from, int to, double dist)
        {
            From = from;
            To = to;
            Dist = dist;
        }
    }

    public class TravelingSalesmanProblem : IProblem
    {
        List<Pathway> pathways = new List<Pathway>();
        public int Size { get; set; }

        public TravelingSalesmanProblem()
        {
            Size = 0;
        }
        public void Read(string path)
        {
            using (TextFieldParser csvParser = new TextFieldParser(path))
            {
                csvParser.CommentTokens = new string[] { "#" };
                csvParser.SetDelimiters(new string[] { "," });
                csvParser.HasFieldsEnclosedInQuotes = true;

                while (!csvParser.EndOfData)
                {

                    string[] cols = csvParser.ReadFields();
                    int from = Convert.ToInt32(cols[0]);
                    int to = Convert.ToInt32(cols[1]);

                    if (from + 1 > Size)
                    {
                        Size = from + 1;
                    }
                    if (to + 1 > Size)
                    {
                        Size = to + 1;
                    }

                    Pathway pathway = new Pathway(
                                                    from,
                                                    to,
                                                    Convert.ToDouble(cols[2])
                                                    );
                    pathways.Add(pathway);
                }
            }
        }
        public WeightedGraph ToGraph()
        {
            List<Nbr>[] adjList = new List<Nbr>[Size];
            for (int i = 0; i < Size; i++)
            {
                adjList[i] = new List<Nbr>();
            }
            foreach (Pathway pathway in pathways)
            {
                Nbr nbr = new Nbr(pathway.To, pathway.Dist);
                adjList[pathway.From].Add(nbr);
            }
            return new WeightedGraph(adjList);
        }
    }
}