using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Actor : MonoBehaviour {

	protected new Rigidbody2D rigidbody;
	protected Animator animator;
	SpriteRenderer spriteRenderer;

	protected int facing = Facing.Down;

	protected int hp = 3;
	protected bool hasControl = true;

	protected float radius = 0.5f;

	public Weapon weapon;

	protected void Awake() {
		rigidbody = GetComponent<Rigidbody2D>();
		animator = GetComponent<Animator>();
		spriteRenderer = GetComponent<SpriteRenderer>();
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

	public void takeDamage(int damage) {
		hp -= damage;
		animator.SetInteger("HP", hp);
		animator.SetTrigger("Flinch");
		hasControl = false;
	}

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
		RaycastHit2D[] hitInfos = Physics2D.CircleCastAll(transform.position, radius, getForward(), weapon.range, 1 << Layers.ENEMY);
		foreach (RaycastHit2D hit in hitInfos) {
			hit.collider.gameObject.GetComponent<Enemy>().takeDamage(weapon.damage);
		}
	}

}
