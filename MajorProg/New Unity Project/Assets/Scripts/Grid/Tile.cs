using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;

public class Tile : IHasNeighbours<Tile>
{
	public Tile(Vector2 point)
	{
		location = point;

		Passable = true;
	}
	

	public bool Passable
	{
		get
		{
			return Passable;
		}
		set
		{
			Passable = (Empty && value);
		}
	}

	public int X { get { return (int)location.x; } }

	public int Y { get { return (int)location.y; } }


	public bool Empty;


	public IEnumerable<Tile> AllNeighbours { get; set; }
	
	public IEnumerable<Tile> Neighbours { get { return AllNeighbours.Where( o => (o.Passable)); } }

	Vector2 location;

	public static List<Vector2> ValidNeighbours
	{
		get
		{
			return new List<Vector2>
			{
				new Vector2(0, 1),
				new Vector2(0, -1),
				new Vector2(1, 0),
				new Vector2(-1, 0)
			};
		}
	}

	public void FindNeighbours(Dictionary<Vector2, Tile> Board)
	{
		List<Tile> neighbours = new List<Tile>();

		foreach(var point in ValidNeighbours)
		{
			Vector2 neighbour = location + point;
			if(Board.ContainsKey(neighbour))
			{
				neighbours.Add(Board[neighbour]);
			}
		}

		AllNeighbours = neighbours;
	}

	static double distance(Tile tile1, Tile tile2)
	{
		// placeholder might need to change
		return 1;
	}

	static double estimate(Tile tile, Tile destTile)
	{
		float distanceX = Mathf.Abs(destTile.X - tile.X);
		float distanceY = Mathf.Abs(destTile.Y - tile.Y);
	}
}
