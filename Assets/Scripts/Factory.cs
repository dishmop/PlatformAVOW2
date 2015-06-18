using UnityEngine;
using System.Collections;

public class Factory : MonoBehaviour {

	public static Factory singleton = null;
	
	public GameObject wireLinePrefab;
	public GameObject wireRectPrefab;
	public GameObject wire;
	


	
	
	//----------------------------------------------
	void Awake(){
		if (singleton != null) Debug.LogError ("Error assigning singleton");
		singleton = this;
	}
	
	
	
	void OnDestroy(){
		singleton = null;
	}
}
