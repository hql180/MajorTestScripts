using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellInfo : MonoBehaviour
{
	public Vector2 gridPos;
	public Tile tile;

	public UnitController unit;

	public bool empty
	{
		get
		{
			return ((unit == null)); //|| (unit != null && unit.unitType == UnitType.Player));
		}
	}

	public Color original;
	public Color red;
	public Color green;
	public Color yellow;

	public Renderer rend;

	// Use this for initialization
	void Start ()
	{
		rend = GetComponent<Renderer>();
		original = rend.material.color;
		unit = null;
	}
	
	// Update is called once per frame
	void Update ()
	{
		if(tile != null && !tile.Passable)
		{
			SetRed();
		}
	}

	public void SetYellow()
	{
		rend.material.color = yellow;
	}

	public void SetDefault()
	{
		rend.material.color = original;
	}

	public void SetGreen()
	{
		rend.material.color = green;
	}

	public void SetRed()
	{
		rend.material.color = red;
	}

	public bool Passable(UnitController mover)
	{
		if (mover == null || unit == null)
			return true;

		if(mover.unitType == UnitType.Ally || mover.unitType == UnitType.Player)
		{
			if (unit.unitType == UnitType.Hostile)
				return false;

			return true;
		}

		if (mover.unitType == UnitType.Hostile)
		{
			if (unit.unitType == UnitType.Ally || unit.unitType == UnitType.Player)
				return false;			

			return true;
		}

		Debug.Log("something went wrong... cellinfo.Passable");
		return false;
	}
}
