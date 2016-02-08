using UnityEngine;
using System.Collections;
//using System.Collections.Generic;
//using UnityEngine.Analytics;

public class ExitDoor : MonoBehaviour {

	public GameObject electricsGO;
	
	public GameObject doorPanelGO;
	public AudioSource doorOpen;
	public AudioSource doorClose;
	public GameObject avowGridGO;
	

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
		if (!electricsGO) return;
		
		bool wasOpen = isOpen;
		float current = Mathf.Abs(electricsGO.GetComponent<ElectricalComponent>().GetSimFwCurrent() );
		isOpen = (current> 0.9f);
		// If closing
		if (wasOpen && !isOpen){
			doorClose.Play ();
		}
		if (!wasOpen && isOpen){	
			doorOpen.Play ();
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
			float speed = 0;
			float offset = 0;
			
			GameObject wire0GO = electricsGO.GetComponent<ElectricalComponent>().connectionData[0].wire;
			if (wire0GO != null){
				Wire wire0 = wire0GO.GetComponent<Wire>();
				int wireEndIndex = wire0.ends[0].component == electricsGO ? 0 : 1;
				
				wire0.GetSpeedOffset(wireEndIndex, out speed, out offset);
			}
			
			avowGridGO.GetComponent<AVOWGrid>().SetBubble(
				electricsGO.GetComponent<ElectricalComponent>().GetVoltageMin(), 
				electricsGO.GetComponent<ElectricalComponent>().GetVoltageMax(),
				electricsGO.GetComponent<ElectricalComponent>().GetSimFwCurrent(),
				speed,
				offset
				);
			CircuitSimulator.singleton.RegisterPulseEdge(electricsGO.GetComponent<ElectricalComponent>().simEdgeId, speed, offset, true);
		}
		
	}
	
	void OnTriggerEnter2D(Collider2D collider){
		if (isOpen && collider.gameObject.tag == "Player" && !hasTriggered){
			GameMode.singleton.isEndOfLevel = true;
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
