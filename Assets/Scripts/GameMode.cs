using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Analytics;


public class GameMode : MonoBehaviour {

	public static GameMode singleton = null;
	
	public GameObject endOfLevelPanelGO;
	public GameObject canvasGO;
	
	public bool isEditingCircuit = false;
	public bool isEndOfLevel = true;
	public string nextLevelName;
	
	public static float gameStartTime;

	// Use this for initialization
	void Start () {
//		Debug.Log ("startLevel - levelName: " + Application.loadedLevelName + ", gameTime: " + (Time.time - gameStartTime));
		
		Analytics.CustomEvent("startLevel", new Dictionary<string, object>
		{
			{ "levelName", Application.loadedLevelName },
			{ "gameTime", (Time.time - GameMode.gameStartTime)},
		});	
		
	}
	
	// Update is called once per frame
	void Update () {
	
		canvasGO.SetActive(true);
		
		endOfLevelPanelGO.SetActive(isEndOfLevel);
		if (isEndOfLevel){
			if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space)){
			 	Application.LoadLevel(nextLevelName);
			}
		}
		
		if (Input.GetKeyDown(KeyCode.R)){
//			Debug.Log ("restartLevel - levelName: " + Application.loadedLevelName + "levelTime: " + Time.timeSinceLevelLoad + ", gameTime: " + (Time.time - GameMode.gameStartTime));
			
			Analytics.CustomEvent("restartLevel", new Dictionary<string, object>
			{
				{ "levelName", Application.loadedLevelName },
				{ "levelTime", Time.timeSinceLevelLoad },
				{ "gameTime", (Time.time - GameMode.gameStartTime)},
			});				
			
			Application.LoadLevel (Application.loadedLevelName);
		}
		
		Cursor.visible = isEditingCircuit;
	
	}
	
	//----------------------------------------------
	void Awake(){
		if (singleton != null) Debug.LogError ("Error assigning singleton");
		singleton = this;
	}
	
	
	
	void OnDestroy(){
		singleton = null;
	}
}
