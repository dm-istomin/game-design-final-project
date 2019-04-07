using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class WorldGrid : MonoBehaviour {
	public List<Room> mainRoomPrefabs;
	public List<Room> corridorPrefabs;
	public int numRoomsToGenerate = 10;

	List<Room> generatedRooms = new List<Room>();
	List<RoomConnector> availableSpaces = new List<RoomConnector>();

	void GenerateRandomRoom() {
		RoomConnector space = availableSpaces[Random.Range(0, availableSpaces.Count)];

		Room room = Instantiate(mainRoomPrefabs[Random.Range(0, mainRoomPrefabs.Count)]);
		room.transform.SetParent(transform);

		bool isAligned = false;
		RoomConnector entrance = null;

		foreach(RoomConnector conn in room.exits) {
			room.transform.position = new Vector3(space.transform.position.x, space.transform.position.y, 0f);
			room.transform.position += conn.transform.localPosition;

			bool roomPlaced = TryAligning(space, room);

			if (roomPlaced == true) {
				//Debug.Log("TryAligning() worked!");
				isAligned = true;
				room.transform.position = new Vector3(
					Mathf.Round(room.transform.position.x),
					Mathf.Round(room.transform.position.y),
					Mathf.Round(room.transform.position.z)
				);
				entrance = conn;
				generatedRooms.Add(room);
				break;
			} else {
				//Debug.Log("TryAligning() failed!");
			}
		}

		if (isAligned) {
			foreach (RoomConnector conn in room.exits) { availableSpaces.Add(conn); }
			availableSpaces.Remove(space);
			availableSpaces.Remove(entrance);
		} else {
			Destroy(room.gameObject);
		}
	}

	bool CheckOverlap(Room a, Room b) {
		/*
		Debug.Log("a: " + a.transform.position);
		Debug.Log("a.floor.cellBounds.xMax: " + (a.transform.position.x + a.floor.cellBounds.xMax));
		Debug.Log("a.floor.cellBounds.xMin: " + (a.transform.position.x + a.floor.cellBounds.xMin));
		Debug.Log("b.floor.cellBounds.xMax: " + (b.transform.position.x + a.floor.cellBounds.xMax));
		Debug.Log("b.floor.cellBounds.xMin: " + (b.transform.position.x + a.floor.cellBounds.xMin));
		Debug.Log("b: " + b.transform.position);
		*/

		float xMaxA = a.transform.position.x + a.floor.cellBounds.xMax;
		float xMinA = a.transform.position.x + a.floor.cellBounds.xMin;
		float yMaxA = a.transform.position.y + a.floor.cellBounds.yMax;
		float yMinA = a.transform.position.y + a.floor.cellBounds.yMin;

		float xMaxB = b.transform.position.x + b.floor.cellBounds.xMax;
		float xMinB = b.transform.position.x + b.floor.cellBounds.xMin;
		float yMaxB = b.transform.position.y + b.floor.cellBounds.yMax;
		float yMinB = b.transform.position.y + b.floor.cellBounds.yMin;

		if (xMinA >= xMaxB || xMinB >= xMaxA) {
			//Debug.Log("xMinA > xMaxB || xMinB > xMaxA: " + xMinA + " > " + xMaxB + " || " + xMinB + " > " + xMaxA);
			return false;
		}

		if (yMinA >= yMaxB || yMinB >= yMaxA) {
			//Debug.Log("yMinA > yMaxB || yMinB > yMaxA: " + yMinA + " > " + yMaxB + " || " + yMinB + " > " + yMaxA);
			return false;
		}

		return true;
	}

	bool TryAligning(RoomConnector emptySpace, Room newRoom) {
		Vector3 originalPosition = new Vector3(
			newRoom.transform.position.x,
			newRoom.transform.position.y,
			newRoom.transform.position.z
		);

		Room prevRoom = emptySpace.transform.parent.gameObject.GetComponent<Room>();

		newRoom.transform.position = originalPosition + new Vector3(1f, 0, 0);

		if (!CheckOverlap(newRoom, prevRoom)) {
			//Debug.Log("+1 x");
			return true;
		}

		newRoom.transform.position = originalPosition + new Vector3(-1f, 0, 0);

		if (!CheckOverlap(newRoom, prevRoom)) {
			//Debug.Log("-1 x");
			return true;
		}

		newRoom.transform.position = originalPosition + new Vector3(0, 1f, 0);

		if (!CheckOverlap(newRoom, prevRoom)) {
			// Debug.Log("+1 y");
			return true;
		}

		newRoom.transform.position = originalPosition + new Vector3(0, -1f, 0);

		if (!CheckOverlap(newRoom, prevRoom)) {
			// Debug.Log("-1 y");
			return true;
		}

		newRoom.transform.position = originalPosition;
		return false;
	}

	void RemoveDuplicateRooms() {
		Room duplicateRoom = null;
		Debug.Log("before delete: " + generatedRooms.Count);

		for (int i = 0; i < generatedRooms.Count; i++) {
			for (int j = 0; j < generatedRooms.Count; j++) {
				if (i != j) {
					if (generatedRooms[i].transform.position == generatedRooms[j].transform.position) {
						duplicateRoom = generatedRooms[i];
					}
				}
			}
		}

		if (duplicateRoom != null) {
			foreach (RoomConnector conn in duplicateRoom.exits) {
				availableSpaces.Remove(conn);
				Destroy(conn.gameObject);
			}
			generatedRooms.Remove(duplicateRoom);
			Destroy(duplicateRoom.gameObject);
		}
		Debug.Log("after delete: " + generatedRooms.Count);
	}

	void Start() {
		Room startingRoom = Instantiate(mainRoomPrefabs[0]);
		startingRoom.transform.SetParent(transform);
		generatedRooms.Add(startingRoom);

		foreach (RoomConnector conn in startingRoom.exits) {
			availableSpaces.Add(conn);
		}

		while (generatedRooms.Count < numRoomsToGenerate) {
			RemoveDuplicateRooms();
			GenerateRandomRoom();
		}
	}
}
