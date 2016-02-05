using UnityEngine;
using System.Collections;

public class ResetScript : MonoBehaviour {
	static bool messageShown = false;
	public GameObject fuelCellGO;
	bool oldCursorVis;
	float oldTimeScale;

	// Use this for initialization
	void Start () {
	
		fuelCellGO = GameObject.Find ("Cell");
	
	}
	
	// Update is called once per frame
	void Update () {
	
		if (fuelCellGO.GetComponent<Cell>().isTripped && !messageShown){
			transform.GetChild(0).gameObject.SetActive(true);
			messageShown = true;
			oldCursorVis = Cursor.visible;
			oldTimeScale = Time.timeScale;
			Cursor.visible = true;
			Time.timeScale = 0;
		}
	
	}
	
	public void OK(){
		Cursor.visible = oldCursorVis;
		gameObject.SetActive(false);
		Time.timeScale = oldTimeScale;
	}
}
