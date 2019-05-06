using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Player : Actor {

	public static Player instance;

	public const int MAX_HP = 3;

	const float INVINCIBLE_TIME = 0.4f;

	const float TOP_SPEED = 5f;
//	const float ACCELERATION = 10f;
//	const float DECELERATION = 20f;
//	const float ROTATION_SPEED = 360f;
//	const float ATTACK_RANGE = 0.5f;
//	const float TOP_SPEED_SQR = TOP_SPEED * TOP_SPEED;

	[SerializeField] AnimatorOverrideController unarmedControllerUp;
	[SerializeField] AnimatorOverrideController unarmedControllerRight;
	[SerializeField] AnimatorOverrideController unarmedControllerDown;
	[SerializeField] AnimatorOverrideController meleeControllerUp;
	[SerializeField] AnimatorOverrideController meleeControllerRight;
	[SerializeField] AnimatorOverrideController meleeControllerDown;
	[SerializeField] AnimatorOverrideController rangedControllerUp;
	[SerializeField] AnimatorOverrideController rangedControllerRight;
	[SerializeField] AnimatorOverrideController rangedControllerDown;
	[SerializeField] AnimatorOverrideController invisControllerUp;
	[SerializeField] AnimatorOverrideController invisControllerRight;
	[SerializeField] AnimatorOverrideController invisControllerDown;

	float invincibleTimer = 0f;

	public Image weaponUI;
	public Text ammoUI;

	Animation weaponUIAnimation;

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
		Enemy.resetNumAlertedEnemies();
		Breakable.potionsProvided = 0;
		instance = this;
		if (gameObject.layer != Layers.PLAYER) {
			Debug.LogWarning("The player is not set to the Player layer");
		}
		weaponUIAnimation = weaponUI.GetComponentInParent<Animation>();
		hp = MAX_HP;
		updateAmmo();
		updateOverrideControllerType();
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
		if (invincibleTimer > 0) {
			invincibleTimer -= Time.deltaTime;
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
//		else if (Input.GetButtonDown("Pickup")) {
			RaycastHit2D hitInfo = Physics2D.CircleCast(transform.position, radius - 0.05f, getForward(), 0.2f, ~(1 << Layers.PLAYER));
			if (hitInfo.collider != null) {
				if (hitInfo.collider.gameObject.layer == Layers.WEAPON) {
					if (hitInfo.collider.GetComponent<Weapon>() != null) {
						// Picked up weapon
						if (weapon != null) {
							unhide();
							if (weapon is InvisibilityRing && weapon.ammo <= 0) {
								destroyWeapon();
							}
							else {
								weapon.gameObject.SetActive(true);
								weapon.transform.position = transform.position + (getForward() * (radius + weapon.radius));
							}
						}
						weapon = hitInfo.collider.GetComponent<Weapon>();
						weapon.gameObject.SetActive(false);
						weaponUI.enabled = true;
						weaponUI.sprite = weapon.GetComponent<SpriteRenderer>().sprite;
						weaponUIAnimation.Play();
						updateAmmo();
						updateOverrideControllerType();
						AudioManager.playSFX(AudioManager.instance.pickupSfx);
						return;
					}
					else if (hitInfo.collider.GetComponent<Item>() != null) {
						// Picked up Item
						hitInfo.collider.GetComponent<Item>().use();
						Destroy(hitInfo.collider.gameObject);
						return;
					}
					else {
						// Picked up key
						AudioManager.playSFX(AudioManager.instance.keySfx);
						keys += 1;
						Destroy(hitInfo.collider.gameObject);
						return;
					}
				}
			}
			attack();
		}
	}

	void updateOverrideControllerType() {
		if (weapon == null) {
			// Unarmed
			controllerUp = unarmedControllerUp;
			controllerRight = unarmedControllerRight;
			controllerDown = unarmedControllerDown;
		}
		else if (weapon is MeleeWeapon) {
			// Melee Weapon
			controllerUp = meleeControllerUp;
			controllerRight = meleeControllerRight;
			controllerDown = meleeControllerDown;
		}
		else if (weapon is RangedWeapon) {
			// Ranged Weapon
			controllerUp = rangedControllerUp;
			controllerRight = rangedControllerRight;
			controllerDown = rangedControllerDown;
		}
		else {
			// Invisibility Ring
			controllerUp = invisControllerUp;
			controllerRight = invisControllerRight;
			controllerDown = invisControllerDown;
		}
		updateOverrideController();
	}

	public void updateAmmo() {
		if (weapon == null) {
			ammoUI.text = "";
		}
		else {
			if (weapon.ammo == 0 && !(weapon is InvisibilityRing)) {
				destroyWeapon();
			}
			else if (weapon.ammo >= 0) {
				ammoUI.text = "x" + weapon.ammo.ToString();
			}
			else {
				ammoUI.text = "";
			}
		}
	}

	public void destroyWeapon() {
		Destroy(weapon.gameObject);
		weapon = null;
		weaponUI.enabled = false;
		ammoUI.text = "";
		updateOverrideControllerType();
	}

	public override void takeDamage(int damage) {
		if (invincibleTimer > 0) {
			return;
		}
		invincibleTimer = INVINCIBLE_TIME;
		hp -= damage;
		AudioManager.playSFX(AudioManager.instance.hurtSfx);
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
				AudioManager.playSFX(AudioManager.instance.openDoorSfx);
				collision.gameObject.SetActive(false);
			}
		}
	}
}
