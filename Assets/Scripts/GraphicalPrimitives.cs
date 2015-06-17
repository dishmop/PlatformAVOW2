using UnityEngine;
using System.Collections;

public class GraphicalPrimitives : MonoBehaviour {

	public static GraphicalPrimitives singleton = null;
	
	public GameObject wireLinePrefab;
	public GameObject wireRectPrefab;
	


	
	
	//----------------------------------------------
	void Awake(){
		if (singleton != null) Debug.LogError ("Error assigning singleton");
		singleton = this;
	}
	
	
	
	void OnDestroy(){
		singleton = null;
	}
}
