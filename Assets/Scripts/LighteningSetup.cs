using UnityEngine;
using System.Collections;

public class LighteningSetup : MonoBehaviour {
	public Vector3 pos1;
	public Vector3 pos2;
	public bool isOn = false;

	// Use this for initialization
	void Start () {
		Update ();
	}
	
	public void EnableLightening(Vector3 pos1, Vector3 pos2){
		this.pos1 = pos1;
		this.pos2 = pos2;
		isOn = true;
	}
	
	public void DisableLightening(){
		isOn = false;
	}
	
	// Update is called once per frame
	void Update () {
		Lightening lightening = GetComponent<Lightening>();
		if (isOn){
			Vector3 fromHereToThere = pos1 - pos2;
			fromHereToThere.Normalize();
			
			lightening.startPoint = pos1;
			lightening.endPoint = pos2;
			
			float len = (lightening.startPoint  - lightening.endPoint).magnitude;
			lightening.numStages = Mathf.Max ((int)(len * 10), 2);
			lightening.size =   1f;
			lightening.ConstructMesh();
			GetComponent<AudioSource>().volume = 0.2f;
		
		}
		else{
			lightening.numStages = 0;
			lightening.ConstructMesh();
			GetComponent<AudioSource>().volume = 0f;
		}
	
	}
}
