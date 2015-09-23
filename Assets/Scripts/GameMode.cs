using UnityEngine;
using System.Collections;

public class GameMode : MonoBehaviour {

	public static GameMode singleton = null;
	
	public GameObject endOfLevelPanelGO;
	public GameObject canvasGO;
	
	public bool isEditingCircuit = false;
	public bool isEndOfLevel = true;
	public string nextLevelName;
	


	// Use this for initialization
	void Start () {

		
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
