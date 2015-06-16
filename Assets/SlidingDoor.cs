using UnityEngine;
using System.Collections;

public class SlidingDoor : MonoBehaviour {

	public bool isOpen;
	public float offsetProp = 0;
	Transform slideDoorTransform;
	Vector3 sliderDoorLocalPos = Vector3.zero;
	public float sliderHeight = 0;
	public float speed = 1;

	// Use this for initialization
	void Start () {
		slideDoorTransform = transform.FindChild("SlideDoor");
		sliderDoorLocalPos = slideDoorTransform.localPosition;
		sliderHeight = slideDoorTransform.GetComponent<BoxCollider2D>().size.y;
	
	}
	
	// Update is called once per frame
	void Update () {
		Vector3 newLocalPos = sliderDoorLocalPos + new Vector3(0, offsetProp * sliderHeight, 0);
		slideDoorTransform.localPosition = newLocalPos;
		
		if (isOpen){
			offsetProp = Mathf.Min (1, offsetProp + speed * Time.deltaTime);
		}
		else{
			offsetProp = Mathf.Max (0, offsetProp - speed * Time.deltaTime);
		}
	
	}
}
