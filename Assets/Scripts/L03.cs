using UnityEngine;
using System.Collections;

public class L03 : MonoBehaviour {

	public GameObject textGO;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		textGO.SetActive(GameMode.singleton.isEditingCircuit);
	
	}
}
