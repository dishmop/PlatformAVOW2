using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour {

	public float walkSpeed = 10;
	GameObject model;
	
	float horizontalSpeed;

	// Use this for initialization
	void Start () {
		model = transform.FindChild("Animation_firstHuman_02").gameObject;
	
	}
	
	// Update is called once per frame
	void Update () {
		horizontalSpeed = walkSpeed * Input.GetAxis("Horizontal");
		float absHSpeed = Mathf.Abs (horizontalSpeed);
		if (absHSpeed > 0.01f){
			if (horizontalSpeed > 0){
				model.transform.rotation = Quaternion.Euler(0, 90, 0);
			}
			else{
				model.transform.rotation = Quaternion.Euler(0, -90, 0);
			}
		}
		model.GetComponent<Animator>().SetFloat("speed", absHSpeed);
		
	}
	
	void FixedUpdate () {
		//transform.position = transform.position + horizontalVel * Time.fixedDeltaTime;
		Vector3 newVel = GetComponent<Rigidbody2D>().velocity;
		newVel.x = horizontalSpeed;
		GetComponent<Rigidbody2D>().velocity = newVel;
		
	}
}
