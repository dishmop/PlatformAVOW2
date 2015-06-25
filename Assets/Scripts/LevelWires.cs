using UnityEngine;
using System.Collections;

public class LevelWires : MonoBehaviour {

	[System.Serializable]
	public class WireRoute{
		public GameObject 	component0;
		public int			connectionIndex0;
		public GameObject 	component1;
		public int			connectionIndex1;
	};
	
	public WireRoute[] wireRoutes;
	
	bool firstUpdate = true;

	// Use this for initialization
	void Update () {
	
		if (!firstUpdate) return;
		firstUpdate = false;
	
		foreach (WireRoute route in wireRoutes){
			GameObject wireGO = GameObject.Instantiate(Factory.singleton.wirePrefab);
			wireGO.transform.SetParent(transform);
			wireGO.GetComponent<Wire>().ends[0].component = route.component0;
			wireGO.GetComponent<Wire>().ends[1].component = route.component1;
			
			route.component0.GetComponent<ElectricalComponent>().connectionData[route.connectionIndex0].wire = wireGO;
			route.component1.GetComponent<ElectricalComponent>().connectionData[route.connectionIndex1].wire = wireGO;
			Circuit.singleton.AddWire(wireGO);
		}
	
	}
	

}
