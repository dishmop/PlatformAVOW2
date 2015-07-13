using UnityEngine;
using System.Collections;

public class QuitOnEsc : MonoBehaviour {

	public string OnQuitLevelName;


	// Use this for initialization
	void Start () {
//		Cursor.visible = false;
		

	
	}
	
	// Update is called once per frame
	void Update () {

		
		// Test for exit
		if (Input.GetKeyDown (KeyCode.Escape)) {
			if (OnQuitLevelName != null && OnQuitLevelName != ""){
				Application.LoadLevel(OnQuitLevelName);
			}
			else{
				AppHelper.Quit();
			}
		}
		

			
		
	}
}
