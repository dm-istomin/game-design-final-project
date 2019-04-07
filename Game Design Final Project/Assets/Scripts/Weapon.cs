using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour {

	[HideInInspector]
	public float radius = 0.5f;

	public int damage = 1;
	public float range = 0.5f;

	void Awake() {
		if (gameObject.layer != Layers.WEAPON) {
			Debug.LogWarning("The weapon '" + name + "' is not set to the Weapon layer");
		}
//		radius = GetComponentInChildren<Collider>().transform.lossyScale.x / 2f;
	}

}
