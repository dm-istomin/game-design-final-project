using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Breakable : MonoBehaviour {

	static readonly float[] POTION_PROBABILITIES = new float[] { 0.25f, 0.175f, 0.10f };

	public static int potionsProvided;

	bool broken = false;

	public Sprite brokenSprite;

	void Awake() {
		if (gameObject.layer != Layers.ENEMY) {
			Debug.LogWarning("The breakable object '" + name + "' is not set to the Enemy layer");
		}
	}

	public void takeDamage() {
		if (!broken) {
			broken = true;
			AudioManager.playSFX(AudioManager.instance.breakSfx);
			Collider2D collider = GetComponent<Collider2D>();
			if (collider != null) {
				Destroy(collider);
			}
			StartCoroutine(doom());
			if (potionsProvided < POTION_PROBABILITIES.Length) {
				if (Random.value < POTION_PROBABILITIES[potionsProvided]) {
					potionsProvided += 1;
					Instantiate(FXManager.instance.potionPrefab, transform.position, Quaternion.identity);
				}
			}
		}
	}

	IEnumerator doom() {
		GetComponent<SpriteRenderer>().sprite = brokenSprite;
		yield return new WaitForSeconds(0.25f);
		Destroy(gameObject);
	}

}
