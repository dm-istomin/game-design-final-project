using System;
﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class WorldGrid : MonoBehaviour {
	public List<Room> mainRoomPrefabs;
	public Room xAxisCorridor;
	public Room yAxisCorridor;
	public int numRoomsToGenerate = 10;
	public int maxIterations = 10000;

	List<Room> generatedRooms = new List<Room>();
	List<RoomConnector> availableSpaces = new List<RoomConnector>();

	bool IsOverlappingWithOtherRooms(Room room) {
		foreach (Room placedRoom in generatedRooms) {
			bool isOverlapping = IsRoomOverlapping(room, placedRoom);

			if (isOverlapping) {
				return true;
			}
		}
		return false;
	}

	bool IsDoorPairAligned(RoomConnector door1, RoomConnector door2) {
		if (door1.transform.position.x == door2.transform.position.x) {
			if (Mathf.Abs(door1.transform.position.y - door2.transform.position.y) == 2f) {
				return true;
			}
		}
		if (door1.transform.position.y == door2.transform.position.y) {
			if (Mathf.Abs(door1.transform.position.x - door2.transform.position.x) == 2f) {
				return true;
			}
		}
		return false;
	}

	bool AreDoorsAligned(Room room1, Room room2) {
		foreach (RoomConnector exit1 in room1.exits) {
			foreach (RoomConnector exit2 in room2.exits) {
				if (IsDoorPairAligned(exit1, exit2)) { return true; }
			}
		}
		return false;
	}

	void GenerateRandomRoom() {
		RoomConnector space = availableSpaces[UnityEngine.Random.Range(0, availableSpaces.Count)];

		Room room = Instantiate(mainRoomPrefabs[UnityEngine.Random.Range(0, mainRoomPrefabs.Count)]);
		room.transform.SetParent(transform);

		bool isAligned = false;
		RoomConnector entrance = null;

		List<RoomConnector> shuffledExits = new List<RoomConnector>();
		System.Random rand = new System.Random();

		for (int i = 0; i < room.exits.Count; i++) {
			shuffledExits.Add(room.exits[rand.Next(0, room.exits.Count)]);
		}

		foreach(RoomConnector conn in shuffledExits) {
			room.transform.position = new Vector3(
				space.transform.position.x,
				space.transform.position.y,
				0f
			);

			bool roomPlaced = TryAligning(space, room);

			if (roomPlaced == true) {
					isAligned = true;
					room.transform.position = new Vector3(
						Mathf.Round(room.transform.position.x),
						Mathf.Round(room.transform.position.y),
						Mathf.Round(room.transform.position.z)
					);
					entrance = conn;
					generatedRooms.Add(room);
					break;
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

	bool IsRoomOverlapping(Room a, Room b) {
		float xMaxA = a.transform.position.x + a.floor.cellBounds.xMax;
		float xMinA = a.transform.position.x + a.floor.cellBounds.xMin;
		float yMaxA = a.transform.position.y + a.floor.cellBounds.yMax;
		float yMinA = a.transform.position.y + a.floor.cellBounds.yMin;

		float xMaxB = b.transform.position.x + b.floor.cellBounds.xMax;
		float xMinB = b.transform.position.x + b.floor.cellBounds.xMin;
		float yMaxB = b.transform.position.y + b.floor.cellBounds.yMax;
		float yMinB = b.transform.position.y + b.floor.cellBounds.yMin;

		if (xMinA > xMaxB || xMinB > xMaxA) {
			return false;
		}

		if (yMinA > yMaxB || yMinB > yMaxA) {
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

		for (float x = 0f; x <= 20f; x += 0.5f) {
			for (float y = 0f; y <= 20f; y += 0.5f) {

				newRoom.transform.position = originalPosition + new Vector3(x, y, 0);

				if (AreDoorsAligned(newRoom, prevRoom)) {
					if (!IsOverlappingWithOtherRooms(newRoom)) {
						return true;
					}
				}

				newRoom.transform.position = originalPosition + new Vector3(-x, y, 0);

				if (AreDoorsAligned(newRoom, prevRoom)) {
					if (!IsOverlappingWithOtherRooms(newRoom)) { return true; }
				}

				newRoom.transform.position = originalPosition + new Vector3(x, -y, 0);

				if (AreDoorsAligned(newRoom, prevRoom)) {
					if (!IsOverlappingWithOtherRooms(newRoom)) { return true; }
				}
				newRoom.transform.position = originalPosition + new Vector3(-x, -y, 0);

				if (AreDoorsAligned(newRoom, prevRoom)) {
					if (!IsOverlappingWithOtherRooms(newRoom)) { return true; }
				}
			}
		}

		return false;
	}

	void Start() {
		int numIterations = 0;

		Debug.Log("Generating initial room...");
		Room startingRoom = Instantiate(mainRoomPrefabs[0]);
		startingRoom.transform.SetParent(transform);
		generatedRooms.Add(startingRoom);

		foreach (RoomConnector conn in startingRoom.exits) {
			availableSpaces.Add(conn);
		}

		Debug.Log("Populating rooms...");
		while (generatedRooms.Count < numRoomsToGenerate && numIterations < maxIterations) {
			Debug.Log("Generating random room, iteration #" + numIterations);
			GenerateRandomRoom();
			numIterations++;
		}

	Debug.Log("Toggling doors between rooms and adding corridors...");
		// Hide walls for doors
		for (int i = 0; i < generatedRooms.Count; i++) {
			for (int j = 0; j < generatedRooms.Count; j++) {
				if (i != j) {
					Room room1 = generatedRooms[i];
					Room room2 = generatedRooms[j];

					foreach (RoomConnector r1 in room1.exits) {
						foreach (RoomConnector r2 in room2.exits) {
							if (IsDoorPairAligned(r1, r2) && (r1.gameObject.active || r2.gameObject.active)) {
								r1.gameObject.SetActive(false);
								r2.gameObject.SetActive(false);

								Vector3 midpoint = (r1.transform.position + r2.transform.position) / 2f;

								float angleToX = Vector3.Angle(r1.transform.position - r2.transform.position, Vector3.right);
								float angleToY = Vector3.Angle(r1.transform.position - r2.transform.position, Vector3.up);

								if (angleToX == 180 || angleToX == 0) {
									Vector3 position = midpoint - new Vector3(
										0.5f,
										UnityEngine.Random.Range(0, 2) == 1 ? -0.5f : 0.5f,
										0f
									);
									Room xCorridor = Instantiate(
										xAxisCorridor,
										position,
										Quaternion.identity
									);
									xCorridor.transform.SetParent(transform);
								}

								if (angleToY == 180 || angleToY == 0) {
									Vector3 position = midpoint - new Vector3(
										UnityEngine.Random.Range(0, 2) == 1 ? -0.5f : 0.5f,
										0.5f,
										0f
									);
									Room yCorridor = Instantiate(
										yAxisCorridor,
										position,
										Quaternion.identity
									);
									yCorridor.transform.SetParent(transform);
								}
							}
						}
					}
				}
			}
		}
	}
}
