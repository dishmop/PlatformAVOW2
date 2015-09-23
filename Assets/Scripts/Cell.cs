using UnityEngine;
using System.Collections;

public class Cell : MonoBehaviour {

	public GameObject powerLevel0GO;
	public GameObject powerLevel1GO;
	public GameObject powerLevel2GO;
	
	public float tripDuration = 1;
	float tripTime = -100;
	
	public GameObject cellElectrics;
	
	float oldVoltageRise = -1;
	
	// Use this for initialization
	void Start () {
		oldVoltageRise = cellElectrics.GetComponent<ElectricalComponent>().voltageRise;
	
	}
	
	// Update is called once per frame
	void FixedUpdate () {
	
		if (Time.fixedTime > tripTime + tripDuration){
			cellElectrics.GetComponent<ElectricalComponent>().voltageRise =oldVoltageRise;
	
		
			float current = cellElectrics.GetComponent<ElectricalComponent>().GetSimFwCurrent();
			if (MathUtils.FP.Feq (current, 0)){
				powerLevel0GO.GetComponent<SpriteRenderer>().color = GameConfig.singleton.indicatorUnpowered;
				powerLevel1GO.GetComponent<SpriteRenderer>().color = GameConfig.singleton.indicatorUnpowered;
				powerLevel2GO.GetComponent<SpriteRenderer>().color = GameConfig.singleton.indicatorUnpowered;
			}
			else if (MathUtils.FP.Fleq (current, 1)){
				powerLevel0GO.GetComponent<SpriteRenderer>().color = GameConfig.singleton.indicatorOK * current;
				powerLevel1GO.GetComponent<SpriteRenderer>().color = GameConfig.singleton.indicatorUnpowered;
				powerLevel2GO.GetComponent<SpriteRenderer>().color = GameConfig.singleton.indicatorUnpowered;
			}
			else if (MathUtils.FP.Fleq (current, 2)){
				powerLevel0GO.GetComponent<SpriteRenderer>().color = GameConfig.singleton.indicatorOK;
				powerLevel1GO.GetComponent<SpriteRenderer>().color = GameConfig.singleton.indicatorOK * (current-1);
				powerLevel2GO.GetComponent<SpriteRenderer>().color = GameConfig.singleton.indicatorUnpowered;
			}
			else {
				powerLevel0GO.GetComponent<SpriteRenderer>().color = GameConfig.singleton.indicatorOK;
				powerLevel1GO.GetComponent<SpriteRenderer>().color = GameConfig.singleton.indicatorOK;
				powerLevel2GO.GetComponent<SpriteRenderer>().color = GameConfig.singleton.indicatorError;
				cellElectrics.GetComponent<ElectricalComponent>().voltageRise = 0.1f;
				tripTime = Time.fixedTime;
				transform.FindChild("Warning").gameObject.SetActive(true);
			}
		}
		
		// Check if we have a short circuit
		if (CircuitSimulator.singleton.voltageError){
			cellElectrics.GetComponent<ElectricalComponent>().resistance = 0.01f;
		}
		else{
			cellElectrics.GetComponent<ElectricalComponent>().resistance = 0;
		}


	
	}
}
