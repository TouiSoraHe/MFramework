using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour {

	// Use this for initialization
	void Start () {
		Debug.LogError(AssetService.GetInstance().gameObject);
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
