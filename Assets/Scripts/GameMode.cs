using UnityEngine;
using System.Collections;
using UnityEngine.UI;
//using System.Collections.Generic;
//using UnityEngine.Analytics;


public class GameMode : MonoBehaviour {

	public static GameMode singleton = null;
	
	public GameObject endOfLevelPanelGO;
	public GameObject canvasGO;
	
	public bool isEditingCircuit = false;
	public bool isEndOfLevel = true;
	bool wasEndOfLevel = true;
	
	float endOfLevelTime = 0;
	float endOfLevelFadeDuration = 1;
	
	public static float gameStartTime;

	// Use this for initialization
	void Start () {
//		Debug.Log ("startLevel - levelName: " + Application.loadedLevelName + ", gameTime: " + (Time.time - gameStartTime));
		GoogleAnalytics.Client.SendEventHit("gameFlow", "startLevel", Application.loadedLevelName);
		
//		Analytics.CustomEvent("startLevel", new Dictionary<string, object>
//		{
//			{ "levelName", Application.loadedLevelName },
//			{ "gameTime", (Time.time - GameMode.gameStartTime)},
//		});	
		
	}
	
	// Update is called once per frame
	void Update () {
	
		canvasGO.SetActive(true);
		
		if (!wasEndOfLevel && isEndOfLevel){
			endOfLevelTime = Time.time;
		}
		
		if (isEndOfLevel && endOfLevelPanelGO){
			endOfLevelPanelGO.SetActive(true);
			float fadeVal = (Time.time - endOfLevelTime) / endOfLevelFadeDuration;
			endOfLevelPanelGO.GetComponent<Image>().color = Color.Lerp(new Color(0, 0, 0, 0), new Color(0, 0, 0, 1), fadeVal);
			endOfLevelPanelGO.GetComponentInChildren<Text>().text = "Level Complete";
			
			if (fadeVal > 1){
				Application.LoadLevel(Application.loadedLevel + 1);
			}
			
		}
		else if (Time.timeSinceLevelLoad < endOfLevelFadeDuration){
			endOfLevelPanelGO.SetActive(true);
			float fadeVal = Time.timeSinceLevelLoad / endOfLevelFadeDuration;
			endOfLevelPanelGO.GetComponent<Image>().color = Color.Lerp(new Color(0, 0, 0, 1), new Color(0, 0, 0, 0), fadeVal);
			endOfLevelPanelGO.GetComponentInChildren<Text>().text = "";
		}
		else{
			endOfLevelPanelGO.SetActive(false);
		}
		
		
		wasEndOfLevel = isEndOfLevel;
		
		if (Input.GetKeyDown(KeyCode.L) && Time.timeScale != 0){
//			Debug.Log ("restartLevel - levelName: " + Application.loadedLevelName + "levelTime: " + Time.timeSinceLevelLoad + ", gameTime: " + (Time.time - GameMode.gameStartTime));
			GoogleAnalytics.Client.SendTimedEventHit("gameFlow", "restartLevel", Application.loadedLevelName, Time.timeSinceLevelLoad);
//						
//			Analytics.CustomEvent("restartLevel", new Dictionary<string, object>
//			{
//				{ "levelName", Application.loadedLevelName },
//				{ "levelTime", Time.timeSinceLevelLoad },
//				{ "gameTime", (Time.time - GameMode.gameStartTime)},
//			});				
			
			Application.LoadLevel (Application.loadedLevelName);
		}
		
		if (Time.timeScale != 0) Cursor.visible = isEditingCircuit;
	
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
