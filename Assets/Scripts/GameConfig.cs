using UnityEngine;
using System.Collections;

public class GameConfig : MonoBehaviour {
	public static GameConfig singleton = null;

	public float routingFirstStepDist = 1f;
	

	
	
	void Awake(){
		if (singleton != null) Debug.LogError ("Error assigning singleton");
		singleton = this;
	}
	
	
	
	void OnDestroy(){
		singleton = null;
	}
}
