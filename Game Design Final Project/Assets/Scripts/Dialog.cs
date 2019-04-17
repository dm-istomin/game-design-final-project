using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Dialog : MonoBehaviour {

	static Dialog instance;

	public static bool shown {
		get {
			return _shown;
		}
	}
	static bool _shown;

	[SerializeField] GameObject textBox;
	[SerializeField] Text label;

	void Awake() {
		instance = this;
		close();
	}
	
	public static void close() {
		instance.textBox.SetActive(false);
		Time.timeScale = 1f;
		_shown = false;
	}

	public static void show(string message) {
		instance.textBox.SetActive(true);
		Time.timeScale = 0f;
		_shown = true;
		instance.label.text = message;
	}

}
