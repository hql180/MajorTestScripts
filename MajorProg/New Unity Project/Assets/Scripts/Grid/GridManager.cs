using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GridType
{
	Cube,
	Hex
}

public class GridManager : MonoBehaviour
{
	public static GridManager instance = null;

	public GameObject linePrefab;
	//List to hold "Lines" indicating the path
	List<GameObject> path;
	HashSet<CellInfo> visted;

	public float tempRange = 5;

	//Pathfinding stuff
	CellInfo originTile = null;
	CellInfo destinationTile = null;

	public GameObject gridPrefab;

	public GameObject groundBounds;

	public GameObject[] colliderObjects;

	public GridType gridType;

	float gridWidth;
	float gridHeight;
	float groundWidth;
	float groundHeight;

	public GameObject hexGridGo;

	// List grid points and related tile data
	public Dictionary<Vector2, CellInfo> map = new Dictionary<Vector2, CellInfo>();

	void Start()
	{
		instance = this;
		SetSizes();
		CreateGrid();
		DestroyExtraGrids();
		FindObstacles();
		foreach (var tile in map)
		{
			tile.Value.tile.FindNeighbours(map);
		}
	}

	// Update is called once per frame
	void Update()
	{
		if (Input.GetMouseButtonDown(0))
		{
			var gridPos = CalcGridPos(GetMouseToWorld());
			Debug.Log(gridPos);
			map.TryGetValue(gridPos, out originTile);
			GenerateAndShowPath();
		}
		if (Input.GetMouseButtonDown(1))
		{
			var gridPos = CalcGridPos(GetMouseToWorld());
			Debug.Log(gridPos);
			map.TryGetValue(gridPos, out destinationTile);
			GenerateAndShowPath();
		}
		if (Input.GetMouseButtonDown(2))
		{
			var gridPos = CalcGridPos(GetMouseToWorld());
			Debug.Log(gridPos);
			Range(gridPos, (int)tempRange);
		}
	}

	void SetSizes()
	{
		var hexRend = (gridPrefab.GetComponent<Renderer>()) ? gridPrefab.GetComponent<Renderer>() : gridPrefab.GetComponentInChildren<Renderer>();
		var groundRend = groundBounds.GetComponent<Renderer>();

		gridWidth = hexRend.bounds.size.x;
		gridHeight = hexRend.bounds.size.z;

		groundWidth = groundRend.bounds.size.x;
		groundHeight = groundRend.bounds.size.z;
	}

	Vector2 CalcGridSize()
	{
		int col = 0;
		int row = 0;

		if (gridType == GridType.Cube)
		{
			col = (int)(groundWidth / gridWidth);
			row = (int)(groundHeight / gridHeight) + 1;
		}
		else if (gridType == GridType.Hex)
		{
			col = (int)(groundWidth / (gridWidth * 3 / 4));
			row = (int)(groundHeight / gridHeight) + 1;
		}

		return new Vector2(col, row);
	}

	Vector3 CalcInitPos()
	{
		Vector3 initPos;
		initPos = new Vector3(-groundWidth / 2 + gridWidth / 2, 0, groundHeight / 2 - gridWidth / 2);
		return initPos;
	}

	Vector3 CalcWorldCoord(Vector2 gridPos)
	{
		Vector3 initPos = CalcInitPos();

		float x = 0;
		float z = 0;

		if (gridType == GridType.Cube)
		{
			x = initPos.x + gridPos.x * gridWidth;
			z = initPos.z - gridPos.y * gridHeight;
		}
		else if (gridType == GridType.Hex)
		{
			x = initPos.x + gridPos.x * gridWidth * 3 / 4;
			z = initPos.z - gridPos.y * gridHeight;
			if (gridPos.x % 2 == 0)
			{
				z += gridHeight * 0.5f;
			}

		}

		return new Vector3(x, 0, z);
	}

	public Vector2 CalcGridPos(Vector3 coord)
	{
		Vector3 initPos = CalcInitPos();
		Vector2 gridPos = new Vector2();

		if (gridType == GridType.Cube)
		{
			gridPos.x = Mathf.RoundToInt((coord.x - initPos.x) / gridWidth);
			gridPos.y = Mathf.RoundToInt((initPos.z - coord.z) / (gridHeight));
		}
		else if (gridType == GridType.Hex)
		{
			gridPos.x = Mathf.RoundToInt((coord.x - initPos.x) / (gridWidth * 3 / 4));
			gridPos.y = Mathf.RoundToInt((initPos.z - coord.z) / (gridHeight));
			if(gridPos.x % 2 == 0)
			{
				gridPos.y = Mathf.RoundToInt((initPos.z - coord.z + (gridHeight * 0.5f)) / (gridHeight));
			}
		}

		return gridPos;
	}

