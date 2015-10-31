using UnityEngine;
using System.Collections;
//using System.Collections.Generic;
//using UnityEngine.Analytics;

public class EndOfGame : MonoBehaviour {

	public void RestartGame(){
		Application.LoadLevel("Menu");
	}

	// Use this for initialization
	void Start () {
		Cursor.visible = true;
//		Debug.Log ("gameComplete - gameTime: " + (Time.time - GameMode.gameStartTime));
		GoogleAnalytics.Client.SendTimedEventHit("gameFlow", "gameComplete", "", (Time.time - GameMode.gameStartTime));
//		Analytics.CustomEvent("gameComplete", new Dictionary<string, object>{ { "gameTime", (Time.time - GameMode.gameStartTime) }	});		
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
