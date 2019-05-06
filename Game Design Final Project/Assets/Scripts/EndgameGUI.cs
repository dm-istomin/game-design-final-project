using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class EndgameGUI : MonoBehaviour {
  public Text detail;

  void Start() {
    detail.text = WorldGrid.dungeonsBeaten + " floors cleared.";
  }

  public void SetDetail (string message) {
    detail.text = message;
  }
}
