using UnityEngine;
using System.Collections;
using System.Linq;

public class LeverSwitch : MonoBehaviour {

	public bool isInTrigger = false;
	public GameObject leverGO;
	public GameObject switchElectricsGO;
	public GameObject wireGO;
	public GameObject connectionGO0;
	public GameObject connectionGO1;
	
	public bool desiredOn = false;
	
	float angle = -45;
	float angleSpeed = 360f;
	
	enum State{
		kOn,
		kOff,
		kOnToOff,
		kOffToOn
	};
	
	State state;
	
	float transDuration = 0.2f;
	float onResistanceAngle;
	float maxResistanceAngle = 90f;
	float minResistanceAngle = 0f;
	
	
	// Use this for initialization
	void Start () {
	
		connectionGO0 = GameObject.Instantiate(Factory.singleton.emptyConnectorPrefab);
		connectionGO0.transform.SetParent(transform);
		connectionGO0.transform.position = transform.TransformPoint(switchElectricsGO.GetComponent<ElectricalComponent>().connectionData[0].pos);
		connectionGO0.GetComponent<ElectricalComponent>().connectionData[0].dir = Directions.CalcOppDir(switchElectricsGO.GetComponent<ElectricalComponent>().connectionData[0].dir);
		
		connectionGO1 = GameObject.Instantiate(Factory.singleton.emptyConnectorPrefab);
		connectionGO1.transform.SetParent(transform);
		connectionGO1.transform.position =  transform.TransformPoint(switchElectricsGO.GetComponent<ElectricalComponent>().connectionData[1].pos);
		connectionGO1.GetComponent<ElectricalComponent>().connectionData[0].dir = Directions.CalcOppDir(switchElectricsGO.GetComponent<ElectricalComponent>().connectionData[1].dir);
		
		wireGO = GameObject.Instantiate(Factory.singleton.wirePrefab);
		
		wireGO.transform.SetParent(transform);
		wireGO.GetComponent<Wire>().ends[0].component = connectionGO0;
		wireGO.GetComponent<Wire>().ends[1].component = connectionGO1;
		wireGO.GetComponent<Wire>().enableHandleInput = false;
		
		connectionGO0.GetComponent<ElectricalComponent>().connectionData[0].wire = wireGO;
		connectionGO1.GetComponent<ElectricalComponent>().connectionData[0].wire = wireGO;
		
		if (desiredOn){
			angle = 45;
			state = State.kOn;
			onResistanceAngle = minResistanceAngle;
		}
		else{
			angle = -45;
			state = State.kOff;
			onResistanceAngle = maxResistanceAngle;
		}

			
	}
	
	float GetResistance(float resistance){
		if (MathUtils.FP.Fgeq(resistance, 90)){
			return -1;
		}
		else{
			return Mathf.Tan(Mathf.Deg2Rad * resistance);
		}
	}
	
	
	void  UpdateEmptyConnectors(){
	
		int index = desiredOn ? 1 : 2;
		
		connectionGO1.transform.position =  transform.TransformPoint(switchElectricsGO.GetComponent<ElectricalComponent>().connectionData[index].pos);
		connectionGO1.GetComponent<ElectricalComponent>().connectionData[0].dir = Directions.CalcOppDir(switchElectricsGO.GetComponent<ElectricalComponent>().connectionData[index].dir);
		
		if (connectionGO0.GetComponent<ElectricalComponent>().simNodeIndices.Count() == 0){
			return;
		}
		if (switchElectricsGO.GetComponent<ElectricalComponent>().simNodeIndices != null && switchElectricsGO.GetComponent<ElectricalComponent>().simNodeIndices.Count() != 0){
			connectionGO0.GetComponent<ElectricalComponent>().simNodeIndices [0] = switchElectricsGO.GetComponent<ElectricalComponent>().simNodeIndices[0];
			connectionGO1.GetComponent<ElectricalComponent>().simNodeIndices [0] = switchElectricsGO.GetComponent<ElectricalComponent>().simNodeIndices[index];
		}
		
		
	}
	
	
	// Update is called once per frame
	void Update () {
		if (desiredOn){
			angle = Mathf.Min (45, angle + angleSpeed * Time.deltaTime);
		}
		else{
			angle = Mathf.Max (-45, angle - angleSpeed * Time.deltaTime);
		}
		leverGO.transform.rotation = Quaternion.Euler(0, 0, angle);
		
		leverGO.transform.FindChild ("SwitchLever").GetComponent<SpriteRenderer>().color = isInTrigger ? GameConfig.singleton.interactionReady : GameConfig.singleton.interactionNormal;
		
		
		if (isInTrigger  && Time.timeScale != 0 && (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))){
			desiredOn = !desiredOn;
			GetComponent<AudioSource>().Play();
			
		}
		UpdateEmptyConnectors();
		
		float angleToTurn = (maxResistanceAngle - minResistanceAngle) * Time.deltaTime / transDuration;
		

		switch (state){
			case State.kOn:
			{
				onResistanceAngle = minResistanceAngle;
				if (!desiredOn)
				{
					state = State.kOnToOff;
				}
				break;
			}
			case State.kOnToOff:
			{
				onResistanceAngle += angleToTurn;
				if (onResistanceAngle >= maxResistanceAngle){
					onResistanceAngle = maxResistanceAngle;
					state = State.kOff;
				}
				break;
			}
			case State.kOff:
			{
				onResistanceAngle = maxResistanceAngle;
				if (desiredOn)
				{
					state = State.kOffToOn;
				}
				break;
			}
			case State.kOffToOn:
			{
				onResistanceAngle -= angleToTurn;
				if (onResistanceAngle <= minResistanceAngle){
					onResistanceAngle = minResistanceAngle;
					state = State.kOn;
				}
				break;
			}
		}
		
		
		int simEdgeId = switchElectricsGO.GetComponent<ElectricalComponent>().internalRouting[0].simEdgeId;
		
		wireGO.GetComponent<Wire>().simEdgeId = simEdgeId;
		
		
		switchElectricsGO.GetComponent<ElectricalComponent>().internalRouting[0].resistance = GetResistance(maxResistanceAngle - onResistanceAngle);
		switchElectricsGO.GetComponent<ElectricalComponent>().internalRouting[1].resistance = GetResistance(onResistanceAngle);
		
		transform.FindChild("Text").gameObject.SetActive(isInTrigger);
	
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