	void CreateGrid()
	{
		Vector2 gridSize = CalcGridSize();
		hexGridGo = new GameObject("HexGrid");

	
		for (float y = 0; y < gridSize.y; ++y)
		{
			for (float x = 0; x < gridSize.x; ++x)
			{
				GameObject hex = (GameObject)Instantiate(gridPrefab);
				hex.transform.tag = "DestroyMe";
				Vector2 gridPos = new Vector2(x, y);
				var cellInfo = hex.GetComponent<CellInfo>();
				cellInfo.gridPos = gridPos;

				hex.transform.position = CalcWorldCoord(gridPos);
				hex.transform.parent = hexGridGo.transform;

				Tile tile = new Tile(gridPos, cellInfo);

				cellInfo.tile = tile;

				map.Add(gridPos, cellInfo);
			}
		}
	}

	void DestroyExtraGrids()
	{
		colliderObjects = GameObject.FindGameObjectsWithTag("MapBounds");
		foreach (var o in colliderObjects)
		{
			var col = o.GetComponent<BoxCollider>();
			// getting all objects in colliders
			foreach (var grid in Physics.OverlapBox(col.bounds.center, Vector3.Scale(col.size, col.transform.localScale * 0.5f), col.transform.rotation))
			{
				//Remove DestroyMe tag
				if (grid.tag == "DestroyMe")
					grid.tag = "Grid";
			}

		}

		var grids = hexGridGo.transform.GetComponentsInChildren<Transform>();

		// Find all grids with destroyme to destroy
		//foreach (var grid in grids)
		//{
		//	if (grid.tag == "DestroyMe")
		//	{
		//		map.Remove(grid.GetComponent<CellInfo>().gridPos);
		//		Destroy(grid.gameObject);
		//	}
		//}
	}

	void FindObstacles()
	{
		colliderObjects = GameObject.FindGameObjectsWithTag("Obstacle");

		foreach (var o in colliderObjects)
		{
			var col = o.GetComponent<BoxCollider>();
			foreach (var grid in Physics.OverlapBox(col.bounds.center, Vector3.Scale(col.size, col.transform.localScale * 0.5f), col.transform.rotation))
			{
				if (grid.tag == "Grid")
				{
					CellInfo tile = null;
					map.TryGetValue(grid.transform.GetComponent<CellInfo>().gridPos, out tile);
					if (tile != null)
						tile.tile.Passable = false;
					int i = 1;
					i++;
				}
			}
		}
	}

	void DrawPath(IEnumerable<Tile> path)
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
			var line = Instantiate(linePrefab);
			Vector2 gridPos = new Vector2(tile.X, tile.Y);
			line.transform.position = CalcWorldCoord(gridPos);
			this.path.Add(line);
			line.transform.parent = lines.transform;
		}
	}

	public void GenerateAndShowPath()
	{
		if (originTile == null || destinationTile == null)
		{
			DrawPath(new List<Tile>());
			return;
		}

		var path = Path<Tile>.FindPath(originTile.tile, destinationTile.tile, Tile.distance, Tile.estimate);

		DrawPath(path);
	}

	Vector3 GetMouseToWorld()
	{
		var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

		RaycastHit hit;

		Physics.Raycast(ray, out hit);
		return hit.point;
	}

	void ShowNeightbours(Vector2 gridPos)
	{
		if (this.path == null)
			this.path = new List<GameObject>();

		this.path.ForEach(Destroy);
		this.path.Clear();

		GameObject lines = GameObject.Find("Lines");

		if (lines == null)
			lines = new GameObject("Lines");

		CellInfo tile;

		map.TryGetValue(gridPos, out tile);

		//Debug.Log(tile.Neighbours);
		foreach (var t in tile.tile.AllNeighbours)
		{
			var line = Instantiate(linePrefab);
			Vector2 pos = new Vector2(t.X, t.Y);
			line.transform.position = CalcWorldCoord(pos);
			this.path.Add(line);
		}
	}

	Vector2 CubeCoordToOddQ(Vector3 cube)
	{
		float col = cube.x;
		// using bitwise operator to find odd numbers
		float row = cube.z + (cube.x - ((int)cube.x & 1)) / 2;
		return new Vector2(col, row);
	}

	Vector3 ToCubeCoord(Vector2 gridPos)
	{
		float x = gridPos.x;
		float z = gridPos.y - (gridPos.x - ((int)gridPos.x & 1)) / 2;
		float y = -x - z;
		return new Vector3(x, y, z);
	}

	void Range(Vector2 gridPos, int range)
	{
		List<CellInfo> traversable = new List<CellInfo>();

		if(visted != null)
		{
			foreach(var cell in visted)
			{
				cell.rend.material.color = cell.original;
			}
		}

		visted = new HashSet<CellInfo>();

		CellInfo start;
		if (map.TryGetValue(gridPos, out start))
		{

			visted.Add(start);

			var fringe = new HashSet<CellInfo>();

			foreach (var n in start.tile.Neighbours)
			{
				fringe.Add(n.info);
			}

			while (range > 0)
			{
				var next = new HashSet<CellInfo>();

				foreach (var grid in fringe)
				{
					if (visted.Add(grid))
					{
						foreach (var n in grid.tile.Neighbours)
						{
							if (!visted.Contains(n.info))
								next.Add(n.info);
						}

					}
				}
				fringe = next;
				range--;
			}

			foreach (var tile in visted)
			{
				tile.rend.material.color = tile.green;
			}
		}
	}
}
