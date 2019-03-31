using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {

	public static Player instance;

	new Rigidbody rigidbody;

	const float TOP_SPEED = 5f;
	const float ACCELERATION = 10f;
	const float DECELERATION = 20f;
	const float ROTATION_SPEED = 360f;

	const float TOP_SPEED_SQR = TOP_SPEED * TOP_SPEED;


	void Awake() {
		instance = this;
		rigidbody = GetComponent<Rigidbody>();
		if (gameObject.layer != Layers.PLAYER) {
			Debug.LogWarning("The player is not set to the Player layer");
		}
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
		Vector3 dir = new Vector3(x, 0, y);
		float accel;
		if (x != 0 || y != 0) {
			dir.Normalize();
			float dot = Vector3.Dot(rigidbody.velocity.normalized, dir);
			accel = Mathf.Lerp(DECELERATION, ACCELERATION, dot);
			transform.localRotation = Quaternion.RotateTowards(transform.localRotation, Quaternion.LookRotation(dir), ROTATION_SPEED * Time.deltaTime);
		}
		else {
			accel = DECELERATION;
		}
		rigidbody.velocity = Vector3.MoveTowards(rigidbody.velocity, dir * TOP_SPEED, accel * Time.deltaTime);
		if (rigidbody.velocity.sqrMagnitude > TOP_SPEED_SQR) {
			rigidbody.velocity = rigidbody.velocity.normalized * TOP_SPEED;
		}
	}

}
