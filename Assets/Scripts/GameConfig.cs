using UnityEngine;
using System.Collections;

public class GameConfig : MonoBehaviour {
	public static GameConfig singleton = null;
	public Color attachedColor = new Color(1f/255f, 2f/255f, 2f/255f, 1);
	public Color selectedColor = new Color(1f/255f, 1f/255f, 2f/255f, 1);
	
	public float routingFirstStepDist = 1f;
	
	void Awake(){
		if (singleton != null) Debug.LogError ("Error assigning singleton");
		singleton = this;
	}
	
	
	
	void OnDestroy(){
		singleton = null;
	}
}
