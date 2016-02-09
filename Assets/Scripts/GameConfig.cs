using UnityEngine;
using System.Collections;

public class GameConfig : MonoBehaviour {
	public static GameConfig singleton = null;
	public Color highVolt = new Color(127f/255f, 10f/255f, 0f/255f, 1);
	public Color lowVolt = new Color(0f/255f, 10f/255f, 127f/255f, 1);
	public Color interactionNormal = new Color(0, 0.75f, 0, 1);
	public Color interactionReady = new Color(0.25f, 1, 0.25f, 1);
	
	public Color squareInsideCol1 = new Color(0f, 0f, 0f, 1);
	public Color squareInsideCol2 = new Color(0f, 0f, 0f, 1);
	
	public float routingFirstStepDist = 0.35f;
	public bool showDebug = false;
	int randCount;
	
	public Color GetNextSquareCol(){
		Random.seed = randCount++;
		float valR = Random.Range(0f, 1f);
		float valG = Random.Range(0f, 1f);
		float valB = Random.Range(0f, 1f);
		float invSum = 2f/(valR + valG + valB);
		valR *= invSum;
		valG *= invSum;
		valB *= invSum;
		
	
		return new Color(valR, valG, valB, 1);
		
	}
	
	void Awake(){
		if (singleton != null) Debug.LogError ("Error assigning singleton");
		singleton = this;
		randCount = 10;
		
	}
	
	
	
	void OnDestroy(){
		singleton = null;
	}
}
