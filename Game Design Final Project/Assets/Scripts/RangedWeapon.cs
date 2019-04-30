using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangedWeapon : Weapon {

	public Projectile projectile;
//	public float cooldown = 1f;
//	float cooldownTimer = 0f;
	public bool multiShot = false;

	const float MULTI_SPREAD = 10f;

	public override void use(Actor actor, int opponentLayer) {
		ammo -= 1;
		Player.instance.updateAmmo();
		if (multiShot) {
			GameObject projectileObject;
			projectileObject = Instantiate(projectile.gameObject, getProjectileOrigin(actor, -MULTI_SPREAD), Quaternion.identity);
			projectileObject.GetComponent<Projectile>().initailize(Quaternion.Euler(0, 0, -MULTI_SPREAD) * actor.getForward(), opponentLayer);
			projectileObject = Instantiate(projectile.gameObject, getProjectileOrigin(actor), Quaternion.identity);
			projectileObject.GetComponent<Projectile>().initailize(actor.getForward(), opponentLayer);
			projectileObject = Instantiate(projectile.gameObject, getProjectileOrigin(actor, MULTI_SPREAD), Quaternion.identity);
			projectileObject.GetComponent<Projectile>().initailize(Quaternion.Euler(0, 0, MULTI_SPREAD) * actor.getForward(), opponentLayer);
		}
		else {
			GameObject projectileObject = Instantiate(projectile.gameObject, getProjectileOrigin(actor), Quaternion.identity);
			projectileObject.GetComponent<Projectile>().initailize(actor.getForward(), opponentLayer);
		}
		AudioManager.playSFX(AudioManager.instance.slingUseSfx);
//		cooldownTimer = cooldown;
	}

//	void Update() {
//		if (cooldownTimer > 0) {
//			cooldownTimer -= Time.deltaTime;
//		}
//	}

	public override bool hasClearShot(Actor actor, int opponentLayer) {
		/*
		RaycastHit2D hitInfo = Physics2D.CircleCast(getProjectileOrigin(actor), projectile.radius, actor.getForward(), 10f, 1 << opponentLayer);//, ~(1 << actor.gameObject.layer));
		if (hitInfo.collider != null) {
			Debug.Log(hitInfo.collider.name);
		}
		if (hitInfo.collider != null && hitInfo.collider.gameObject.layer == opponentLayer) {
			return true;
		}
		return false;
		*/
		Vector3 toPlayer = Player.instance.transform.position - actor.transform.position;
		return (Vector3.Angle(actor.getForward(), toPlayer) < 45f);
	}

	Vector3 getProjectileOrigin(Actor actor) {
		return actor.transform.position + (actor.getForward() * (actor.radius + projectile.radius + 0.2f));
	}

	Vector3 getProjectileOrigin(Actor actor, float spread) {
		return actor.transform.position + ((Quaternion.Euler(0, 0, spread) * actor.getForward()) * (actor.radius + projectile.radius + 0.2f));
	}

}
