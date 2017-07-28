using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;

public class Tile : IHasNeighbours<Tile>
{
	public Tile(Vector2 point, CellInfo info)
	{
		location = point;
		Passable = true;
		this.info = info;
	}

	public bool Passable
	{
		get;
		set;
	}

	Vector2 location;

	public CellInfo info;

	public bool Empty = true;

	public int X { get { return (int)location.x; } }

	public int Y { get { return (int)location.y; } }
	
	public IEnumerable<Tile> AllNeighbours { get; set; }
	
	public IEnumerable<Tile> Neighbours
	{
		get
		{
			return AllNeighbours.Where(o => (o.Passable && o.info.Passable(UnitManager.instance.selectedUnit)));
		}
	}

	public List<Vector2> ValidNeighbours
	{
		get
		{
			if (GridManager.instance.gridType == GridType.Cube)
			{
				return new List<Vector2>
				{
					new Vector2(0, 1),
					new Vector2(0, -1),
					new Vector2(1, 0),
					new Vector2(-1, 0)
				};
			}
			else if (GridManager.instance.gridType == GridType.Hex)
			{				
				// neighbour coord offset due to grid shift upwards on odd x grids
				if(location.x % 2 == 0)
				return new List<Vector2>
				{
					new Vector2(0, 1),
					new Vector2(0, -1),
					new Vector2(1, 0),
					new Vector2(-1, 0),
					
					new Vector2(-1, -1),
					new Vector2(1, -1)
				};
				else
					return new List<Vector2>
				{
					new Vector2(0, 1),
					new Vector2(0, -1),
					new Vector2(1, 0),
					new Vector2(-1, 0),

					new Vector2(1, 1),
					new Vector2(-1, 1)
				};
			}
			return null;
		}
	}

	public void FindNeighbours(Dictionary<Vector2, CellInfo> map)
	{
		List<Tile> neighbours = new List<Tile>();

		foreach(var point in ValidNeighbours)
		{
			Vector2 neighbour = location + point;
			if(map.ContainsKey(neighbour))
			{
				neighbours.Add(map[neighbour].tile);
			}
		}

		AllNeighbours = neighbours;
	}

	static public double distance(Tile tile1, Tile tile2)
	{
		// placeholder might need to change
		return 1;
	}

	static public double estimate(Tile tile, Tile destTile)
	{
		float distanceX = Mathf.Abs(destTile.X - tile.X);
		float distanceY = Mathf.Abs(destTile.Y - tile.Y);
		int z1 = -(tile.X + tile.Y);
		int z2 = -(destTile.X + destTile.Y);
		float distanceZ = Mathf.Abs(z2 - z1);

		return Mathf.Max(distanceX, distanceY, distanceZ);
	}
}
