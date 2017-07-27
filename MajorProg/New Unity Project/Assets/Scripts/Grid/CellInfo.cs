using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellInfo : MonoBehaviour
{
	public Vector2 gridPos;
	public Tile tile;

	public Color original;
	public Color red;
	public Color green;

	public Renderer rend;

	// Use this for initialization
	void Start ()
	{
		rend = GetComponent<Renderer>();
		original = rend.material.color;
	}
	
	// Update is called once per frame
	void Update ()
	{
		if(!tile.Passable)
		{
			rend.material.color = red;
		}
	}
}
