using UnityEngine;
using System.Collections;

public class SlidingDoor : MonoBehaviour {

	public bool isOpen;
	public float offsetProp = 0;
	public GameObject indicatorGO;
	public GameObject avowGridGO;
	public GameObject electricsGO;
	
	Transform slideDoorTransform;
	Vector3 sliderDoorLocalPos = Vector3.zero;
	float sliderHeight = 0;
	float openSpeed = 0.25f;
	float closeSpeed = 1f;
	
	// Use this for initialization
	void Start () {
		slideDoorTransform = transform.FindChild("SlideDoor");
		sliderDoorLocalPos = slideDoorTransform.localPosition;
		sliderHeight = slideDoorTransform.GetComponent<BoxCollider2D>().size.y;
	
	}
	
	void HandleSliderVisibility(){
		foreach (Transform child in slideDoorTransform){
			if (MathUtils.FP.Feq(transform.rotation.eulerAngles.z, 180, 1)){
				child.gameObject.SetActive(child.position.y > transform.position.y);
			}
			else{
				child.gameObject.SetActive(child.position.y < transform.position.y);
			}
		}
	}
	
	// Update is called once per frame
	void Update () {
		Vector3 newLocalPos = sliderDoorLocalPos + new Vector3(0, offsetProp * sliderHeight, 0);
		slideDoorTransform.localPosition = newLocalPos;
		
		float oldOffset = offsetProp;
		
		float useSpeed = isOpen ? openSpeed : closeSpeed;
		if (isOpen){
			offsetProp = Mathf.Min (1, offsetProp + useSpeed * Time.deltaTime);
		}
		else{
			offsetProp = Mathf.Max (0, offsetProp - useSpeed * Time.deltaTime);
		}
		
		GetComponent<AudioSource>().pitch = useSpeed;
		
		
		// if it has changes
		if (oldOffset != offsetProp){
			// if we are not yet playing a sound - then start playing it
			if (!GetComponent<ASDAudioSource>().IsPlaying()){
				GetComponent<ASDAudioSource>().Play();
			}
		}
		else{
			if (GetComponent<ASDAudioSource>().IsPlaying()){
				GetComponent<ASDAudioSource>().Stop();
			}
			
		}
		
		
		slideDoorTransform.GetComponent<BoxCollider2D>().enabled = (offsetProp < 0.9f);

		
		HandleSliderVisibility();
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
				offset
				);
			CircuitSimulator.singleton.RegisterPulseEdge(electricsGO.GetComponent<ElectricalComponent>().simEdgeId, speed, offset, false);
			
		}
	}
	

	
	void FixedUpdate(){
	
		float minVoltage = 0.5f;
		if (avowGridGO != null){
			minVoltage = avowGridGO.GetComponent<AVOWGrid>().minVoltage;
		}
		ElectricalComponent component = electricsGO.GetComponent<ElectricalComponent>();
		if (component.simEdgeId >= 0){
			float current = electricsGO.GetComponent<ElectricalComponent>().GetSimFwCurrent();
			openSpeed = current * current;
			if (MathUtils.FP.Fgeq(current, minVoltage)){
				isOpen = true;
			}
			else{
				isOpen = false;
				
			}
			

		}
	}
}
