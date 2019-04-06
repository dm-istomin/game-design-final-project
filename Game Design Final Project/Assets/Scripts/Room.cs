using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour {
	public float height, width;
	public List<Vector2> exits;

	void Start() {
		/*
		GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
		cube.transform.position = new Vector3(exits[0].x, exits[0].y, 0);
		*/
	}
}
