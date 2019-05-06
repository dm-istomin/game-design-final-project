using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FXManager : MonoBehaviour {

	public static FXManager instance;

	[SerializeField] Transform dynamicContainer;
	public GameObject hitFXPrefab;
	public GameObject potionPrefab;

	void Awake() {
		if (instance == null) {
			instance = this;
			DontDestroyOnLoad(gameObject);
		}
		else {
			Destroy(gameObject);
		}
	}

	public void playFX(GameObject prefab, Vector3 pos, Vector3 forward) {
		Quaternion rotation = Quaternion.LookRotation(forward);
		Instantiate(prefab, pos, rotation, dynamicContainer);
	}

}
