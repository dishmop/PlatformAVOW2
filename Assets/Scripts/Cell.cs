using UnityEngine;
using System.Collections;

public class Cell : MonoBehaviour {

	
	public GameObject avowGridBackgroundGO;
	public GameObject resetGO;
	public Color resetCol;
	Color glowCol = Color.red;
	Color oldCellCol;
	
	public float maxCurrent = 2.25f;
	
	public GameObject cellElectrics;
	public bool isTripped = false;
	
	float oldVoltageRise = -1;
	float resetMin = 1f;
	float resetMax = 1.55f;
	float resetSpeed = 0.2f;
	
	bool tripTimerStarted;
	float tripTime = 0;
	float tripSafeDuration = 0.2f;
	
	// Use this for initialization
	void Start () {
		oldVoltageRise = cellElectrics.GetComponent<ElectricalComponent>().voltageRise;
		oldCellCol = GetComponent<SpriteRenderer>().color;
	
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		if (isTripped){
			cellElectrics.GetComponent<ElectricalComponent>().voltageRise = 0.001f;
		}
		else{
			cellElectrics.GetComponent<ElectricalComponent>().voltageRise = Mathf.Min (cellElectrics.GetComponent<ElectricalComponent>().voltageRise +0.06f, oldVoltageRise);
			
		}

		float current = cellElectrics.GetComponent<ElectricalComponent>().GetSimFwCurrent();

		// Must be strictly greater than at trigger the trip switch
		if (!isTripped && !MathUtils.FP.Fleq(current, maxCurrent) && !tripTimerStarted) {
			tripTimerStarted = true;
			tripTime = Time.time;
//			transform.FindChild("Warning").gameObject.SetActive(true);
		}
//		float val = Mathf.Abs(cellElectrics.GetComponent<ElectricalComponent>().voltageRise - oldVoltageRise);
//		GetComponent<SpriteRenderer>().color = Color.Lerp(oldCellCol, glowCol, val);
//		avowGridBackgroundGO.GetComponent<Renderer>().material.SetColor("_EmissiveColor", Color.Lerp(Color.black, glowCol, val));
		
		if (tripTimerStarted && Time.time > tripTime + tripSafeDuration){
			isTripped = true;
			GetComponent<AudioSource>().Play();
			tripTimerStarted = false;
		}
		
	
		
		// Check if we have a short circuit
		if (CircuitSimulator.singleton.voltageError){
			cellElectrics.GetComponent<ElectricalComponent>().resistance = 0.01f;
		}
		else{
			cellElectrics.GetComponent<ElectricalComponent>().resistance = 0;
		}

		HandleTripSwitch();
	
	}
	
	void HandleTripSwitch(){
		Vector3 localPos = resetGO.transform.localPosition;
		if (isTripped){
			localPos.y = Mathf.Min (resetMax, localPos.y + resetSpeed);
			resetGO.GetComponent<Renderer>().material.color = resetCol + Mathf.Sin (Time.time * 2 * Mathf.PI) * new Color(32f/256f, 32f/256f, 32f/256f, 0);
			
			// Calc the mouse posiiton on world space
			Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint( Input.mousePosition);
			
			Collider2D collider = resetGO.GetComponent<Collider2D>();
			bool isOver = collider.OverlapPoint(new Vector2(mouseWorldPos.x, mouseWorldPos.y));
			resetGO.transform.GetChild(0).gameObject.SetActive(isOver);
			
			if (isOver && Input.GetMouseButtonDown(0)){
				isTripped = false;
				GetComponent<AudioSource>().Play();
			}
			
			
		}
		else{
			localPos.y = Mathf.Max (resetMin, localPos.y - resetSpeed);
		}
		resetGO.transform.localPosition = localPos;
		
	}
}
