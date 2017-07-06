using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{	
	public static GridManager instance = null;

	public GameObject Line;
	//List to hold "Lines" indicating the path
	List<GameObject> path;

	public GameObject hexCellPrefab;

	public GameObject groundBounds;

	public GameObject[] colliders;

	float hexWidth;
	float hexHeight;
	float groundWidth;
	float groundHeight;

	public GameObject hexGridGo;

	void SetSizes()
	{
		var hexRend = hexCellPrefab.GetComponent<Renderer>();
		var groundRend = groundBounds.GetComponent<Renderer>();

		hexWidth = hexRend.bounds.size.x;
		hexHeight = hexRend.bounds.size.z;

		groundWidth = groundRend.bounds.size.x;
		groundHeight = groundRend.bounds.size.z;
	}

	//http://www.redblobgames.com/grids/hexagons/#map-storage

	//The method used to calculate the number hexagons in a row and number of rows
	//Vector2.x is gridWidthInHexes and Vector2.y is gridHeightInHexes
	Vector2 CalcGridSize()
	{
		//According to the math textbook hexagon's side length is half of the height
		float width = hexWidth * 2;
		// calculating x spacing between hex cells
		float widthSpacing = width * 3 / 4;
		//the number of whole hex sides that fit inside inside ground height
		int numOfCol = (int)(groundWidth / width);

		int height = (int)(Mathf.Sqrt(3) / 2 * width);
		//When the number of hexes is even the tip of the last hex in the offset column might stick up.
		//The number of hexes in that case is reduced.
		if (gridHeightInHexes % 2 == 0
			&& (numOfCol + 0.5f) * sideLength > groundHeight)
			gridHeightInHexes--;
		//gridWidth in hexes is calculated by simply dividing ground width by hex width
		return new Vector2((int)(groundWidth / hexWidth), gridHeightInHexes);

		////According to the math textbook hexagon's side length is half of the height
		//float sideLength = hexHeight / 2;
		////the number of whole hex sides that fit inside inside ground height
		//int nrOfSides = (int)(groundHeight / sideLength);
		////I will not try to explain the following calculation because I made some assumptions, which might not be correct in all cases, to come up with the formula. So you'll have to trust me or figure it out yourselves.
		//int gridHeightInHexes = (int)(nrOfSides * 2 / 3);
		////When the number of hexes is even the tip of the last hex in the offset column might stick up.
		////The number of hexes in that case is reduced.
		//if (gridHeightInHexes % 2 == 0
		//	&& (nrOfSides + 0.5f) * sideLength > groundHeight)
		//	gridHeightInHexes--;
		////gridWidth in hexes is calculated by simply dividing ground width by hex width
		//return new Vector2((int)(groundWidth / hexWidth), gridHeightInHexes);
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
		if (gridPos.y % 2 != 0)
		{
			offset = hexWidth / 2;
		}

		float x = initPos.x + offset + gridPos.x * hexWidth;
		float z = initPos.z - gridPos.y * hexHeight * 0.75f;

		return new Vector3(x, 0, z);
	}

	void CreateGrid()
	{
		Vector2 gridSize = CalcGridSize();
		hexGridGo = new GameObject("HexGrid");

		for (float y = 0; y < gridSize.y; ++y)
		{
			float sizeX = gridSize.x;

			if (y % 2 != 0 && (gridSize.x + 0.5) * hexWidth > groundWidth)
			{
				sizeX--;
			}

			for (float x = 0; x < sizeX; ++x)
			{
				GameObject hex = (GameObject)Instantiate(hexCellPrefab);
				hex.transform.tag = "DestroyMe";
				Vector2 gridPos = new Vector2(x, y);
				hex.GetComponent<CellInfo>().gridPos = gridPos;
	
				hex.transform.position = CalcWorldCoord(gridPos);
				hex.transform.parent = hexGridGo.transform;
			}
		}

		//Vector2 gridSize = CalcGridSize();
		//hexGridGo = new GameObject("HexGrid");
		//board is used to store tile locations
		//Dictionary<Point, Tile> board = new Dictionary<Point, Tile>();




		//for (float y = 0; y < gridSize.y; y++)
		//{
		//	float sizeX = gridSize.x;
		//	//if the offset row sticks up, reduce the number of hexes in a row
		//	if (y % 2 != 0 && (gridSize.x + 0.5) * hexWidth > groundWidth)
		//		sizeX--;
		//	for (float x = 0; x < sizeX; x++)
		//	{
		//		GameObject hex = (GameObject)Instantiate(hexGrid);
		//		Vector2 gridPos = new Vector2(x, y);
		//		hex.transform.position = CalcWorldCoord(gridPos);
		//		hex.transform.parent = hexGridGo.transform;
		//		hex.transform.tag = "DestroyMe";
		//		var tb = (TileBehaviour)hex.GetComponent("TileBehaviour");
		//		//y / 2 is subtracted from x because we are using straight axis coordinate system
		//		tb.tile = new Tile((int)x - (int)(y / 2), (int)y);
		//		//board.Add(tb.tile.Location, tb.tile);
		//		Board.Add(tb.tile.Location, tb.tile);
		//	}
		//}
		////variable to indicate if all rows have the same number of hexes in them
		////this is checked by comparing width of the first hex row plus half of the hexWidth with groundWidth
		//bool equalLineLengths = (gridSize.x + 0.5) * hexWidth <= groundWidth;
		////Neighboring tile coordinates of all the tiles are calculated
		//foreach (Tile tile in Board.Values)
		//	tile.FindNeighbours(Board, gridSize, equalLineLengths);
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

		foreach (var grid in grids)
		{
			if (grid.tag == "DestroyMe")
			{
				//var pos = CalcGridPos(grid.transform.position);

				//Board.Remove(new Point((int)pos.x, (int)pos.y));

				Destroy(grid.gameObject);
			}
		}
	}


	// Use this for initialization
	void Start()
	{
		instance = this;
		SetSizes();
		CreateGrid();
	}

	// Update is called once per frame
	void Update()
	{
		if (Time.realtimeSinceStartup > 2)
		{
			DestroyExtraGrids();
		}
	}
}
