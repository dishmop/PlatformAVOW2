using UnityEngine;
using System.Collections;

public class GameConfig : MonoBehaviour {
	public static GameConfig singleton = null;
	public Color highVolt = new Color(127f/255f, 10f/255f, 0f/255f, 1);
	public Color lowVolt = new Color(0f/255f, 10f/255f, 127f/255f, 1);
	
	public float routingFirstStepDist = 0.25f;
	public bool showDebug = false;
	
	void Awake(){
		if (singleton != null) Debug.LogError ("Error assigning singleton");
		singleton = this;
	}
	
	
	
	void OnDestroy(){
		singleton = null;
	}
}
