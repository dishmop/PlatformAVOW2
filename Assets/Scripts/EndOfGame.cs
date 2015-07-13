using UnityEngine;
using System.Collections;

public class EndOfGame : MonoBehaviour {

	public void RestartGame(){
		Application.LoadLevel("Menu");
	}

	// Use this for initialization
	void Start () {
	Cursor.visible = true;
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
