using UnityEngine;
using System.Collections;

public class Terminal : MonoBehaviour {

	bool isInTrigger = false;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
		GetComponent<SpriteRenderer>().color = isInTrigger ? GameConfig.singleton.interactionReady : GameConfig.singleton.interactionNormal;
		
		if (isInTrigger && Input.GetKeyDown(KeyCode.Space)){
			GameMode.singleton.isEditingCircuit = !GameMode.singleton.isEditingCircuit;
			
		}
		
	}
	
	
	
	void OnTriggerEnter2D(Collider2D collider){
		if (collider.gameObject.tag == "Player"){
			isInTrigger = true;
		}
	}
	
	void OnTriggerExit2D(Collider2D collider){
		if (collider.gameObject.tag == "Player"){
			isInTrigger = false;
		}
	}
}
