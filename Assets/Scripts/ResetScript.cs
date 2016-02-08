using UnityEngine;
using System.Collections;

public class ResetScript : MonoBehaviour {
	static bool messageShown = false;
	public GameObject fuelCellGO;
	bool oldCursorVis;
	float oldTimeScale;
	GameObject resetPanel;
	GameObject skipPanel;

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
	
		if (fuelCellGO.GetComponent<Cell>().isTripped && !messageShown){
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
