using UnityEngine;
using System.Collections;

public class ExitDoor : MonoBehaviour {

	public GameObject electricsGO;
	
	public GameObject doorPanelGO;
	public AudioSource doorOpen;
	public AudioSource doorClose;
	public GameObject indicatorGO;
	
	public string nextLevelName;
	
	public enum Dir{
		kGoLeft,
		kGoRight,
	}
	public Dir panelDir = Dir.kGoRight;
	float panelPos = 0;
	float speed = 1;
	
	public bool isOpen = false;
	
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
		doorPanelGO.transform.localPosition = new Vector3((panelDir == Dir.kGoRight) ? panelPos : -panelPos, 0, 0);
		
		if (isOpen){
			if (panelPos < 1){
				panelPos += speed * Time.fixedDeltaTime;
				if (panelPos > 1) panelPos = 1; 
			}
		}
		else{
			if (panelPos > 0){
				panelPos -= speed * Time.fixedDeltaTime;
				if (panelPos < 0) panelPos = 0; 
			}
		}
	}
	
	void Update(){
	}
	
	void OnTriggerEnter2D(Collider2D collider){
		if (isOpen && collider.gameObject.tag == "Player"){
			GameMode.singleton.isEndOfLevel = true;
			GameMode.singleton.nextLevelName = nextLevelName;
		}
	}
	
}
