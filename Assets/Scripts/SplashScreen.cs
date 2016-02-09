using UnityEngine;
using System.Collections;
//using System.Collections.Generic;
//using UnityEngine.Analytics;

public class SplashScreen : MonoBehaviour {

	public string firstLevelName;
	

	public void StartGame(){
//		Debug.Log ("startGame");
		GoogleAnalytics.Client.SendEventHit("gameFlow", "startGame");
		GoogleAnalytics.Client.SendScreenHit("startGame");		
//		
//		Analytics.CustomEvent("startGame", new Dictionary<string, object>
//		                      {
//			{ "dummy", 0 },
//		});			
		GameMode.gameStartTime = Time.time;
		Application.LoadLevel(firstLevelName);
	}

	// Use this for initialization
	void Start () {
		Cursor.visible = true;
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
