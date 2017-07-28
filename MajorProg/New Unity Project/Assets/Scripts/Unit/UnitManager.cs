using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitManager : MonoBehaviour
{
	public static UnitManager instance;

	public UnitController selectedUnit;

	public CellInfo currentTile;

	// Use this for initialization
	void Start ()
	{
		if (instance == null)
			instance = this;

		selectedUnit = null;
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (selectedUnit)
		{
			selectedUnit.ShowAvailableMoves();

			var grid = GridManager.instance.GetGridAtMouse();
			if (grid)
				selectedUnit.UpdateDestination(grid);

			if (Input.GetMouseButtonDown(0))
			{
				selectedUnit.followPath = true;
			}


			if (Input.GetKeyDown(KeyCode.Escape))
			{
				selectedUnit = null;

				GridManager.instance.ClearSelections();
			}
		}

		if (selectedUnit == null)
		{
			if (Input.GetMouseButtonDown(0))
			{
				SelectTile();
			}
		}
	}

	void SelectTile()
	{
		if (currentTile)
			currentTile.SetDefault();

		currentTile = GridManager.instance.GetGridAtMouse();

		if(currentTile)
		{
			currentTile.SetGreen();
			if(currentTile.unit)
			{
				selectedUnit = currentTile.unit;
			}
		}
	}
}
