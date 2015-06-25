using UnityEngine;
using System.Collections;

public class ExitDoor : MonoBehaviour {

	public GameObject electricsGO;
	
	public GameObject openDoorGO;
	public GameObject shutDoorGO;
	public AudioSource doorOpen;
	public AudioSource doorClose;
	public GameObject indicatorGO;
	
	public string nextLevelName;
	
	bool isOpen = false;
	
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void FixedUpdate(){
		bool wasOpen = isOpen;
		float current = electricsGO.GetComponent<ElectricalComponent>().GetSimFwCurrent() ;
		isOpen = (current> 0.75f);
		// If closing
		if (wasOpen && !isOpen){
			doorClose.Play ();
		}
		if (!wasOpen && isOpen){	
			doorOpen.Play ();
		}
		
		if (current < -0.1){
			indicatorGO.GetComponent<SpriteRenderer>().color = GameConfig.singleton.indicatorError;
		}
		else if (current < 0.75f){
			indicatorGO.GetComponent<SpriteRenderer>().color = GameConfig.singleton.indicatorUnpowered;
		}
		else {
			indicatorGO.GetComponent<SpriteRenderer>().color = GameConfig.singleton.indicatorOK;
		}
	}
	
	void Update(){
		openDoorGO.SetActive(isOpen);
		shutDoorGO.SetActive(!isOpen);
	}
	
	void OnTriggerEnter2D(Collider2D collider){
		if (collider.gameObject.tag == "Player"){
			GameMode.singleton.isEndOfLevel = true;
			GameMode.singleton.nextLevelName = nextLevelName;
		}
	}
	
}
