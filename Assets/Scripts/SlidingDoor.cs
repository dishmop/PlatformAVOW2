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
			child.gameObject.SetActive(child.position.y < transform.position.y);
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
		
		
		slideDoorTransform.GetComponent<BoxCollider2D>().enabled = !isOpen;

		
		HandleSliderVisibility();
		if (avowGridGO != null){
			avowGridGO.GetComponent<AVOWGrid>().SetBubble(
				electricsGO.GetComponent<ElectricalComponent>().GetVoltageMin(), 
				electricsGO.GetComponent<ElectricalComponent>().GetVoltageMax());
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
			
			if (current < -0.1){
				indicatorGO.GetComponent<SpriteRenderer>().color = GameConfig.singleton.indicatorError;
			}
			else if (MathUtils.FP.Fgeq(current, minVoltage)){
				indicatorGO.GetComponent<SpriteRenderer>().color = GameConfig.singleton.indicatorUnpowered;
			}
			else if (current < 0.85f){
				indicatorGO.GetComponent<SpriteRenderer>().color = GameConfig.singleton.indicatorSemi;
			}
			else {
				indicatorGO.GetComponent<SpriteRenderer>().color = GameConfig.singleton.indicatorOK;
			}
		}
	}
}
