using UnityEngine;
using System.Collections;

public class CamController : MonoBehaviour {
	public GameObject trackObject;
	public float zPos = -10;


	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		Renderer renderer = trackObject.GetComponent<Renderer>();
		if (renderer != null){
			Bounds trackBounds = renderer.bounds;
			transform.position = new Vector3(trackBounds.center.x, trackBounds.center.y, zPos);
			
			// Set the size so the whole of the object is visible on screen
			float screenAspectRatio = (float)Screen.width / (float)Screen.height;
			float trackWidth = trackBounds.max.x - trackBounds.min.x;
			float trackHeight = trackBounds.max.y - trackBounds.min.y;
			float trackAspectRatio = trackWidth / trackHeight;
			
			// If we are limited by screen height
			if (screenAspectRatio > trackAspectRatio){
				GetComponent<Camera>().orthographicSize = trackHeight * 0.5f;
			}
			// If we are limited by screen width
			else{
				float virtualHeight = trackWidth / screenAspectRatio;
				GetComponent<Camera>().orthographicSize = virtualHeight * 0.5f;
			}
			
			
			
		}
			
		
		
	
	
	}
}
