using UnityEngine;
using System.Collections;

public class Piston : MonoBehaviour {

	public GameObject liftPlatformGO;
	public GameObject liftBaseGO;
	public GameObject springPoleGO;
	public GameObject electricsGO;
	public GameObject avowGridGO;
	public GameObject hummGO;
	
	float springConstant = 4;
	float motorMaxForce = 8;
	
	float initialSpringSize = -1;
	
	Vector3 platformInitPos;
	

	// Use this for initialization
	void Start () {
		platformInitPos = liftPlatformGO.transform.position;
		
		// get the top and bottom of base and platform.
		float bottom = liftBaseGO.GetComponent<BoxCollider2D>().bounds.max.y;
		float top = liftPlatformGO.GetComponent<BoxCollider2D>().bounds.min.y;
		initialSpringSize = top - bottom;
	
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
			float current = electricsGO.GetComponent<ElectricalComponent>().GetSimFwCurrent();
			avowGridGO.GetComponent<AVOWGrid>().SetBubble(
				electricsGO.GetComponent<ElectricalComponent>().GetVoltageMin(), 
				electricsGO.GetComponent<ElectricalComponent>().GetVoltageMax(),
				current,
				speed,
				offset,
				electricsGO.GetComponent<ElectricalComponent>().squareCol
				);
			CircuitSimulator.singleton.RegisterPulseEdge(electricsGO.GetComponent<ElectricalComponent>().simEdgeId, speed, offset, false, electricsGO.GetComponent<ElectricalComponent>().squareCol);
			
			hummGO.GetComponent<AudioSource>().volume = Mathf.Clamp01(Mathf.Abs (2 * current));
			hummGO.GetComponent<AudioSource>().pitch = Mathf.Abs (2 * current);
		}
		UpdateSpring();
		
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		Vector3 liftPos = liftPlatformGO.transform.position;
		
		float current = electricsGO.GetComponent<ElectricalComponent>().GetSimFwCurrent();
		
		float calcMotorForce = current * motorMaxForce;
		float calcSpringForce = (liftPos - platformInitPos).y * springConstant;
		float speed = calcMotorForce - calcSpringForce;
		
		
		
		liftPos.y = liftPos.y + speed * Time.fixedDeltaTime;
		
		
		liftPlatformGO.transform.position = liftPos;
		
		GetComponent<AudioSource>().pitch = Mathf.Abs(speed * 0.3f);
		GetComponent<AudioSource>().volume = Mathf.Abs(speed);
		
		if (MathUtils.FP.Feq(current, 0)){
			if (GetComponent<ASDAudioSource>().IsPlaying()){
				GetComponent<ASDAudioSource>().Stop();
			}	
		}
		else{
			if (!GetComponent<ASDAudioSource>().IsPlaying()){
				GetComponent<ASDAudioSource>().Play();
			}	
		}
	}
	
	void UpdateSpring(){
		// get the top and bottom of base and platform.
		float bottom = liftBaseGO.GetComponent<BoxCollider2D>().bounds.max.y;
		float top = liftPlatformGO.GetComponent<BoxCollider2D>().bounds.min.y;
		springPoleGO.transform.position = new Vector3(transform.position.x, 0.5f * (top + bottom), 0);
		springPoleGO.transform.localScale = new Vector3(1, (top - bottom) / initialSpringSize, 0);
		
	}
}
