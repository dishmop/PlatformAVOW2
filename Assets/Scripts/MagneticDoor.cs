using UnityEngine;
using System.Collections;
//using System.Collections.Generic;
//using UnityEngine.Analytics;

public class MagneticDoor : MonoBehaviour {

	public GameObject electricsGO;
	
	public GameObject doorPanelGO;
	public GameObject avowGridGO;
	public float maxDoorPos;
	public float minDoorPos;
	public GameObject magneticLinesGO;
	
	
	public bool isUp = false;
	
	float speed = 10;
	
	
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void FixedUpdate(){
		float current = electricsGO.GetComponent<ElectricalComponent>().GetSimFwCurrent() ;
		isUp =  (Mathf.Abs (current) > 0.9f);
		
		magneticLinesGO.GetComponent<Renderer>().material.SetFloat("_Intensity", Mathf.Abs(current));
		
		
		Vector3 doorPos = doorPanelGO.transform.localPosition;
		if (isUp && !GetComponent<AudioSource>().isPlaying){
			GetComponent<AudioSource>().Play();
		}
		else if (!isUp && GetComponent<AudioSource>().isPlaying){
			GetComponent<AudioSource>().Stop();
		}
		
		if (isUp){
			doorPos.y = Mathf.Min (doorPos.y + speed * Time.fixedDeltaTime, maxDoorPos);

		}
		else{
			doorPos.y = Mathf.Max (doorPos.y - speed * Time.fixedDeltaTime, minDoorPos);
		}
		doorPanelGO.transform.localPosition = doorPos;
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
			CircuitSimulator.singleton.RegisterPulseEdge(electricsGO.GetComponent<ElectricalComponent>().simEdgeId, speed, offset);
		}
		
	}
	
	
}
