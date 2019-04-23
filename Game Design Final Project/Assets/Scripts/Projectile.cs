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

	void Awake() {
		rigidbody = GetComponent<Rigidbody2D>();
		radius = GetComponent<CircleCollider2D>().radius;
	}

	public void initailize(Vector3 direction, int opponentLayer) {
		rigidbody.velocity = direction * speed;
		this.opponentLayer = opponentLayer;
	}
	
	void OnCollisionEnter2D(Collision2D c) {
		if (c.gameObject.layer == opponentLayer && opponentLayer != -1) {
			c.gameObject.GetComponent<Actor>().takeDamage(damage);
		}
		Destroy(gameObject);
	}



}
