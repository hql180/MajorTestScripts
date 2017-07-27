using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapBounds : MonoBehaviour
{

	// Use this for initialization
	void Start ()
	{
		transform.tag = "MapBounds";
		gameObject.GetComponent<MeshRenderer>().enabled = false;
	}
	
	// Update is called once per frame
	void Update ()
	{
		
	}
}
