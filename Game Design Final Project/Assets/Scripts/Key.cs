using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Key : MonoBehaviour {

	void Awake() {
		if (gameObject.layer != Layers.WEAPON) {
			Debug.LogWarning("The key '" + name + "' is not set to the Weapon layer");
		}
	}

}
