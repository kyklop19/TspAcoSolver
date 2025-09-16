using System.Windows.Forms.Design;
using Microsoft.Msagl.Core.Layout;
using Microsoft.Msagl.Drawing;
using ScottPlot.WinForms;
using ScottPlot;
using ScottPlot.Plottables;
using System.Linq;

namespace TspAcoSolver
{
    /// <summary>
    /// Visualizer for visualizing graph
    /// </summary>
    public class Visualiser
    {
        PheromoneGraph _pheromoneGraph;
        SpaceTravelingSalesmanProblem _problem;
        System.Windows.Forms.Form _form;
        Microsoft.Msagl.GraphViewerGdi.GViewer _viewer;
        Microsoft.Msagl.Drawing.Graph _graph;

        /// <summary>
        /// Render graph
        /// </summary>
        void VisualiseGraph()
        {
            for (int i = 0; i < _problem.CityCount; i++)
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

        /// <summary>
        /// Construct from graph and problem
        /// </summary>
        /// <param name="pheromoneGraph">Graph of <c>problem</c> to visualize</param>
        /// <param name="problem">Problem to visualize</param>
        public Visualiser(PheromoneGraph pheromoneGraph, SpaceTravelingSalesmanProblem problem)
        {
            _pheromoneGraph = pheromoneGraph;
            _problem = problem;
        }

        /// <summary>
        /// Show window with visualization
        /// </summary>
        void Show()
        {
            _viewer = new Microsoft.Msagl.GraphViewerGdi.GViewer();
            _graph = new Microsoft.Msagl.Drawing.Graph("graph");
            VisualiseGraph();
            _viewer.Graph = _graph;
            for (int i = 0; i < _problem.CityCount; i++)
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

        /// <summary>
        /// Start showing window from separate thread
        /// </summary>
        public void Visualise()
        {
            Thread t = new Thread(new ThreadStart(Show));
            t.Start();
        }
    }


    /// <summary>
    /// Interface for visualizing <c>PheromoneGraph</c>.
    /// </summary>
    public interface IPheromoneGraphVisualiser
    {
        /// <summary>
        /// Load <c>graph</c> that should be visualized.
        /// </summary>
        /// <param name="graph"><c>PheromoneGraph</c> that visualized</param>
        public void SetGraph(PheromoneGraph graph);

        /// <summary>
        /// Refresh the visuals of the visualizer intended for when <c>PheromoneGraph</c> changes.
        /// </summary>
        public void Refresh();
    }

    /// <summary>
    /// Visualizer of <c>PheromoneGraph</c> that doesn't visualize anything.
    /// </summary>
    public class NullPheromoneGraphVisualiser : IPheromoneGraphVisualiser
    {
        /// <summary>
        /// Doesn't set the <c>graph</c>
        /// </summary>
        /// <param name="graph"></param>
        public void SetGraph(PheromoneGraph graph) { }
        /// <summary>
        /// Doesn't refresh
        /// </summary>
        public void Refresh() { }
    }

    /// <summary>
    /// Visualizer that shows the pheromone matrix of the <c>PheromoneGraph</c> as a heatmap.
    /// </summary>
    public class HeatmapPheromoneVisualiser : IPheromoneGraphVisualiser
    {
        Form _form = new();
        FormsPlot _formsPlot;
        double[,] _pheromones;
        Thread _thread;

        bool _enableLabels = false;

        /// <summary>
        /// Render labels with amount of pheromones on edge
        /// </summary>
        void AddLabels()
        {
            for (int y = 0; y < _pheromones.GetLength(0); y++)
            {
                for (int x = 0; x < _pheromones.GetLength(1); x++)
                {
                    Coordinates coord = new(x, y);
                    string label = _pheromones[y, x].ToString();
                    Text text = _formsPlot.Plot.Add.Text(label, coord);
                    text.Alignment = Alignment.MiddleCenter;
                    text.LabelFontSize = 10;
                    text.LabelFontColor = Colors.Red;
                    text.LabelText = label;
                }
            }
        }

        /// <summary>
        /// Rerender heatmap in the form
        /// </summary>
        void DrawHeatmap()
        {
            _formsPlot.Plot.Clear();
            Heatmap hm = _formsPlot.Plot.Add.Heatmap(_pheromones);
            if (_enableLabels)
            {
                AddLabels();
            }
            _formsPlot.Refresh();
        }

        /// <summary>
        /// Toggle visibility of the pheromone amount labels
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ToggleLabels(object sender, EventArgs e)
        {
            _enableLabels = !_enableLabels;
            Refresh();
        }

        /// <summary>
        /// Load the visualized <c>graph</c> and start the visualizer in separate thread.
        /// </summary>
        /// <param name="graph">Graph to be visualized</param>
        public void SetGraph(PheromoneGraph graph)
        {
            _pheromones = graph.Pheromones;
            using (ManualResetEvent resetEvent = new(false))
            {
                _thread = new Thread(() =>
                {
                    _formsPlot = new()
                    {
                        Dock = DockStyle.Fill
                    };

                    resetEvent.Set();

                    DrawHeatmap();

                    TableLayoutPanel panel = new()
                    {
                        Dock = DockStyle.Fill
                    };
                    panel.RowCount = 2;
                    Button btn = new()
                    {
                        Dock = DockStyle.Fill,
                        Text = "Toggle labels",
                    };
                    btn.Click += new EventHandler(ToggleLabels);
                    panel.RowStyles.Add(new RowStyle(SizeType.Percent, 90f));
                    panel.RowStyles.Add(new RowStyle(SizeType.Percent, 10f));
                    panel.Controls.Add(_formsPlot, 0, 0);
                    panel.Controls.Add(btn, 0, 1);
                    _form.Controls.Add(panel);

                    _form.ShowDialog();
                });
                _thread.Start();
                resetEvent.WaitOne();
            }
        }

        /// <summary>
        /// Invoke rerendering of the heatmap
        /// </summary>
        public void Refresh()
        {
            _formsPlot.Invoke(DrawHeatmap);
            _form.Invoke(() => { _form.Update(); });
        }

        // ~HeatmapPheromoneVisualiser()
        // {
        //     _form.Close();
        //     _thread.Join();
        //     Console.WriteLine($"window closed");

        // }
    }
}