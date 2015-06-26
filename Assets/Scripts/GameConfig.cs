using UnityEngine;
using System.Collections;

public class GameConfig : MonoBehaviour {
	public static GameConfig singleton = null;
	public Color highVolt = new Color(127f/255f, 10f/255f, 0f/255f, 1);
	public Color lowVolt = new Color(0f/255f, 10f/255f, 127f/255f, 1);
	public Color interactionNormal = new Color(0, 0.75f, 0, 1);
	public Color interactionReady = new Color(0.25f, 1, 0.25f, 1);
	
	public Color indicatorOK = new Color(0, 1, 0, 1);
	public Color indicatorSemi = new Color(1, 1, 0, 1);
	public Color indicatorError = new Color(1, 0, 0, 1);
	public Color indicatorUnpowered = new Color(0, 0, 0, 1);
	
	public float routingFirstStepDist = 0.35f;
	public bool showDebug = false;
	
	void Awake(){
		if (singleton != null) Debug.LogError ("Error assigning singleton");
		singleton = this;
	}
	
	
	
	void OnDestroy(){
		singleton = null;
	}
}
