using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Room : MonoBehaviour {
	public List<RoomConnector> exits;
	public Tilemap floor;
	public GameObject easyEnemies;
	public GameObject mediumEnemies;
	public GameObject hardEnemies;

	public void ActivateEnemies(string difficulty) {
		if (difficulty == "easy" && easyEnemies) {
			easyEnemies.SetActive(true);
		} else if (difficulty == "medium" && mediumEnemies) {
			mediumEnemies.SetActive(true);
		} else if (difficulty == "hard" && hardEnemies) {
			hardEnemies.SetActive(true);
		}
	}
}
