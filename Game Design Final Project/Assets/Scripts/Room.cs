using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Room : MonoBehaviour {
	public List<RoomConnector> exits;
	public Tilemap floor;
}
