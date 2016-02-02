using UnityEngine;
using System.Collections;

public class Ladder : MonoBehaviour {

	public GameObject playerOnLadder = null;
	public GameObject ladderRung;
	public bool wasDisabled;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		wasDisabled = !ladderRung.activeSelf;
		ladderRung.SetActive (playerOnLadder);
		if (playerOnLadder){
			Vector3 rungPos = ladderRung.transform.position;
			if (playerOnLadder.transform.position.y > rungPos.y || wasDisabled){
				rungPos.y = playerOnLadder.transform.position.y;
				ladderRung.transform.position = rungPos;
			}
		}
	
	}
}
