using UnityEngine;
using System.Collections;
//using System.Collections.Generic;
//using UnityEngine.Analytics;

public class ExitDoor : MonoBehaviour {

	public GameObject electricsGO;
	
	public GameObject doorPanelGO;
	public AudioSource doorOpen;
	public AudioSource doorClose;
	public GameObject indicatorGO;
	public GameObject avowGridGO;
	
	public string nextLevelName;
	
	public enum Dir{
		kGoLeft,
		kGoRight,
	}
	
	bool hasTriggered = false;
	
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
	
		if (avowGridGO != null){
			avowGridGO.GetComponent<AVOWGrid>().SetBubble(
				electricsGO.GetComponent<ElectricalComponent>().GetVoltageMin(), 
				electricsGO.GetComponent<ElectricalComponent>().GetVoltageMax(),
				electricsGO.GetComponent<ElectricalComponent>().GetSimFwCurrent());
		}
		
	}
	
	void OnTriggerEnter2D(Collider2D collider){
		if (isOpen && collider.gameObject.tag == "Player" && !hasTriggered){
			GameMode.singleton.isEndOfLevel = true;
			GameMode.singleton.nextLevelName = nextLevelName;
			hasTriggered = true;
//			Debug.Log ("levelComplete - levelTime: " + Time.timeSinceLevelLoad + ", gameTime: " + (Time.time - GameMode.gameStartTime));
			
			GoogleAnalytics.Client.SendTimedEventHit("gameFlow", "levelComplete", Application.loadedLevelName, Time.timeSinceLevelLoad);
			GoogleAnalytics.Client.SendScreenHit("levelComplete_" + Application.loadedLevelName);		
			
//
//			Analytics.CustomEvent("levelComplete", new Dictionary<string, object>
//			                      {
//				{ "levelName", Application.loadedLevelName },
//				{ "levelTime",Time.timeSinceLevelLoad },
//				{ "gameTime", (Time.time - GameMode.gameStartTime)},
//			});			
		}
	}
	
}
