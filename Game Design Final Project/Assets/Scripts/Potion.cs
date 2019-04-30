using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Potion : Item {

	public int hpRecovery = 3;

	public override void use() {
		Player.instance.hp += hpRecovery;
		if (Player.instance.hp > Player.MAX_HP) {
			Player.instance.hp = Player.MAX_HP;
		}
		PlayerGUI.updateHealthDisplay();
		AudioManager.playSFX(AudioManager.instance.potionUseSfx);
	}

}
