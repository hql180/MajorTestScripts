using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public struct Point
{
	public int X, Y;
	public Point(int x, int y)
	{
		X = x;
		Y = y;
	}
}

public abstract class GridObject
{
	public Point Location;
	public int X { get { return Location.X; } }
	public int Y { get { return Location.Y; } }

	public GridObject(Point location)
	{
		Location = location;
	}

	public GridObject(int x, int y): this(new Point(x, y)) { }

	public override string ToString()
	{
		return string.Format("({0}, {1})", X, Y);
	}
}

public interface IHasNeighbours<N>
{
	IEnumerable<N> Neighbours { get; }
}

public class Tile: GridObject, IHasNeighbours<Tile>
{
	public bool Passable;
	public Tile(int x, int y) : base(x,y)
	{
		Passable = true;
	}

	public IEnumerable<Tile> AllNeighbours { get; set; }
	
	public IEnumerable<Tile> Neighbours
	{
		get { return AllNeighbours.Where(o => o.Passable); }
	}
	
	//change of coordinates when moving in any direction
	public static List<Point> NeighbourShift
	{
		get
		{
			return new List<Point>
				{
					new Point(0, 1),
					new Point(1, 0),
					new Point(1, -1),
					new Point(0, -1),
					new Point(-1, 0),
					new Point(-1, 1),
				};
		}
	}

	public void FindNeighbours(Dictionary<Point, Tile> Board,
		Vector2 BoardSize, bool EqualLineLengths)
	{
		List<Tile> neighbours = new List<Tile>();

		foreach (Point point in NeighbourShift)
		{
			int neighbourX = X + point.X;
			int neighbourY = Y + point.Y;
			//x coordinate offset specific to straight axis coordinates
			int xOffset = neighbourY / 2;

			//If every second hexagon row has less hexagons than the first one, just skip the last one when we come to it
			if (neighbourY % 2 != 0 && !EqualLineLengths &&
				neighbourX + xOffset == BoardSize.x - 1)
				continue;
			//Check to determine if currently processed coordinate is still inside the board limits
			if (neighbourX >= 0 - xOffset &&
				neighbourX < (int)BoardSize.x - xOffset &&
				neighbourY >= 0 && neighbourY < (int)BoardSize.y)
				neighbours.Add(Board[new Point(neighbourX, neighbourY)]);
		}

		AllNeighbours = neighbours;
	}
}

public class Path<Node>: IEnumerable<Node>
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
	public Path(Node start) : this(start, null, 0) { }
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
		return this.GetEnumerator();
	}
}

class PriorityQueue<P, V>
{
	private SortedDictionary<P, Queue<V>> list = new SortedDictionary<P, Queue<V>>();
	public void Enqueue(P priority, V value)
	{
		Queue<V> q;
		if (!list.TryGetValue(priority, out q))
		{
			q = new Queue<V>();
			list.Add(priority, q);
		}
		q.Enqueue(value);
	}
	public V Dequeue()
	{
		// will throw if there isn’t any first element!
		var pair = list.First();
		var v = pair.Value.Dequeue();
		if (pair.Value.Count == 0) // nothing left of the top priority.
			list.Remove(pair.Key);
		return v;
	}
	public bool IsEmpty
	{
		get { return !list.Any(); }
	}
}

public static class PathFinder
{
	public static Path<Tile> FindPath(Tile start, Tile destination, Func<Tile, Tile, double> dis, Func<Tile, double> e)
	{
		var closed  = new HashSet<Tile>();

		var queue = new PriorityQueue<double ,Path<Tile> >();
		queue.Enqueue(0, new Path<Tile>(start));

		while(!queue.IsEmpty)
		{
			var path = queue.Dequeue();

			if (closed.Contains(path.LastStep))
				continue;
			if (path.LastStep.Equals(destination))
				return path;


			closed.Add(path.LastStep);

			foreach(Tile n in path.LastStep.Neighbours)
			{
				double d = distance(path.LastStep, n);

				var newPath = path.AddStep(n, d);
				queue.Enqueue(newPath.TotalCost + estimate(n, destination), newPath);
			}

		}

		return null;
	}

	static double distance(Tile tile1, Tile tile2)
	{
		return 1;
	}

