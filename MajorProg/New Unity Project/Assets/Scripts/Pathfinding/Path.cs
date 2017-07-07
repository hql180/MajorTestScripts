using System;
using System.Collections;
using System.Collections.Generic;

public interface IHasNeighbours<N>
{
	IEnumerable<N> Neighbours { get; }
}

public class Path<Node> : IEnumerable<Node>
{
	public Node LastStep { get; private set; }

	public Path<Node> PreviousSteps { get; private set; }

	public double TotalCost { get; private set; }

	private Path(Node lastStep, Path<Node> previousSteps, double totalCost)
	{
		LastStep = lastStep;
		PreviousSteps = previousSteps;
		TotalCost = totalCost;
	}

	public Path(Node start): this(start, null, 0) { }

	public Path<Node> AddStep(Node step, double stepCost)
	{
		return new Path<Node>(step, this, TotalCost + stepCost);
	}
	
	public IEnumerator<Node> GetEnumerator()
	{
		for (Path<Node> p = this; p != null; p = p.PreviousSteps)
			yield return p.LastStep;
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public static Path<TNode> FindPath<TNode>(
		TNode start,
		TNode destination,
		Func<TNode, TNode, double> distance,
		Func<TNode, double> estimate)
		where TNode : IHasNeighbours<TNode>
	{
		var closed = new HashSet<TNode>();
		var queue = new PriorityQueue<double, Path<TNode>>();
		queue.Enqueue(0, new Path<TNode>(start));

		while(!queue.IsEmpty)
		{
			var path = queue.Dequeue();

			if (closed.Contains(path.LastStep))
				continue;

			if (path.LastStep.Equals(destination))
				return path;

			closed.Add(path.LastStep);

			foreach(TNode n in path.LastStep.Neighbours)
			{
				double d = distance(path.LastStep, n);
				var newPath = path.AddStep(n, d);
				queue.Enqueue(newPath.TotalCost + estimate(n), newPath);
			}
		}
		return null;
	}
}
