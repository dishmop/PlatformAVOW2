using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour {

	public float walkSpeed = 10;
	public BoxCollider2D standingBox;
	public BoxCollider2D jumpingBox;
	BoxCollider2D activeBox;
	
	GameObject model;
	bool isGrounded = false;
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
		//transform.position = transform.position + horizontalVel * Time.fixedDeltaTime;
		
		activeBox = isGrounded ? standingBox : jumpingBox;
		Vector3 footPos = transform.position + new Vector3(0, activeBox.offset.y, 0) - 0.5f * new Vector3(0, activeBox.size.y, 0);
		
		
		bool lineTestFwd = false;
		if (Mathf.Abs(horizontalSpeed) > 0.1f){
			Vector3 testVec = (new Vector3(horizontalSpeed, 0, 0)).normalized * (0.1f + activeBox.size.x * 0.5f);
			Vector3 shinAdd = new Vector3(0, 0.01f, 0);
			lineTestFwd = Physics2D.Linecast(footPos + shinAdd, footPos + shinAdd + testVec, 1 << LayerMask.NameToLayer("Ground"));
			Debug.DrawLine(footPos + shinAdd, footPos + shinAdd + testVec, Color.yellow);
			
		}
		if (lineTestFwd){
			horizontalSpeed = 0f;
		}
		Vector3 newVel = GetComponent<Rigidbody2D>().velocity;
		newVel.x = horizontalSpeed;
		GetComponent<Rigidbody2D>().velocity = newVel;
		model.GetComponent<Animator>().SetFloat("speed", Mathf.Abs (horizontalSpeed));
		
		
		

		//Debug.DrawLine(transform.position, footPos, Color.yellow);
		
		Vector3 startPos = footPos + new Vector3(0, 1f, 0);
		Vector3 endPos = footPos + new Vector3(0, -0.1f, 0);
		
		
		Vector3 widthVec = new Vector3(0.2f, 0, 0);
		
		bool lineTest1 = Physics2D.Linecast(startPos - widthVec, endPos - widthVec, 1 << LayerMask.NameToLayer("Ground"));
		bool lineTest2 = Physics2D.Linecast(startPos + widthVec, endPos + widthVec, 1 << LayerMask.NameToLayer("Ground"));
		
		Debug.DrawLine(startPos - widthVec, endPos - widthVec, Color.red);
		Debug.DrawLine(startPos + widthVec, endPos + widthVec, Color.red);
		
		isGrounded = lineTest1 || lineTest2;
		model.GetComponent<Animator>().SetBool("isGrounded", isGrounded);
		
		if (tryJump && isGrounded){
			GetComponent<Rigidbody2D>().AddForce(new Vector2(0, 1.5f), ForceMode2D.Impulse);
		}
		
		// Set the collision box to the correct size
		standingBox.enabled = isGrounded;
		jumpingBox.enabled = !isGrounded;
		

			
		
	}
}
