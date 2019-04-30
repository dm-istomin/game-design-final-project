using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Breakable : MonoBehaviour {

	void Awake() {
		if (gameObject.layer != Layers.ENEMY) {
			Debug.LogWarning("The breakable object '" + name + "' is not set to the Enemy layer");
		}
	}

	public void takeDamage() {
		Destroy(gameObject);
	}

}
