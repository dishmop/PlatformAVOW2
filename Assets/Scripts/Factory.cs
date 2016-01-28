﻿using UnityEngine;
using System.Collections;

public class Factory : MonoBehaviour {

	public static Factory singleton = null;
	
	public GameObject wireLinePrefab;
//	public GameObject wireRectPrefab;
	public GameObject wirePrefab;
	public GameObject wireJunctionPrefab;
	public GameObject socketPrefab;
	public GameObject socketTPrefab;
	public GameObject emptyConnectorPrefab;
	public GameObject socketEndPrefab;
	public GameObject socketTPrefabInert;


	
	
	//----------------------------------------------
	void Awake(){
		if (singleton != null) Debug.LogError ("Error assigning singleton");
		singleton = this;
	}
	
	
	
	void OnDestroy(){
		singleton = null;
	}
}
