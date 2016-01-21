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
			avowGridGO.GetComponent<AVOWGrid>().SetBubble(
				electricsGO.GetComponent<ElectricalComponent>().GetVoltageMin(), 
				electricsGO.GetComponent<ElectricalComponent>().GetVoltageMax());
		}
	}
	
	// Update is called once per frame
	void FixedUpdate () {
	
		float current = electricsGO.GetComponent<ElectricalComponent>().GetSimFwCurrent() ;
		
		float oldHeight = liftHeight;
		
		if (current > 0.5f){
			liftHeight = Mathf.Min (liftHeight + speed * Time.deltaTime, liftMax);
			
		}
		else if (current < -0.5f){
			liftHeight = Mathf.Max (liftHeight - speed * Time.deltaTime, 0);
		}
		Vector3 newPos = platformInitPos + new Vector3(0, liftHeight, 0);
		liftPlatformGO.transform.position = newPos;
		
		if (!MathUtils.FP.Feq(liftHeight, oldHeight)){
			if (!GetComponent<ASDAudioSource>().IsPlaying()){
				GetComponent<ASDAudioSource>().Play();
			}
		}
		else{
			if (GetComponent<ASDAudioSource>().IsPlaying()){
				GetComponent<ASDAudioSource>().Stop();
			}
		}
		
		bool haveMissedLift = liftHeight > 0.7 && playerGO.transform.position.y < transform.position.y + 0.2f;
		
		transform.FindChild("LiftPlatform").FindChild("Text").gameObject.SetActive(haveMissedLift);
		
	
	}
}
