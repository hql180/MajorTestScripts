using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellInfo : MonoBehaviour
{
	public Vector2 gridPos;
	public Tile tile;
	public Vector2 tilePos;

	// Use this for initialization
	void Start ()
	{
		
	}
	
	// Update is called once per frame
	void Update ()
	{
		tilePos.x = tile.X;
		tilePos.y = tile.Y;
	}
}
