using UnityEngine;
using System.Collections;

public class SplashScreen : MonoBehaviour {

	public void StartGame(){
		Application.LoadLevel("Level1");
	}

	// Use this for initialization
	void Start () {
		Cursor.visible = true;
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
