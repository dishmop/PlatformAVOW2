using UnityEngine;
using System.Collections;

public class GameMode : MonoBehaviour {

	public static GameMode singleton = null;
	
	public GameObject endOfLevelPanelGO;
	public GameObject canvasGO;
	
	public bool isEditingCircuit = false;
	public bool isEndOfLevel = true;
	public string nextLevelName;
	
	bool tempDebugFlag = true;

	// Use this for initialization
	void Start () {

		
	}
	
	// Update is called once per frame
	void Update () {
	
		canvasGO.SetActive(true);
		
		endOfLevelPanelGO.SetActive(isEndOfLevel);
		if (isEndOfLevel){
			if (Input.GetMouseButtonDown(0)){
			 	Application.LoadLevel(nextLevelName);
			}
		}
	
		if (isEditingCircuit && !tempDebugFlag){
			tempDebugFlag = true;
			Circuit.singleton.AddConnection(Circuit.singleton.GetElectricalComponent(0), 0, Circuit.singleton.GetElectricalComponent(1), 0);
			Circuit.singleton.AddConnection(Circuit.singleton.GetElectricalComponent(0), 1, Circuit.singleton.GetElectricalComponent(1), 1);
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
