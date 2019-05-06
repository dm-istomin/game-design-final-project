using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeWeapon : Weapon {

	public int damage = 1;
	public float range = 0.5f;

	public override void use(Actor actor, int opponentLayer) {
		AudioManager.playSFX(AudioManager.instance.swordUseSfx);
		RaycastHit2D[] hitInfos = Physics2D.CircleCastAll(actor.transform.position, actor.radius, actor.getForward(), range, 1 << opponentLayer);
		foreach (RaycastHit2D hit in hitInfos) {
			Breakable b = hit.collider.gameObject.GetComponent<Breakable>();
			if (b == null) {
				Actor enemy = hit.collider.gameObject.GetComponent<Actor>();
				enemy.takeDamage(damage);
				Vector3 midPoint = (actor.transform.position + enemy.transform.position) / 2f;
				FXManager.instance.playFX(FXManager.instance.hitFXPrefab, midPoint, actor.getForward());
			}
			else {
				b.takeDamage();
				Vector3 midPoint = (actor.transform.position + b.transform.position) / 2f;
				FXManager.instance.playFX(FXManager.instance.hitFXPrefab, midPoint, actor.getForward());
			}
		}
	}

	public override bool hasClearShot(Actor actor, int opponentLayer) {
		RaycastHit2D hitInfo = Physics2D.CircleCast(actor.transform.position, actor.radius, actor.getForward(), range, 1 << opponentLayer);
		if (hitInfo.collider != null) {
			return true;
		}
		return false;
	}

}
