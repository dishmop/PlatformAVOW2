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
	
	public bool isOn = false;
	float angle = -45;
	float angleSpeed = 360f;
	
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
		
		
		

		
	}
	
	
	void  UpdateEmptyConnectors(){
		int index = isOn ? 1 : 2;
		
		connectionGO1.transform.position =  transform.TransformPoint(switchElectricsGO.GetComponent<ElectricalComponent>().connectionData[index].pos);
		connectionGO1.GetComponent<ElectricalComponent>().connectionData[0].dir = Directions.CalcOppDir(switchElectricsGO.GetComponent<ElectricalComponent>().connectionData[index].dir);
		
		
		if (switchElectricsGO.GetComponent<ElectricalComponent>().simNodeIndices != null && switchElectricsGO.GetComponent<ElectricalComponent>().simNodeIndices.Count() != 0){
			connectionGO0.GetComponent<ElectricalComponent>().simNodeIndices [0] = switchElectricsGO.GetComponent<ElectricalComponent>().simNodeIndices[0];
			connectionGO1.GetComponent<ElectricalComponent>().simNodeIndices [0] = switchElectricsGO.GetComponent<ElectricalComponent>().simNodeIndices[index];
		}
		
		
	}
	
	
	// Update is called once per frame
	void Update () {
		if (isOn){
			angle = Mathf.Min (45, angle + angleSpeed * Time.deltaTime);
		}
		else{
			angle = Mathf.Max (-45, angle - angleSpeed * Time.deltaTime);
		}
		leverGO.transform.rotation = Quaternion.Euler(0, 0, angle);
		
		leverGO.transform.FindChild ("SwitchLever").GetComponent<SpriteRenderer>().color = isInTrigger ? GameConfig.singleton.interactionReady : GameConfig.singleton.interactionNormal;
		
		
		if (isInTrigger && Input.GetKeyDown(KeyCode.Space)){
			isOn = !isOn;
			
		}
		UpdateEmptyConnectors();
		
		switchElectricsGO.GetComponent<ElectricalComponent>().internalRouting[0].connectionIndex1 = isOn ? 1 : 2;
	
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
