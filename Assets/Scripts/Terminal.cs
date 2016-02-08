using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Terminal : MonoBehaviour {

	public bool isEditing = false;
	bool isInTrigger = false;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
		GetComponent<SpriteRenderer>().color = isInTrigger ? GameConfig.singleton.interactionReady : GameConfig.singleton.interactionNormal;
		
		if (isInTrigger && Time.timeScale != 0 && (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))){
			GameMode.singleton.isEditingCircuit = !GameMode.singleton.isEditingCircuit;
			isEditing = GameMode.singleton.isEditingCircuit;
			
		}
		if (GameMode.singleton.isEditingCircuit && Input.GetKeyDown(KeyCode.C) && Time.timeScale != 0){
			Circuit.singleton.RemoveAllWires();
		}
		
		transform.FindChild("Text").gameObject.SetActive(isInTrigger);
		TextMesh textComponent = transform.FindChild("Text").GetComponent<TextMesh>();
		
		textComponent.text = GameMode.singleton.isEditingCircuit ? "Press [W] to stop" : "Press [W] to interact";
		
		
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
