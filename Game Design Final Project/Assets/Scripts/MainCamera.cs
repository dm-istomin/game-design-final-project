using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainCamera : MonoBehaviour {

	Vector3 offset = new Vector3(0, 0, -10);

	void LateUpdate() {
		transform.position = Player.instance.transform.position + offset;
	}

}
