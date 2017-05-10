using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor (typeof(BlockMap))]
public class MapEditor : Editor {

	public override void OnInspectorGUI () {
		base.OnInspectorGUI();

		BlockMap map = target as BlockMap;
		map.GenerateMap();
	}
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
