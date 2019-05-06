using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Doom : MonoBehaviour {

	public float lifespan = 5f;

	void Start() {
		StartCoroutine(doom());
	}

	IEnumerator doom() {
		yield return new WaitForSeconds(lifespan);
		Destroy(gameObject);
	}

}
