using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Actor {

	public static Player instance;

	const float TOP_SPEED = 5f;
	const float ACCELERATION = 10f;
	const float DECELERATION = 20f;
	const float ROTATION_SPEED = 360f;
	const float ATTACK_RANGE = 0.5f;

	const float TOP_SPEED_SQR = TOP_SPEED * TOP_SPEED;


	new void Awake() {
		base.Awake();
		instance = this;
		if (gameObject.layer != Layers.PLAYER) {
			Debug.LogWarning("The player is not set to the Player layer");
		}
	}
	
	new void Update() {
		if (hasControl) {
			movementControls();
			actionControls();
		}
		else {
			rigidbody.velocity = Vector2.zero;
		}
		base.Update();
	}

	void movementControls() {
		float x = Input.GetAxisRaw("Horizontal");
		float y = Input.GetAxisRaw("Vertical");
		if (x != 0 && y != 0) {
			if (Input.GetButtonDown("Vertical")) {
				facing = y > 0 ? Facing.Up : Facing.Down;
			}
			else if (Input.GetButtonDown("Horizontal")) {
				facing = x > 0 ? Facing.Right : Facing.Left;
			}
			if (facing == Facing.Left || facing == Facing.Right) {
				y = 0;
			}
			else {
				x = 0;
			}
		}
		else if (x != 0) {
			facing = x > 0 ? Facing.Right : Facing.Left;
		}
		else if (y != 0) {
			facing = y > 0 ? Facing.Up : Facing.Down;
		}
		animator.SetFloat("Speed", (x == 0 && y == 0) ? 0 : 1);
		rigidbody.velocity = new Vector2(x * TOP_SPEED, y * TOP_SPEED);
	}

	void actionControls() {
		if (Input.GetButtonDown("Attack")) {
			if (weapon != null) {
				attack();
			}
		}
		else if (Input.GetButtonDown("Pickup")) {
			RaycastHit2D hitInfo = Physics2D.CircleCast(transform.position, radius - 0.05f, getForward(), 0.2f, ~(1 << Layers.PLAYER));
			if (hitInfo.collider != null) {
				if (hitInfo.collider.gameObject.layer == Layers.WEAPON) {
					if (weapon != null) {
						weapon.gameObject.SetActive(true);
						weapon.transform.position = transform.position + (getForward() * (radius + weapon.radius));
					}
					weapon = hitInfo.collider.GetComponentInParent<Weapon>();
					weapon.gameObject.SetActive(false);
				}
			}
		}
	}

}
