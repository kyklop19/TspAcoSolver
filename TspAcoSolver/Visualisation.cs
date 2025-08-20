using System.Windows.Forms.Design;
using Microsoft.Msagl.Core.Layout;
using Microsoft.Msagl.Drawing;

namespace TspAcoSolver
{
    public class Visualiser
    {
        PheromoneGraph _pheromoneGraph;
        SpaceTravelingSalesmanProblem _problem;
        System.Windows.Forms.Form _form;
        Microsoft.Msagl.GraphViewerGdi.GViewer _viewer;
        Microsoft.Msagl.Drawing.Graph _graph;

        void VisualiseGraph()
        {
            for (int i = 0; i < _problem.Size; i++)
            {
                Microsoft.Msagl.Drawing.Node node = _graph.AddNode($"{i}");
            }
            // for (int source = 0; source < _pheromoneGraph.VertexCount; source++)
            // {
            //     foreach (int target in _pheromoneGraph.AdjList[source])
            //     {
            //         Microsoft.Msagl.Drawing.Edge edge = _graph.AddEdge($"{source}", $"{_pheromoneGraph.Pheromones[source, target]}", $"{target}");
            //         edge.Attr.Length = _pheromoneGraph.Weight(source, target);
            //         // edge.Attr.LineWidth = _pheromoneGraph.Pheromones[source, target];
            //     }
            // }


            //create the graph content
            // for (int i = 0; i < 280; i++)
            // {
            //     _graph.AddEdge($"{i}", $"{i+1}");
            // }
            // _graph.AddEdge("A", "B");
            // _graph.AddEdge("B", "C");
            // _graph.AddEdge("A", "C").Attr.Color = Microsoft.Msagl.Drawing.Color.Green;
            // _graph.FindNode("A").Attr.FillColor = Microsoft.Msagl.Drawing.Color.Magenta;
            // _graph.FindNode("B").Attr.FillColor = Microsoft.Msagl.Drawing.Color.MistyRose;
            // Microsoft.Msagl.Drawing.Node c = _graph.FindNode("C");
            // c.Attr.FillColor = Microsoft.Msagl.Drawing.Color.PaleGreen;
            // c.Attr.Shape = Microsoft.Msagl.Drawing.Shape.Diamond;
        }

        public Visualiser(PheromoneGraph pheromoneGraph, SpaceTravelingSalesmanProblem problem)
        {
            _pheromoneGraph = pheromoneGraph;
            _problem = problem;
        }
        void Show()
        {
            _viewer = new Microsoft.Msagl.GraphViewerGdi.GViewer();
            _graph = new Microsoft.Msagl.Drawing.Graph("graph");
            VisualiseGraph();
            _viewer.Graph = _graph;
            for (int i = 0; i < _problem.Size; i++)
            {
                Microsoft.Msagl.Drawing.Node node = _graph.FindNode($"{i}");
                Microsoft.Msagl.Core.Geometry.Point point = new(_problem.Coords[i][0], _problem.Coords[i][1]);
                node.GeometryNode.Center = point;

            }
            _viewer.Graph = null;
            _viewer.Graph = _graph;
            _form = new System.Windows.Forms.Form();
            _form.SuspendLayout();
            _viewer.Dock = System.Windows.Forms.DockStyle.Fill;
            _form.Controls.Add(_viewer);
            _form.ResumeLayout();
            _form.ShowDialog();
        }
        public void Visualise()
        {
            Thread t = new Thread(new ThreadStart(Show));
            t.Start();
        }
    }
}