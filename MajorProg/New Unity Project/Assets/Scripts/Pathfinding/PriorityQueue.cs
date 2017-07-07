using System.Collections;
using System.Collections.Generic;
using System.Linq;


public class PriorityQueue<PRIORITY, VALUE>
{
	private SortedDictionary<PRIORITY, Queue<VALUE>> list = new SortedDictionary<PRIORITY, Queue<VALUE>>();

	public void Enqueue(PRIORITY priority, VALUE value)
	{
		Queue<VALUE> queue;

		if(!list.TryGetValue(priority, out queue))
		{
			queue = new Queue<VALUE>();
			list.Add(priority, queue);
		}

		queue.Enqueue(value);
	}

	public VALUE Dequeue()
	{
		var pair = list.First();
		var value = pair.Value.Dequeue();
		if (pair.Value.Count == 0)
			list.Remove(pair.Key);

		return value;
	}

	public bool IsEmpty
	{
		get { return !list.Any(); }
	}
}
