using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Player : Actor {

	public static Player instance;

	public const int MAX_HP = 3;

	const float TOP_SPEED = 5f;
//	const float ACCELERATION = 10f;
//	const float DECELERATION = 20f;
//	const float ROTATION_SPEED = 360f;
//	const float ATTACK_RANGE = 0.5f;
//	const float TOP_SPEED_SQR = TOP_SPEED * TOP_SPEED;

	public Image weaponUI;

	public int keys {
		get {
			return _keys;
		}
		set {
			_keys = value;
			PlayerGUI.updateKeysDisplay();
		}
	}
	int _keys;

	new void Awake() {
		base.Awake();
		instance = this;
		if (gameObject.layer != Layers.PLAYER) {
			Debug.LogWarning("The player is not set to the Player layer");
		}
		hp = MAX_HP;
	}

	new void Update() {
		if (Dialog.shown) {
			if (Input.GetButtonDown("Attack")) {
				Dialog.close();
			}
			return;
		}
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
			attack();
		}
		else if (Input.GetButtonDown("Pickup")) {
			RaycastHit2D hitInfo = Physics2D.CircleCast(transform.position, radius - 0.05f, getForward(), 0.2f, ~(1 << Layers.PLAYER));
			if (hitInfo.collider != null) {
				if (hitInfo.collider.gameObject.layer == Layers.WEAPON) {
					if (hitInfo.collider.GetComponentInParent<Weapon>() != null) {
						// Picked up weapon
						if (weapon != null) {
							weapon.gameObject.SetActive(true);
							weapon.transform.position = transform.position + (getForward() * (radius + weapon.radius));
						}
						weapon = hitInfo.collider.GetComponentInParent<Weapon>();
						weapon.gameObject.SetActive(false);
						weaponUI.enabled = true;
						weaponUI.sprite = weapon.GetComponent<SpriteRenderer>().sprite;
					}
					else {
						// Picked up key
						keys += 1;
						Destroy(hitInfo.collider.gameObject);
					}
				}
			}
		}
	}

	public override void takeDamage(int damage) {
		hp -= damage;
		if (hp <= 0) {
			SceneManager.LoadScene("Game Over");
		}
		else {
			animator.SetInteger("HP", hp);
			animator.SetTrigger("Flinch");
			hasControl = false;
			PlayerGUI.updateHealthDisplay();
		}
	}

	void OnCollisionEnter2D(Collision2D collision) {
		if (collision.gameObject.tag == "LockedDoor") {
			if (keys > 0) {
				keys--;
				collision.gameObject.SetActive(false);
			}
		}
	}
}
