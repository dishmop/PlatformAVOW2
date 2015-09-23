using UnityEngine;
using System.Collections;

public class PowerWarning : MonoBehaviour {

	float triggerTime = -100;
	float pulseDuration = 1;

	// Use this for initialization
	void Start () {
	
	}
	
	void OnEnable(){
		triggerTime = Time.time;
	}
	
	
	// Update is called once per frame
	void Update () {
		if (Time.time > triggerTime + pulseDuration){
			gameObject.SetActive(false);
		}
	
	
	}
}
