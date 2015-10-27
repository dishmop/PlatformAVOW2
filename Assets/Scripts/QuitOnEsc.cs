using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Analytics;

public class QuitOnEsc : MonoBehaviour {

	public string OnQuitLevelName;
	
	
	
	// Update is called once per frame
	void Update () {

		
		// Test for exit
		if (UnityEngine.Input.GetKeyDown (KeyCode.Escape)) {
			if (OnQuitLevelName != null && OnQuitLevelName != ""){
//				Debug.Log ("quitLevel - levelName: " + Application.loadedLevelName + ", gameTime: " + (Time.time - GameMode.gameStartTime));
				
				Analytics.CustomEvent("quitLevel", new Dictionary<string, object>
				{
					{ "levelName", Application.loadedLevelName },
					{ "levelTime", Time.timeSinceLevelLoad },
					{ "gameTime", (Time.time - GameMode.gameStartTime)},
				});	
								
				Application.LoadLevel(OnQuitLevelName);
				
			}
			else{
				Quit();
			}
		}
	}
	
	public static string webplayerQuitURL = "http://i-want-to-study-engineering.org/game/rating/wired/";
	
	public static void Quit()
	{
		#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false;
		#elif UNITY_WEBPLAYER
		Application.OpenURL(webplayerQuitURL);
		#else
		Application.OpenURL(webplayerQuitURL);
		Application.Quit();
		#endif
	}
}
