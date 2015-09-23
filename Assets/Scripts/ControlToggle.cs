using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ControlToggle : MonoBehaviour {

	public GameObject controlsGO;
	bool controlsVisible = false;


	
	// Update is called once per frame
	void Update () {
	
		if (UnityEngine.Input.GetKeyDown(KeyCode.H)){
			controlsVisible = !controlsVisible;
		}
		controlsGO.SetActive(controlsVisible);
		transform.FindChild("Text").GetComponent<Text>().text = controlsVisible ? "Press [H] to hide controls" : "Press [H] to show controls";
	
	}
}
