using System;
﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class WorldGrid : MonoBehaviour {
  public static int dungeonsBeaten = 0;
	public bool enabled = true;
	public List<Room> mainRoomPrefabs;
	public Room startingRoomPrefab;
	public Room endRoomPrefab;
	public Room xAxisCorridorPrefab;
	public Room yAxisCorridorPrefab;

  public Player player;
	public List<GameObject> itemPrefabs;
	public Key keyPrefab;

  public int seed;
	public int numRoomsToGenerate = 10;
	public int maxIterations = 50;
	public int numKeys = 3;

	List<Room> generatedRooms = new List<Room>();
	List<RoomConnector> availableSpaces = new List<RoomConnector>();
  List<GameObject> items = new List<GameObject>();
  List<Key> keys = new List<Key>();

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

  bool PlaceRoom(Room room) {
		int spaceIdx = UnityEngine.Random.Range(0, availableSpaces.Count);
    return PlaceRoom(room, spaceIdx);
  }

	bool PlaceRoom(Room room, int spaceIdx) {
		RoomConnector space = availableSpaces[spaceIdx];

		room.transform.SetParent(transform);

		bool isAligned = false;
		RoomConnector entrance = null;

		List<RoomConnector> shuffledExits = new List<RoomConnector>();
		System.Random rand; 

    if (seed == 0) {
      rand = new System.Random();
    }

    rand = new System.Random(seed);

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
      return true;
		} else {
      Destroy(room.gameObject);
      return false;
		}

	}

	void GenerateRandomRoom() {
		Room room = Instantiate(mainRoomPrefabs[UnityEngine.Random.Range(0, mainRoomPrefabs.Count)]);
		PlaceRoom(room);
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

  void Start () {
		if (!enabled) {
			return;
		}

    if (seed != 0) {
      UnityEngine.Random.seed = seed;
    }

    GenerateDungeon();
  }

	void GenerateDungeon() {
		int numIterations = 0;

		Debug.Log("Generating initial room...");
		Room startingRoom = Instantiate(startingRoomPrefab);
		startingRoom.transform.SetParent(transform);
		generatedRooms.Add(startingRoom);

		foreach (RoomConnector conn in startingRoom.exits) {
			availableSpaces.Add(conn);
		}

		Debug.Log("Generating other rooms...");
		while (generatedRooms.Count < numRoomsToGenerate && numIterations < maxIterations) {
			GenerateRandomRoom();
			numIterations++;
		}

		Debug.Log("... Finished after " + numIterations + " iterations");

		Debug.Log("Adding end room...");
    for (int i = 0; i < availableSpaces.Count; i++)  {
      Room endRoom = Instantiate(endRoomPrefab);
      bool placed = PlaceRoom(endRoom);

      if (placed) {
        Debug.Log("... end room placed.");
        break;
      }
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
										xAxisCorridorPrefab,
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
										yAxisCorridorPrefab,
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

		Debug.Log("Placing enemies...");
		string[] difficulty = {"easy", "medium", "hard"};

		foreach (Room r in generatedRooms) {
			int randomDiffIdx = UnityEngine.Random.Range(0, difficulty.Length);
			r.ActivateEnemies(difficulty[randomDiffIdx]);
		}

		Debug.Log("Placing keys...");

		List<int> keyLocations = new List<int>();

		while (keyLocations.Count < numKeys) {
			// NOTE: keys can't spawn in starting or ending room
			int keyLocation = UnityEngine.Random.Range(1, generatedRooms.Count - 1);
			if (!keyLocations.Contains(keyLocation)) {
				keyLocations.Add(keyLocation);
				Room keyRoom = generatedRooms[keyLocation];
        Vector3 keyCoords = keyRoom.transform.position;
        foreach (GameObject item in items) {
          if (item.transform.position == keyCoords) {
            keyCoords += new Vector3(UnityEngine.Random.Range(0.5f, 1f), UnityEngine.Random.Range(0.5f, 1), 0f);
          }
        }

        foreach (Key key in keys) {
          if (key.gameObject.transform.position == keyCoords) {
            keyCoords += new Vector3(UnityEngine.Random.Range(0.5f, 1f), UnityEngine.Random.Range(0.5f, 1), 0f);
          }
        }
				Key k = Instantiate(keyPrefab, keyCoords, Quaternion.identity);
        keys.Add(k);
			}
		}

		Debug.Log("Placing weapons and items...");

		foreach (GameObject itemPrefab in itemPrefabs) {
			int location = UnityEngine.Random.Range(1, generatedRooms.Count - 1);
			Room weaponRoom = generatedRooms[location];
      Vector3 weaponCoords = weaponRoom.transform.position + new Vector3(0, 1, 0);
      foreach (GameObject otherItem in items) {
        if (otherItem.transform.position == weaponCoords) {
          weaponCoords += new Vector3(UnityEngine.Random.Range(0.5f, 1f), UnityEngine.Random.Range(0.5f, 1), 0f);
        }
      }

      foreach (Key key in keys) {
        if (key.gameObject.transform.position == weaponCoords) {
          weaponCoords += new Vector3(UnityEngine.Random.Range(0.5f, 1f), UnityEngine.Random.Range(0.5f, 1), 0f);
        }
      }
			GameObject item = Instantiate(itemPrefab, weaponCoords, Quaternion.identity);
      items.Add(item);
		}

		Debug.Log("Dungeon generated!");
	}
}
