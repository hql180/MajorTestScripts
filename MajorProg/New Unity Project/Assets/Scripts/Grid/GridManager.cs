using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{	
	public static GridManager instance = null;

	public GameObject Line;
	//List to hold "Lines" indicating the path
	List<GameObject> path;

	public GameObject gridPrefab;

	public GameObject groundBounds;

	public GameObject[] colliderObjects;

	float gridWidth;
	float gridHeight;
	float groundWidth;
	float groundHeight;

	public GameObject hexGridGo;

	void SetSizes()
	{
		var hexRend = gridPrefab.GetComponent<Renderer>();
		var groundRend = groundBounds.GetComponent<Renderer>();

		gridWidth = hexRend.bounds.size.x;
		gridHeight = hexRend.bounds.size.z;

		groundWidth = groundRend.bounds.size.x;
		groundHeight = groundRend.bounds.size.z;
	}

	Vector2 CalcGridSize()
	{
		return new Vector2((int)(groundWidth /gridWidth), (int)(groundHeight / gridHeight));
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
		
		float x = initPos.x + gridPos.x * gridWidth;
		float z = initPos.z - gridPos.y * gridHeight;

		return new Vector3(x, 0, z);
	}

	public Vector2 CalcGridPos(Vector3 coord)
	{
		Vector3 initPos = CalcInitPos();
		Vector2 gridPos = new Vector2();
		gridPos.y = Mathf.RoundToInt((initPos.z - coord.z) / (gridHeight));

		gridPos.x = Mathf.RoundToInt((coord.x - initPos.x) / gridWidth);
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
				hex.GetComponent<CellInfo>().gridPos = gridPos;
	
				hex.transform.position = CalcWorldCoord(gridPos);
				hex.transform.parent = hexGridGo.transform;
			}
		}
	}



	void DestroyExtraGrids()
	{
		colliderObjects = GameObject.FindGameObjectsWithTag("MapBounds");
		foreach(var o in colliderObjects)
		{
			var col = o.GetComponent<BoxCollider>();
			foreach(var grid in Physics.OverlapBox(col.bounds.center, Vector3.Scale(col.size, col.transform.localScale * 0.5f), col.transform.rotation))
			{
				grid.tag = "Untagged";
			}

		}

		var grids = hexGridGo.transform.GetComponentsInChildren<Transform>();

		foreach (var grid in grids)
		{
			if (grid.tag == "DestroyMe")
			{
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
		DestroyExtraGrids();
	}

	// Update is called once per frame
	void Update()
	{
	}
}
