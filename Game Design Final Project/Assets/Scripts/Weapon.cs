using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Weapon : MonoBehaviour {

	[HideInInspector]
	public float radius = 0.5f;

	public int ammo = -1;

	void Awake() {
		if (gameObject.layer != Layers.WEAPON) {
			Debug.LogWarning("The weapon '" + name + "' is not set to the Weapon layer");
		}
		radius = GetComponent<CircleCollider2D>().radius * transform.lossyScale.x;
	}

	public abstract void use(Actor actor, int opponentLayer);

	public abstract bool hasClearShot(Actor actor, int opponentLayer);

}
