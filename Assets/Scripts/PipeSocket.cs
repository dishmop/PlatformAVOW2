using UnityEngine;
using System.Collections;

public class PipeSocket : MonoBehaviour {

	public GameObject plusIconGO;
	public GameObject minusIconGO;
	
	public enum Type{
		kNeutral,
		kPlus,
		kMinus
	}
	
	public Type type = Type.kNeutral;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
		plusIconGO.SetActive(type == Type.kPlus);
		minusIconGO.SetActive(type == Type.kMinus);
		
	
	}
}
