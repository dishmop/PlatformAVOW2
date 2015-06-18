using UnityEngine;
using System.Collections;

public class TerminalFader : MonoBehaviour {

	// needed so we know what size to make out fader quad
	public GameObject sceneBackground;
	float minAlpha;

	// Use this for initialization
	void Start () {
		minAlpha = GetComponent<SpriteRenderer>().color.a;
		Color thisCol = GetComponent<SpriteRenderer>().color;
		GetComponent<SpriteRenderer>().color = new Color(thisCol.r, thisCol.g, thisCol.b, 0);
	
	}
	
	// Update is called once per frame
	void Update () {
		transform.localScale = sceneBackground.transform.localScale;
		Vector3 pos = new Vector3(sceneBackground.transform.position.x, sceneBackground.transform.position.y, transform.position.z);
		transform.position = pos;
		
		Color thisCol = GetComponent<SpriteRenderer>().color;
		float targetAlpha = (GameMode.singleton.isEditingCircuit) ? minAlpha : 0;
		float newAlpha = Mathf.Lerp(thisCol.a, targetAlpha, 0.4f);
		GetComponent<SpriteRenderer>().color = new Color(thisCol.r, thisCol.g, thisCol.b, newAlpha);
	
	}
}
