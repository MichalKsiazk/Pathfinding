using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(Map))]
public class MapEditor : Editor {

	bool pressed = false;

	public override void OnInspectorGUI() {

		base.OnInspectorGUI();

		Map map = (Map)target;



		map.solidColor = EditorGUILayout.ColorField("Solid Color", map.solidColor);
		map.destinationColor = EditorGUILayout.ColorField("Destination Color", map.destinationColor);
		map.startColor = EditorGUILayout.ColorField("Start Color", map.startColor);
		map.pathColor = EditorGUILayout.ColorField("Path Color", map.pathColor);
		map.moveSpeed = EditorGUILayout.IntField("Move Speed", map.moveSpeed);
		GUILayout.BeginHorizontal ();

			if(GUILayout.Button("Generate")) {
				map.Generate ();	
			}
			if(GUILayout.Button("Reset")) {
				map.Reset ();	
				map.stepCount = 0;
				map.path = null;
			}
			if(GUILayout.Button("Clear Paths")) {
				map.ClearPaths();	
			}

		GUILayout.EndHorizontal ();
		GUILayout.BeginHorizontal ();
		if(GUILayout.Button("Pathfind")) {
			map.stepCount = 0;
			map.path = null;
			map.path = map.FindPath ();
			if(map.path != null)
				map.stepCount = map.path.Count-1;
			else
				Debug.Log ("Path not found!");
		}
		if(GUILayout.Button("Pathfind (fast)")) {
			map.path = map.FindPath ();
			if (map.path != null)
				map.DrawPath (map.path);
			else
				Debug.Log ("Path not found!");
		}
		GUILayout.EndHorizontal ();
			
	}

	void OnEnable() { 
		Map map = (Map)target;
		EditorApplication.update += map.UpdateInEditMode; 
	}
	void OnDisable() { 
		Map map = (Map)target;
		EditorApplication.update -= map.UpdateInEditMode; 
	}

	void OnSceneGUI() {

		Map map = (Map)target;


		Event guiEvent = Event.current;

		if (guiEvent.type == EventType.MouseDown && guiEvent.button == 0)
			pressed = true;
		if (guiEvent.type == EventType.MouseUp && guiEvent.button == 0)
			pressed = false;

		if (pressed) {
			Ray ray = HandleUtility.GUIPointToWorldRay (guiEvent.mousePosition);
			float z = 0;
			float dstToDrawPlane = (z - ray.origin.y) / ray.direction.z;
			Vector3 mousePos = ray.GetPoint (dstToDrawPlane);
			if(mousePos.x > 0 && mousePos.x <= map.width && mousePos.y > 0 && mousePos.y <= map.height)
				map.Select (mousePos);
		}
			
		HandleUtility.AddDefaultControl (GUIUtility.GetControlID (FocusType.Passive));
	}
		
}
