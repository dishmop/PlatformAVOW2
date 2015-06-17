using UnityEngine;
using System.Collections;

public class SlidingDoor : MonoBehaviour {

	public bool isOpen;
	public float offsetProp = 0;
	Transform slideDoorTransform;
	Vector3 sliderDoorLocalPos = Vector3.zero;
	float sliderHeight = 0;
	float speed = 2;

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
		
		if (isOpen){
			offsetProp = Mathf.Min (1, offsetProp + speed * Time.deltaTime);
		}
		else{
			offsetProp = Mathf.Max (0, offsetProp - speed * Time.deltaTime);
		}
		
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
		
		HandleSliderVisibility();
	
	}
}
