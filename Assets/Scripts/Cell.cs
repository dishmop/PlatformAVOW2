using UnityEngine;
using System.Collections;

public class Cell : MonoBehaviour {

	
	public GameObject avowGridBackgroundGO;
	Color glowCol = Color.red;
	Color oldCellCol;
	
	public float maxCurrent = 2.25f;
	
	public GameObject cellElectrics;
	
	float oldVoltageRise = -1;
	
	// Use this for initialization
	void Start () {
		oldVoltageRise = cellElectrics.GetComponent<ElectricalComponent>().voltageRise;
		oldCellCol = GetComponent<SpriteRenderer>().color;
	
	}
	
	// Update is called once per frame
	void FixedUpdate () {
	
		
		cellElectrics.GetComponent<ElectricalComponent>().voltageRise = Mathf.Min (cellElectrics.GetComponent<ElectricalComponent>().voltageRise +0.03f, oldVoltageRise);

		float current = cellElectrics.GetComponent<ElectricalComponent>().GetSimFwCurrent();

		// Must be strictly greater then at trigger the trip switch
		if (!MathUtils.FP.Fleq(current, maxCurrent)) {
			cellElectrics.GetComponent<ElectricalComponent>().voltageRise = 0.1f;
			transform.FindChild("Warning").gameObject.SetActive(true);
		}
		float val = Mathf.Abs(cellElectrics.GetComponent<ElectricalComponent>().voltageRise - oldVoltageRise);
		GetComponent<SpriteRenderer>().color = Color.Lerp(oldCellCol, glowCol, val);
		avowGridBackgroundGO.GetComponent<Renderer>().material.SetColor("_EmissiveColor", Color.Lerp(Color.black, glowCol, val));
		
		
	
		
		// Check if we have a short circuit
		if (CircuitSimulator.singleton.voltageError){
			cellElectrics.GetComponent<ElectricalComponent>().resistance = 0.01f;
		}
		else{
			cellElectrics.GetComponent<ElectricalComponent>().resistance = 0;
		}


	
	}
}
