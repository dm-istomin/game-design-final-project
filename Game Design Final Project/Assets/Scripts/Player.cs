using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {

	public static Player instance;

	new Rigidbody rigidbody;

	float TOP_SPEED = 5f;
	const float ACCELERATION = 15f;
	const float DECELERATION = 15f;

	void Awake() {
		instance = this;
		rigidbody = GetComponent<Rigidbody>();
	}

	void Start() {
		
	}
	
	void Update() {
		movementControls();
	}

	void movementControls() {
		int x = 0;
		int y = 0;
		if (Input.GetKey(KeyCode.W)) {
			y += 1;
		}
		if (Input.GetKey(KeyCode.S)) {
			y -= 1;
		}
		if (Input.GetKey(KeyCode.D)) {
			x += 1;
		}
		if (Input.GetKey(KeyCode.A)) {
			x -= 1;
		}
		Vector2 dir = new Vector2(x, y);
		float accel;
		if (x != 0 || y != 0) {
			dir.Normalize();
			accel = ACCELERATION;
		}
		else {
			accel = DECELERATION;
		}
		rigidbody.velocity = Vector3.MoveTowards(rigidbody.velocity, dir * TOP_SPEED, accel * Time.deltaTime);
	}

}
