using System.Formats.Asn1;
using TspAcoSolver;

namespace Tests;

[TestClass]
public sealed class TourTests
{
    WeightedGraph _wGraph;

    Tour _tour;

    [TestInitialize]
    public void TestInitialize()
    {
        _wGraph = new(new double[,]
        {
        {  1,  2,  3, -4 },
        {  5, -6,  7,  8 },
        {  0, 10,  12, 9 },
        { 14, 13.8, Double.NaN,  6 },
        });
        _tour = new(_wGraph);
    }

    [TestMethod]
    public void TestAdd()
    {
        _tour.Add(0);
        _tour.Add(1);
        _tour.Add(2);
        _tour.Add(3);

        CollectionAssert.AreEqual(new List<int> { 0, 1, 2, 3 }, _tour.Vertices);
    }

    [TestMethod]
    [ExpectedException(typeof(DuplicateVertexException))]

    public void TestAdd_Duplicate()
    {
        _tour.Add(0);
        _tour.Add(0);
    }

    [TestMethod]
    [ExpectedException(typeof(DuplicateVertexException))]

    public void TestAdd_CompleteTour()
    {
        _tour.Add(0);
        _tour.Add(1);
        _tour.Add(2);
        _tour.Add(3);
        _tour.Add(0);
    }

    [TestMethod]
    public void TestLength_Complete()
    {
        _tour.Add(0);
        _tour.Add(3);
        _tour.Add(1);
        _tour.Add(2);

        Assert.AreEqual(-4+13.8+7+0, _tour.Length);
    }

    [TestMethod]
    [ExpectedException(typeof(IncompleteTourException))]
    public void TestLength_Incomplete()
    {
        double len = _tour.Length;
    }

    [TestMethod]
    public void TestHasDeadEnd_EmptyTour()
    {
        Assert.IsFalse(_tour.HasDeadEnd());
    }

    [TestMethod]
    public void TestHasDeadEnd_CompleteTour()
    {
        _tour.Add(0);
        _tour.Add(1);
        _tour.Add(2);
        _tour.Add(3);
        Assert.IsTrue(_tour.HasDeadEnd());
    }

    [TestMethod]
    public void TestHasDeadEnd_DeadEnd()
    {
        _tour.Add(0);
        _tour.Add(1);
        _tour.Add(3);
        foreach (int v in _tour.NextPossibleVertices())
        {
            Console.WriteLine($"{v}");

        }
        Console.WriteLine($"{_tour.NextPossibleVertices()}");

        Assert.IsTrue(_tour.HasDeadEnd());
    }
}
