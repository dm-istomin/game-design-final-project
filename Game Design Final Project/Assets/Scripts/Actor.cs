using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Actor : MonoBehaviour {

	protected new Rigidbody2D rigidbody;
	protected Animator animator;
	SpriteRenderer spriteRenderer;

	protected int facing {
		get {
			return _facing;
		}
		set {
			if (_facing != value && 
				!((_facing == Facing.Left && value == Facing.Right) || (_facing == Facing.Right && value == Facing.Left))) {
				// Needs to change character controllers
				if (value == Facing.Up) {
					animator.runtimeAnimatorController = controllerUp;
				}
				else if (value == Facing.Down) {
					animator.runtimeAnimatorController = controllerDown;
				}
				else {
					animator.runtimeAnimatorController = controllerRight;
				}
			}
			_facing = value;
		}
	}
	int _facing;

	public int hp = 3;
	protected bool hasControl = true;

	protected float radius = 0.5f;

	public AnimatorOverrideController controllerUp;
	public AnimatorOverrideController controllerRight;
	public AnimatorOverrideController controllerDown;
	public Weapon weapon;

	protected void Awake() {
		rigidbody = GetComponent<Rigidbody2D>();
		animator = GetComponent<Animator>();
		spriteRenderer = GetComponent<SpriteRenderer>();
		facing = Facing.Down;
//		radius = GetComponentInChildren<Collider>().transform.lossyScale.x / 2f;
	}

	protected void Update() {
		spriteRenderer.flipX = (facing == Facing.Left);
	}

	protected Vector3 getForward() {
		if (facing == Facing.Up) {
			return transform.up;
		}
		else if (facing == Facing.Down) {
			return -transform.up;
		}
		else if (facing == Facing.Left) {
			return -transform.right;
		}
		else {
			return transform.right;
		}
	}

	public abstract void takeDamage(int damage);

	protected void attack() {
		animator.SetTrigger("Attacking");
		hasControl = false;
	}

	// Gets called via the Player Flinch animation
	public void doneFlinching() {
		hasControl = true;
	}

	// Gets called via the Player Attack animation
	public void doneAttacking() {
		hasControl = true;
	}

	// Gets called via the Player Attack animation
	public void applyAttackDamage() {
		if (gameObject.layer == Layers.PLAYER) {
			// Check for NPC interaction first
			RaycastHit2D[] hitInfos = Physics2D.CircleCastAll(transform.position, radius, getForward(), 0.5f, 1 << Layers.NPC);
			foreach (RaycastHit2D hit in hitInfos) {
				hit.collider.gameObject.GetComponent<NPC>().takeDamage(0);
				return;
			}
			// Check for Enemy damage
			if (weapon != null) {
				hitInfos = Physics2D.CircleCastAll(transform.position, radius, getForward(), weapon.range, 1 << Layers.ENEMY);
				foreach (RaycastHit2D hit in hitInfos) {
					hit.collider.gameObject.GetComponent<Enemy>().takeDamage(weapon.damage);
				}
			}
		}
		else {
			RaycastHit2D hitInfo = Physics2D.CircleCast(transform.position, radius, getForward(), weapon.range, 1 << Layers.PLAYER);
			if (hitInfo.collider != null) {
				hitInfo.collider.gameObject.GetComponent<Player>().takeDamage(weapon.damage);
			}
		}
	}

}
