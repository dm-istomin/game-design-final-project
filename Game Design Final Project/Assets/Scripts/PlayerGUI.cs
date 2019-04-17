using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerGUI : MonoBehaviour {
	
	static PlayerGUI instance;

	[SerializeField] Transform healthContainer;
	[SerializeField] Text goldLabel;

	Image[] hearts;

	void Awake() {
		instance = this;
		hearts = healthContainer.GetComponentsInChildren<Image>();
	}

	void Start() {
		updateKeysDisplay();
		updateHealthDisplay();
	}
	
	public static void updateKeysDisplay() {
		instance.goldLabel.text = "x" + Player.instance.keys.ToString();
	}

	public static void updateHealthDisplay() {
		for (int i = 0; i < instance.hearts.Length; ++i) {
			if (i < Player.instance.hp) {
				instance.hearts[i].color = Color.white;
			}
			else {
				instance.hearts[i].color = new Color(0.25f, 0.25f, 0.25f, 1f);
			}
		}
	}

}
