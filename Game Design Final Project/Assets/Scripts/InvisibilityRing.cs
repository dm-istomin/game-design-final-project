using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InvisibilityRing : Weapon {

	public float duration = 5f;
	Actor user;

	public override void use(Actor actor, int opponentLayer) {
		if (actor.hidden) {
			return;
		}
		ammo -= 1;
		Player.instance.updateAmmo();
		user = actor;
		user.hide();
		actor.StartCoroutine(keepHidden());
		AudioManager.playSFX(AudioManager.instance.ringUseSfx);
	}

	IEnumerator keepHidden() {
		yield return new WaitForSeconds(duration);
		if (user.weapon == this) {
			user.unhide();
			if (user.weapon.ammo == 0) {
				Player.instance.destroyWeapon();
			}
		}
	}

	public override bool hasClearShot(Actor actor, int opponentLayer) {
		return false;
	}

}
