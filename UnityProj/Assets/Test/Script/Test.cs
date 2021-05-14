using MFramework.AssetService;
using MFramework.Common;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour {

	// Use this for initialization
	void Start () {
		Log.LogE(AssetService.GetInstance().gameObject.name);
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