	static double estimate(Tile tile, Tile destTile)
	{
		float dx = Mathf.Abs(destTile.X - tile.X);
		float dy = Mathf.Abs(destTile.Y - tile.Y);
		int z1 = -(tile.X + tile.Y);
		int z2 = -(destTile.X + destTile.Y);
		float dz = Mathf.Abs(z2 - z1);

		return Mathf.Max(dx, dy, dz);
	}
}

public class GridManager : MonoBehaviour
{
	public Tile selectedTile = null;

	public TileBehaviour originTileTB = null;

	public TileBehaviour destTileTB = null;

	public static GridManager instance = null;

	public GameObject Line;
	//List to hold "Lines" indicating the path
	List<GameObject> path;

	public GameObject hexTile;

	public GameObject ground;

	public GameObject[] colliders;

	public float hexWidth;
	public float hexHeight;
	public float groundWidth;
	public float groundHeight;

	public Dictionary<Point, Tile> Board = new Dictionary<Point, Tile>();

	public GameObject hexGridGo;

	void SetSizes()
	{
		var hexRend = hexTile.GetComponent<Renderer>();
		var groundRend = ground.GetComponent<Renderer>();

		hexWidth = hexRend.bounds.size.x;
		hexHeight = hexRend.bounds.size.z;

		groundWidth = groundRend.bounds.size.x;
		groundHeight = groundRend.bounds.size.z;
	}

	//The method used to calculate the number hexagons in a row and number of rows
	//Vector2.x is gridWidthInHexes and Vector2.y is gridHeightInHexes
	Vector2 CalcGridSize()
	{
		//According to the math textbook hexagon's side length is half of the height
		float sideLength = hexHeight / 2;
		//the number of whole hex sides that fit inside inside ground height
		int nrOfSides = (int)(groundHeight / sideLength);
		//I will not try to explain the following calculation because I made some assumptions, which might not be correct in all cases, to come up with the formula. So you'll have to trust me or figure it out yourselves.
		int gridHeightInHexes = (int)(nrOfSides * 2 / 3);
		//When the number of hexes is even the tip of the last hex in the offset column might stick up.
		//The number of hexes in that case is reduced.
		if (gridHeightInHexes % 2 == 0
			&& (nrOfSides + 0.5f) * sideLength > groundHeight)
			gridHeightInHexes--;
		//gridWidth in hexes is calculated by simply dividing ground width by hex width
		return new Vector2((int)(groundWidth / hexWidth), gridHeightInHexes);
	}

	Vector3 CalcInitPos()
	{
		Vector3 initPos;
		initPos = new Vector3(-groundWidth / 2 + hexWidth / 2, 0, groundHeight / 2 - hexWidth / 2);
		return initPos;
	}

	Vector3 CalcWorldCoord(Vector2 gridPos)
	{
		Vector3 initPos = CalcInitPos();
		float offset = 0;
		if(gridPos.y % 2 != 0)
		{
			offset = hexWidth / 2;
		}

		float x = initPos.x + offset + gridPos.x * hexWidth;
		float z = initPos.z - gridPos.y * hexHeight * 0.75f;

		return new Vector3(x, 0, z);
	}

	void CreateGrid()
	{
		//Vector2 gridSize = CalcGridSize();
		//hexGridGo = new GameObject("HexGrid");

		//for (float y = 0; y < gridSize.y; ++y) 
		//{
		//	float sizeX = gridSize.x;

		//	if (y % 2 != 0 && (gridSize.x + 0.5) * hexWidth > groundWidth) 
		//	{
		//		sizeX--;
		//	}

		//	for (float x = 0; x < sizeX; ++x)
		//	{
		//		GameObject hex = (GameObject)Instantiate(hexTile);
		//		hex.transform.tag = "DestroyMe";
		//		Vector2 gridPos = new Vector2(x, y);
		//		hex.transform.position = CalcWorldCoord(gridPos);
		//		hex.transform.parent = hexGridGo.transform;
		//	}
		//}

		Vector2 gridSize = CalcGridSize();
		hexGridGo = new GameObject("HexGrid");
		//board is used to store tile locations
		//Dictionary<Point, Tile> board = new Dictionary<Point, Tile>();




		for (float y = 0; y < gridSize.y; y++)
		{
			float sizeX = gridSize.x;
			//if the offset row sticks up, reduce the number of hexes in a row
			if (y % 2 != 0 && (gridSize.x + 0.5) * hexWidth > groundWidth)
				sizeX--;
			for (float x = 0; x < sizeX; x++)
			{
				GameObject hex = (GameObject)Instantiate(hexTile);
				Vector2 gridPos = new Vector2(x, y);
				hex.transform.position = CalcWorldCoord(gridPos);
				hex.transform.parent = hexGridGo.transform;
				hex.transform.tag = "DestroyMe";
				var tb = (TileBehaviour)hex.GetComponent("TileBehaviour");
				//y / 2 is subtracted from x because we are using straight axis coordinate system
				tb.tile = new Tile((int)x - (int)(y / 2), (int)y);
				//board.Add(tb.tile.Location, tb.tile);
				Board.Add(tb.tile.Location, tb.tile);
			}
		}
		//variable to indicate if all rows have the same number of hexes in them
		//this is checked by comparing width of the first hex row plus half of the hexWidth with groundWidth
		bool equalLineLengths = (gridSize.x + 0.5) * hexWidth <= groundWidth;
		//Neighboring tile coordinates of all the tiles are calculated
		foreach (Tile tile in Board.Values)
			tile.FindNeighbours(Board, gridSize, equalLineLengths);
	}

