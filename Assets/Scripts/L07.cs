using UnityEngine;
using System.Collections;

public class L07 : MonoBehaviour {
	public GameObject cellTextGO;
	public GameObject electricsGO;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
		float current = electricsGO.GetComponent<ElectricalComponent>().GetSimFwCurrent();
		
		bool showMessage = (Mathf.Abs(current) > 0.9f);
		cellTextGO.SetActive(showMessage);
			
	
	}
}
