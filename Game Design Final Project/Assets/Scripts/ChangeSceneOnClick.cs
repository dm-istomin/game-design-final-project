using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeSceneOnClick : MonoBehaviour {
	public void clickedButton(string sceneName) {
		SceneManager.LoadScene(sceneName);
	}

  public void ReturnToGame(bool isLoss) {
    if (isLoss) {
      WorldGrid.dungeonsBeaten = 0;
    }
		SceneManager.LoadScene("Playtest");
  }
}