	public Vector2 CalcGridPos(Vector3 coord)
	{
		Vector3 initPos = CalcInitPos();
		Vector2 gridPos = new Vector2();
		float offset = 0;
		gridPos.y = Mathf.RoundToInt((initPos.z - coord.z) / (hexHeight * 0.75f));
		if (gridPos.y % 2 != 0)
			offset = hexWidth / 2;
		gridPos.x = Mathf.RoundToInt((coord.x - initPos.x - offset) / hexWidth);
		return gridPos;
	}

	void DestroyExtraGrids()
	{
		var grids = hexGridGo.transform.GetComponentsInChildren<Transform>();

		foreach (var t in colliders)
		{
			var c = t.GetComponent<BoxCollider>();
			if (c != null)
			{
				Collider[] cols = Physics.OverlapBox(c.bounds.center, Vector3.Scale(c.size,  c.transform.localScale / 2), c.transform.rotation);

				foreach (var p in cols)
				{
					p.tag = "Untagged";
				}
			}
			
		}

		foreach (var grid in grids)
		{


				if (grid.tag == "DestroyMe")
				{
					var pos = CalcGridPos(grid.transform.position);

					Board.Remove(new Point((int)pos.x, (int)pos.y));

					Destroy(grid.gameObject);
				}
			
		}

	}

	double calcDistance(Tile tile)
	{
		Tile destTile = destTileTB.tile;
		//Formula used here can be found in Chris Schetter's article
		float deltaX = Mathf.Abs(destTile.X - tile.X);
		float deltaY = Mathf.Abs(destTile.Y - tile.Y);
		int z1 = -(tile.X + tile.Y);
		int z2 = -(destTile.X + destTile.Y);
		float deltaZ = Mathf.Abs(z2 - z1);

		return Mathf.Max(deltaX, deltaY, deltaZ);
	}

	private void DrawPath(IEnumerable<Tile> path)
	{
		if (this.path == null)
			this.path = new List<GameObject>();

		this.path.ForEach(Destroy);
		this.path.Clear();

		GameObject lines = GameObject.Find("Lines");
		if (lines == null)
			lines = new GameObject("Lines");
		foreach (Tile tile in path)
		{
			var line = (GameObject)Instantiate(Line);
			Vector2 gridPos = new Vector2(tile.X + tile.Y / 2, tile.Y);
			line.transform.position = CalcWorldCoord(gridPos);
			this.path.Add(line);
			line.transform.parent = lines.transform;
		}
	}

	public void GenerateAndShowPath()
	{
		//Don't do anything if origin or destination is not defined yet
		if (originTileTB == null || destTileTB == null)
		{
			DrawPath(new List<Tile>());
			return;
		}
		//We assume that the distance between any two adjacent tiles is 1
		//If you want to have some mountains, rivers, dirt roads or something else which might slow down the player you should replace the function with something that suits better your needs
		Func<Tile, Tile, double> distance = (node1, node2) => 1;

		var path = PathFinder.FindPath(originTileTB.tile, destTileTB.tile,
			distance, calcDistance);
		DrawPath(path);
	}

	// Use this for initialization
	void Start ()
	{
		instance = this;
		SetSizes();
		CreateGrid();
		DestroyExtraGrids();
		GenerateAndShowPath();
	}
	
	// Update is called once per frame
	void Update ()
	{		
	}
}
