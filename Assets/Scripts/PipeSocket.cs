using UnityEngine;
using System.Collections;

public class PipeSocket : MonoBehaviour {

	public GameObject plusIconGO;
	public GameObject minusIconGO;
	
	Color plusInitColor;
	Color minusInitColor;
	
	public enum Type{
		kNeutral,
		kPlus,
		kMinus
	}
	
	public Type type = Type.kNeutral;

	// Use this for initialization
	void Start () {
		plusInitColor = plusIconGO.GetComponent<Renderer>().material.color;
		minusInitColor = minusIconGO.GetComponent<Renderer>().material.color;
	
	}
	
	// Update is called once per frame
	void Update () {
	
		plusIconGO.SetActive(type == Type.kPlus);
		minusIconGO.SetActive(type == Type.kMinus);
		
		// Set the colours up (if we have been darkened then darken the icons too).
		Color mulCol = Color.Lerp(Color.white, GetComponent<Renderer>().material.color, 0.75f);
		plusIconGO.GetComponent<Renderer>().material.color = plusInitColor * mulCol;
		minusIconGO.GetComponent<Renderer>().material.color = minusInitColor * mulCol;
		
	
	}
}
