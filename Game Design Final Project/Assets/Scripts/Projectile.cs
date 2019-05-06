using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour {
	
	public int damage = 1;
	public float speed = 6f;

	[HideInInspector]
	public float radius;

	new Rigidbody2D rigidbody;
	int opponentLayer = -1;
	Vector3 originalVelocity;

	void Awake() {
		rigidbody = GetComponent<Rigidbody2D>();
		radius = GetComponent<CircleCollider2D>().radius;
	}

	public void initailize(Vector3 direction, int opponentLayer) {
		rigidbody.velocity = direction.normalized * speed;
		originalVelocity = rigidbody.velocity;
		this.opponentLayer = opponentLayer;
	}
	
	void OnCollisionEnter2D(Collision2D c) {
		if (c.gameObject.layer == Layers.PROJECTILE) {
			return;
		}
		if (c.gameObject.layer == opponentLayer && opponentLayer != -1) {
			Breakable b = c.gameObject.GetComponent<Breakable>();
			if (b == null) {
				Actor enemy = c.gameObject.GetComponent<Actor>();
				enemy.takeDamage(damage);
				FXManager.instance.playFX(FXManager.instance.hitFXPrefab, enemy.transform.position, originalVelocity);
			}
			else {
				b.takeDamage();
				FXManager.instance.playFX(FXManager.instance.hitFXPrefab, b.transform.position, originalVelocity);
			}
		}
		Destroy(gameObject);
	}



}
