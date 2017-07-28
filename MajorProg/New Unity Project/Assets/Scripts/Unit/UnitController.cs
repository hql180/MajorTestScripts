using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum UnitType
{
	Player,
	Ally,
	Hostile
}

[RequireComponent(typeof(CharacterController))]
public class UnitController : MonoBehaviour
{
	public int movementRange = 5;

	public UnitType unitType;

	public CellInfo currentTile;
	public CellInfo destTile;

	public Path<Tile> path;

	LinkedList<CellInfo> path2 = new LinkedList<CellInfo>();

	public bool followPath = false;

	CharacterController controller;

	// Use this for initialization
	void Start()
	{
		currentTile = null;
		destTile = null;

		if (!currentTile)
		{
			currentTile = GridManager.instance.GetGridFromWorldPos(transform.position);
			Debug.Log("Setting current tile for " + transform.name.ToString());
		}

		var pos = currentTile.transform.position;

		pos.y = transform.position.y;

		transform.position = pos;
		path = null;

		followPath = false;

		controller = GetComponent<CharacterController>();
	}

	// Update is called once per frame
	void Update()
	{
		if (currentTile.unit != this)
			currentTile.unit = this;

		if(followPath)
		{
			FollowPath();
		}

	}

	public void GenerateAndShowPath()
	{
		if (currentTile == null || destTile == null)
		{
			GridManager.instance.DrawPath(new List<Tile>());
			return;
		}

		var path = Path<Tile>.FindPath(currentTile.tile, destTile.tile, Tile.distance, Tile.estimate);

		GridManager.instance.DrawPath(path);
	}

	public void ShowAvailableMoves()
	{
		GridManager.instance.Range(currentTile, movementRange);
	}

	void ShowPath()
	{
		if(path != null)
			foreach (var node in path)
			{
				node.info.SetYellow();
			}
	}

	void GeneratePath()
	{
		if (path != null)
		{
			foreach (var node in path)
			{
				node.info.SetDefault();
			}
		}

		if (currentTile && destTile)
		{
			var newPath = Path<Tile>.FindPath(currentTile.tile, destTile.tile, Tile.distance, Tile.estimate);

			if (newPath != null && newPath.TotalCost <= movementRange)
				path = newPath;
		}
	}

	public void UpdateDestination(CellInfo grid)
	{
		if(grid && !followPath)
		{
			if (grid.empty)
				destTile = grid;

			GeneratePath();
			ShowPath();			
		}
	}

	public void UnitDeselected()
	{
		path = null;
		destTile = null;
	}

	public void FollowPath()
	{
		if (path2.Count == 0 && path != null)
		{
			path2.Clear();
			foreach(var tile in path)
			{
				path2.AddFirst(tile.info);
			}

			path = null;
		}

		if (path2.Count > 0)
		{
			Vector3 pos = transform.position;
			pos.y = 0;
			var dir = path2.First.Value.transform.position - pos;


			dir.Normalize();
			controller.Move(dir * 5 * Time.deltaTime);

			if (Vector3.Distance(transform.position, path2.First.Value.transform.position) < transform.position.y + 0.05f)
			{
				if(path2.Count == 1)
				{
					followPath = false;
					currentTile.unit = null;
					currentTile = path2.First.Value;
					currentTile.unit = this;
					var move = currentTile.transform.position;
					move.y = transform.position.y;
					transform.position = move;
				}
				path2.RemoveFirst();
			}
		}
		
	}
}