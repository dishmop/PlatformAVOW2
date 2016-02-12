using UnityEngine;
using UnityEngine.UI;

using System.Collections;

public class LevelText : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
		string rawName = ReadSceneNames.singleton.scenes[Application.loadedLevel];
		string displayName = CreateDisplayName(rawName);
		transform.GetComponent<Text>().text = "Level " + Application.loadedLevel + ": " + displayName;
	
	}
	
	string CreateDisplayName( string rawName){
		return rawName.Substring(rawName.IndexOf(" ") + 1);
	}
}
