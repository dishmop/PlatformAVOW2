using UnityEngine;
using System.Collections;

public class Lift : MonoBehaviour {

	public GameObject liftPlatformGO;
	public GameObject liftSideGO;
	public GameObject electricsGO;
	public GameObject playerGO;
	public GameObject avowGridGO;
	
	float speed = 0.25f;
	
	Vector3 platformInitPos;
	
	float liftHeight = 0;
	public float liftMax = 1;

	// Use this for initialization
	void Start () {
		platformInitPos = liftPlatformGO.transform.position;
	
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
				offset,
				electricsGO.GetComponent<ElectricalComponent>().squareCol
				);
			CircuitSimulator.singleton.RegisterPulseEdge(electricsGO.GetComponent<ElectricalComponent>().simEdgeId, speed, offset, false, electricsGO.GetComponent<ElectricalComponent>().squareCol);
			
		}
	}
	
	// Update is called once per frame
	void FixedUpdate () {
	
		float current = electricsGO.GetComponent<ElectricalComponent>().GetSimFwCurrent();
		
		float oldHeight = liftHeight;
		
		if (MathUtils.FP.Fgeq(current, 0.9f)){
			liftHeight = Mathf.Min (liftHeight + speed * Time.deltaTime, liftMax);
			speed = 0.25f;
			
		}
		if (MathUtils.FP.Fgeq(current, 0.5f)){
			liftHeight = Mathf.Min (liftHeight + speed * Time.deltaTime, liftMax);
			speed = 0.125f;
			
		}
		else/* if (MathUtils.FP.Fleq(current, -0.5f))*/{
			liftHeight = Mathf.Max (liftHeight - 10 * speed * Time.deltaTime, 0);
		}
		
		Vector3 newPos = platformInitPos + new Vector3(0, liftHeight, 0);
		liftPlatformGO.transform.position = newPos;
		
		if (!MathUtils.FP.Feq(liftHeight, oldHeight)){
			if (!GetComponent<ASDAudioSource>().IsPlaying()){
				GetComponent<ASDAudioSource>().Play();
			}
			if (liftHeight > oldHeight){
				GetComponent<AudioSource>().pitch = speed * 4;
				GetComponent<AudioSource>().volume = 1f;
			}
			else{
				GetComponent<AudioSource>().pitch = 1.6f;
				GetComponent<AudioSource>().volume = 0.6f;
			}
			
		}
		else{
			if (GetComponent<ASDAudioSource>().IsPlaying()){
				GetComponent<ASDAudioSource>().Stop();
			}
		}
		
		
	
	}
}
