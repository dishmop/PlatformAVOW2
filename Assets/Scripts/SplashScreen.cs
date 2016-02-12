using UnityEngine;
using System.Collections;
using UnityEngine.UI;

//using System.Collections.Generic;
//using UnityEngine.Analytics;

public class SplashScreen : MonoBehaviour {

	public int resumeLevelNumber;
	public GameObject mainPageGO;
	public GameObject selectLevelGO;
	public GameObject creditsGO;
	


	// Use this for initialization
	void Start () {
		Cursor.visible = true;
		ActivateMainMenu();
	}
	
	public void ActivateMainMenu(){
		mainPageGO.SetActive(true);
		selectLevelGO.SetActive(false);
		creditsGO.SetActive(false);
	}
	
	public void ActivateLevelSelect(){
		mainPageGO.SetActive(false);
		selectLevelGO.SetActive(true);
		creditsGO.SetActive(false);
	}
	
	public void ActivateCredits(){
		mainPageGO.SetActive(false);
		selectLevelGO.SetActive(false);
		creditsGO.SetActive(true);
	}

}
