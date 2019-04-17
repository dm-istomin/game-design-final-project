using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetActiveOnAwake : MonoBehaviour {

	[SerializeField] GameObject obj;

	void Awake() {
		obj.SetActive(true);
	}

}
