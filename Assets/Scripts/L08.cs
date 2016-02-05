using UnityEngine;
using System.Collections;

public class L08 : MonoBehaviour {

	public GameObject upperLabelGO;
	public GameObject lowerLabelGO;
	public GameObject doorElectricsGO;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
		bool isEditing = GameMode.singleton.isEditingCircuit;
		GameObject wireUpperGO = doorElectricsGO.GetComponent<ElectricalComponent>().connectionData[0].wire;
		GameObject wireLowerGO = doorElectricsGO.GetComponent<ElectricalComponent>().connectionData[1].wire;
		bool hasUpperConnection = (wireUpperGO != null && wireUpperGO != UI.singleton.attachedWire);
		bool hasLowerConnection = (wireLowerGO != null && wireLowerGO != UI.singleton.attachedWire);
		upperLabelGO.SetActive(isEditing && !hasUpperConnection);
		lowerLabelGO.SetActive(isEditing && hasUpperConnection && !hasLowerConnection);
		
	}
}
