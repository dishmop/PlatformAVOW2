using UnityEngine;
using System.Collections;
using UnityEngine.UI;

//using System.Collections.Generic;
//using UnityEngine.Analytics;

public class MainPage : MonoBehaviour {

	public int resumeLevelNumber;
	public GameObject resumeButtonGO;
	

	public void StartGame(){
//		Debug.Log ("startGame");
		GoogleAnalytics.Client.SendEventHit("gameFlow", "startGame");
		GoogleAnalytics.Client.SendScreenHit("startGame");		

		PlayerPrefs.DeleteKey("ResumeLevelNumber");
		PlayerPrefs.DeleteKey("ResumeLevelTime");

		GameMode.gameStartTime = Time.time;
		Application.LoadLevel(1);
	}
	
	public void ResumeGame(){
		//		Debug.Log ("startGame");
		GoogleAnalytics.Client.SendEventHit("gameFlow", "resumeGame");
		GoogleAnalytics.Client.SendScreenHit("resumeGame");		
		float gameLen = PlayerPrefs.GetFloat("ResumeLevelTime");
		
		GameMode.gameStartTime = Time.time - gameLen;
		Application.LoadLevel(resumeLevelNumber);

	}
	
	public void SelectLevel(int levelNum){
		if (levelNum > 0 && levelNum < Application.levelCount - 2){
			GoogleAnalytics.Client.SendEventHit("gameFlow", "selectLevel", levelNum.ToString ());
			GoogleAnalytics.Client.SendScreenHit("selectLevel");		
			GameMode.gameStartTime = Time.time;
			
			Application.LoadLevel(levelNum);
			
		}
		
	}

	// Use this for initialization
	void Start () {
		Cursor.visible = true;
		
		if (PlayerPrefs.HasKey("ResumeLevelNumber")){
			resumeLevelNumber = PlayerPrefs.GetInt("ResumeLevelNumber");
		}
		else{
			resumeLevelNumber = 0;
		}
	
	}
	
	// Update is called once per frame
	void Update () {
	
		resumeButtonGO.GetComponent<Button>().interactable = (resumeLevelNumber != 0);
	
	}
}
