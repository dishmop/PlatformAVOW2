using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Analytics;

public class SplashScreen : MonoBehaviour {

	public void StartGame(){
//		Debug.Log ("startGame");
		Analytics.CustomEvent("startGame", new Dictionary<string, object>
		                      {
			{ "dummy", 0 },
		});			
		GameMode.gameStartTime = Time.time;
		Application.LoadLevel("Level1");
	}

	// Use this for initialization
	void Start () {
		Cursor.visible = true;
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
