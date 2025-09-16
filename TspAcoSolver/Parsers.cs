using System.Drawing;
using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.VisualBasic.FileIO;

namespace TspAcoSolver
{
    /// <summary>
    /// Interface for parsers that parse files with traveling salesman problem
    /// </summary>
    interface IParser
    {
        /// <summary>
        /// Parse file with traveling salesman problem whose path is <c>path</c>
        /// </summary>
        /// <param name="path">Path to the file with traveling salesman problem</param>
        /// <returns></returns>
        public IProblem Parse(string path);
    }

    /// <summary>
    /// Parser for parsing files with traveling salesman problem in <c>.tsp</c> file format
    /// </summary>
    public class TspParser : IParser
    {
        /// <summary>
        /// Parse file with traveling salesman problem in <c>.tsp</c> file format whose path is <c>path</c>
        /// </summary>
        /// <param name="path">Path to the file with traveling salesman problem</param>
        /// <returns></returns>
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
                            double[] coord = new double[strCoord.Length - 1];
                            for (int i = 1; i < strCoord.Length; i++)
                            {
                                coord[i - 1] = Convert.ToDouble(strCoord[i], new CultureInfo("en-US"));
                            }
                            coords.Add(coord);
                        }
                    }

                }
            }

            Match match = Regex.Match(edgeWeightType, @"^EUC_(\d)D$");

            if (match.Success)
            {
                res = new EuclidTravelingSalesmanProblem(Convert.ToInt32(match.Groups[1].Value), coords);
            }
            else
            {
                throw new Exception($"Parsing of problem type {edgeWeightType} is currently not implemented.");
            }
            return res;
        }
    }

    /// <summary>
    /// Parser for parsing files with traveling salesman problem in <c>.csv</c> file format
    /// </summary>
    public class CsvParser : IParser
    {
        /// <summary>
        /// Parse file with traveling salesman problem in <c>.csv</c> file format whose path is <c>path</c>
        /// </summary>
        /// <param name="path">Path to the file with traveling salesman problem</param>
        /// <returns></returns>
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