using UnityEngine;
using System.Collections;

public class ResetScript : MonoBehaviour {
	static bool messageShown = false;
	public GameObject fuelCellGO;
	bool oldCursorVis;
	float oldTimeScale;
	GameObject resetPanel;
	GameObject skipPanel;
	float delay = 1;
	float startTime = 0;
	static bool isTriggered  = false;

	
	// Use this for initialization
	void Start () {
		resetPanel = transform.FindChild("ResetPanel").gameObject;
		skipPanel = transform.FindChild ("SkipPanel").gameObject;
		if (fuelCellGO == null){
			fuelCellGO = GameObject.Find ("Cell");
		}
		
	
	}
	
	// Update is called once per frame
	void Update () {
		if (!fuelCellGO) return;
	
		if (fuelCellGO.GetComponent<Cell>().isTripped && !messageShown && !isTriggered){
			isTriggered = true;
			startTime = Time.time;
		}
		if (isTriggered && !fuelCellGO.GetComponent<Cell>().isTripped && !messageShown){
			isTriggered = false;
		}
		
		if (!messageShown && isTriggered && Time.time > startTime + delay){
			resetPanel.SetActive(true);
			skipPanel.SetActive (false);
			messageShown = true;
			oldCursorVis = Cursor.visible;
			oldTimeScale = Time.timeScale;
			Cursor.visible = true;
			Time.timeScale = 0;
		}
	
	}
	
	public void OK(){
		Cursor.visible = oldCursorVis;
		resetPanel.SetActive(false);
		skipPanel.SetActive (true);
		Time.timeScale = oldTimeScale;
	}
}
