using System.Drawing;
using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.VisualBasic.FileIO;

namespace TspAcoSolver
{
    interface IParser
    {
        public IProblem Parse(string path);
    }
    public class TspParser : IParser
    {
        public IProblem Parse(string path)
        {
            IProblem? res = null; //TODO: change?
            string edgeWeightType = "";
            List<double[]> coords = new();

            bool readingData = false;

            using (StreamReader reader = new(path))
            {
                string line = "";
                while (line != "EOF" && line != null)
                {
                    line = reader.ReadLine();
                    if (line.Contains(":"))
                    {
                        string[] keyval = Regex.Split(line, @" *: *");
                        switch (keyval[0])
                        {
                            case "EDGE_WEIGHT_TYPE":
                                edgeWeightType = keyval[1];
                                break;
                        }
                    }
                    else
                    {
                        if (line == "EOF") //TODO: remove
                        {
                            readingData = false;
                        }
                        if (!readingData)
                        {
                            switch (line)
                            {
                                case "NODE_COORD_SECTION":
                                    readingData = true;
                                    break;
                            }
                        }
                        else
                        {
                            string[] strCoord = line.Split(" ", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                            double[] coord = new double[strCoord.Length-1];
                            for (int i = 1; i < strCoord.Length; i++)
                            {
                                coord[i-1] = Convert.ToDouble(strCoord[i], new CultureInfo("en-US"));
                            }
                            coords.Add(coord);
                        }
                    }

                }
            }
            switch (edgeWeightType)
            {
                case "EUC_2D":
                    res = new EuclidTravelingSalesmanProblem(2, coords);
                    break;
            }
            return res;
        }
    }
    public class CsvParser : IParser
    {
        public IProblem Parse(string path)
        {
            List<Pathway> pathways = new();
            int vertexCount = 0; //TODO: rename size and put into problem class
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

                    if (from + 1 > vertexCount)
                    {
                        vertexCount = from + 1;
                    }
                    if (to + 1 > vertexCount)
                    {
                        vertexCount = to + 1;
                    }

                    Pathway pathway = new Pathway(
                                                    from,
                                                    to,
                                                    Convert.ToDouble(cols[2])
                                                    );
                    pathways.Add(pathway);
                }
            }
            return new TravelingSalesmanProblem(vertexCount, pathways);
        }
    }
}