using UnityEngine;
using System.Collections;

public class ElectricHum : MonoBehaviour {

	public GameObject electrics;
	public GameObject motorASD;
	
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if (!motorASD.GetComponent<ASDAudioSource>().IsSustainingOrAttacking()){
			GetComponent<AudioSource>().volume = Mathf.Abs(electrics.GetComponent<ElectricalComponent>().GetSimFwCurrent());
		}
		else{
			GetComponent<AudioSource>().volume = 0;
		
		}
	
	}
}
