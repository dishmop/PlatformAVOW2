using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour {

	public float walkSpeed = 10;
	
	public CircleCollider2D footCollider;


	
	GameObject model;
	public bool isGrounded = false;
	bool tryJump = false;
	
	float horizontalSpeed;

	// Use this for initialization
	void Start () {
		model = transform.FindChild("Animation_firstHuman_02").gameObject;
	
	}
	
	// Update is called once per frame
	void Update () {
		horizontalSpeed = walkSpeed * Input.GetAxis("Horizontal");
		if (Mathf.Abs (horizontalSpeed) > 0.01f){
			if (horizontalSpeed > 0){
				model.transform.rotation = Quaternion.Euler(0, 90, 0);
			}
			else{
				model.transform.rotation = Quaternion.Euler(0, -90, 0);
			}
		}
		
		tryJump = Input.GetMouseButton(0);
		
		
	}
	
	void FixedUpdate () {
//		Debug.Log("FixedUpdate: " + Time.fixedTime);

		Vector3 newVel = GetComponent<Rigidbody2D>().velocity;
		newVel.x = horizontalSpeed;
		GetComponent<Rigidbody2D>().velocity = newVel;
		model.GetComponent<Animator>().SetFloat("speed", Mathf.Abs (horizontalSpeed));
		
		model.GetComponent<Animator>().SetBool ("isGrounded", isGrounded);
		
		
		
		if (tryJump && isGrounded){
			GetComponent<Rigidbody2D>().AddForce(new Vector2(0, 2.5f), ForceMode2D.Impulse);
		}
		
		// Reset (so it can be set asgain by any collision messages)
		isGrounded = false;
	}
	
	void TestForGround(Collision2D collision){
	
		Vector2 upDir = new Vector3(0, 	1);
		foreach (ContactPoint2D contactPoint in collision.contacts){
			if (contactPoint.otherCollider != footCollider) continue;
			
			Vector2 collisionNormal = contactPoint.normal;
			Vector2 startPos = contactPoint.point;
			Vector2 endPos =  startPos + collisionNormal;

			float dotResult = Vector2.Dot(collisionNormal.normalized, upDir);
//			Debug.Log("Dot = " + dotResult);
			if (dotResult > 0.8f){
				isGrounded = true;
				Debug.DrawLine(startPos, endPos, Color.green);
			}
			else{
				Debug.DrawLine(startPos, endPos, Color.red);
				
			}
			
		}
		
		
	}
	
	void OnCollisionEnter2D(Collision2D collision){
		//Debug.Log("OnCollisionEnter2D: " + col.collider.gameObject.name);
		TestForGround(collision);
	}
		void OnCollisionStay2D(Collision2D collision){
		//Debug.Log("OnCollisionStay2D: " + col.collider.gameObject.name);
		TestForGround(collision);
	}
	
	void OnGUI(){
		float lineHeight = 20.0f;
		int lineNum = 0;
		GUI.Label(new Rect(10, lineHeight * lineNum++, Screen.width, lineHeight), "Idle: " + model.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("Idle").ToString());
		GUI.Label(new Rect(10, lineHeight * lineNum++, Screen.width, lineHeight), "Jump_New3: " + model.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("Jump_New3").ToString());
	}
	
}
