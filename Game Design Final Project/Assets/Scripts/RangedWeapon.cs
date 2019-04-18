using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangedWeapon : Weapon {

	public Projectile projectile;
//	public float cooldown = 1f;
//	float cooldownTimer = 0f;

	public override void use(Actor actor, int opponentLayer) {
		ammo -= 1;
		Player.instance.updateAmmo();
		GameObject projectileObject = Instantiate(projectile.gameObject, getProjectileOrigin(actor), Quaternion.identity);
		projectileObject.GetComponent<Projectile>().initailize(actor.getForward(), opponentLayer);
//		cooldownTimer = cooldown;
	}

//	void Update() {
//		if (cooldownTimer > 0) {
//			cooldownTimer -= Time.deltaTime;
//		}
//	}

	public override bool hasClearShot(Actor actor, int opponentLayer) {
		RaycastHit2D hitInfo = Physics2D.CircleCast(getProjectileOrigin(actor), projectile.radius, actor.getForward(), 10f);
		if (hitInfo.collider != null && hitInfo.collider.gameObject.layer == opponentLayer) {
			return true;
		}
		return false;
	}

	Vector3 getProjectileOrigin(Actor actor) {
		return actor.transform.position + (actor.getForward() * (actor.radius + projectile.radius + 0.2f));
	}

}
