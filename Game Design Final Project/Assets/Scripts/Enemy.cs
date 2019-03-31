using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour {

	public enum State { Wait, Wander, Chase }

	const float FIELD_OF_VIEW_ANGLE = 60f;
	const float ALLOWABLE_DESTINATION_ERROR = 0.25f;
	const float ALLOWABLE_DESTINATION_ERROR_SQR = ALLOWABLE_DESTINATION_ERROR * ALLOWABLE_DESTINATION_ERROR;

	[SerializeField] float spotDistance = 5f;
	[SerializeField] float trackDistance = 8f;
	[SerializeField] float waitIntervalLow = 0f;
	[SerializeField] float waitIntervalHigh = 5f;
	[SerializeField] float wanderSpeed = 2f;
	[SerializeField] float chaseSpeed = 4f;

	[SerializeField] bool debugVision = false;


	float spotDistanceSqr;
	float trackDistanceSqr;
	NavMeshAgent agent;
	float waitTimer;

	State state {
		get {
			return _state;
		}
		set {
			if (value == State.Wait) {
				waitTimer = Random.Range(waitIntervalLow, waitIntervalHigh);
			}
			else if (value == State.Wander) {
				agent.speed = wanderSpeed;
				Vector2 rand = Random.insideUnitCircle.normalized * Random.Range(8f, 20f);
				agent.destination = transform.position + new Vector3(rand.x, 0, rand.y);
			}
			else if (value == State.Chase) {
				agent.speed = chaseSpeed;
				agent.destination = Player.instance.transform.position;
			}
			_state = value;
		}
	}
	private State _state;

	void Awake() {
		agent = GetComponent<NavMeshAgent>();
		if (gameObject.layer != Layers.ENEMY) {
			Debug.LogWarning("The enemy '" + name + "' is not set to the Enemy layer");
		}
		spotDistanceSqr = spotDistance * spotDistance;
		trackDistanceSqr = trackDistance * trackDistance;
		state = State.Wait;
	}

	void Update() {
		transform.GetChild(1).gameObject.SetActive(debugVision && state != State.Chase);
		transform.GetChild(2).gameObject.SetActive(debugVision && state == State.Chase);
		if (debugVision) {
			if (state == State.Chase) {
				transform.GetChild(2).localScale = new Vector3(2f * trackDistance, 0.5f, 2f * trackDistance);
			}
			else {
				Mesh m = transform.GetChild(1).GetComponent<MeshFilter>().mesh;
				m.vertices = new Vector3[] {
					new Vector3(0, 0, 0),
					Vector3.up * spotDistance,
					Quaternion.Euler(0, 0, -FIELD_OF_VIEW_ANGLE) * Vector3.up * spotDistance,
					Quaternion.Euler(0, 0, FIELD_OF_VIEW_ANGLE) * Vector3.up * spotDistance,
				};
			}
		}

		if (Time.timeScale == 0f) {
			return; // Skip if the game is paused
		}
		if (state == State.Wait) {
			if (playerSpotted()) {
				state = State.Chase;
			}
			else {
				waitTimer -= Time.deltaTime;
				if (waitTimer <= 0) {
					state = State.Wander;
				}
			}
		}
		else if (state == State.Wander) {
			if (playerSpotted()) {
				state = State.Chase;
			}
			else if (reachedDestination()) {
				state = State.Wait;
			}
		}
		else if (state == State.Chase) {
			if (playerTracked()) {
				// Updates the destination if the player moved too much from the destination
				if ((agent.destination - Player.instance.transform.position).sqrMagnitude > ALLOWABLE_DESTINATION_ERROR_SQR) {
					agent.destination = Player.instance.transform.position;
				}
				// Checking if he should attack
			}
			else if (reachedDestination()) {
				// Arrived at the last known location of the player and he's still out of sight
				state = State.Wander;
			}

//			else {
//				if (!attackAnimationPlaying && hasClearShot(getEnemyData("Range"))) {
//					StartCoroutine(attackAnimation(getEnemyData("Damage Interval")));
//				}
//				// Setting stopping distance for mummies
//				if (enemy == Enemy.MUMMY) {
//					if (agent.remainingDistance < 10f) {
//						agent.speed = 0.05f;
//					}
//					else {
//						agent.speed = getEnemyData("Hi Speed");
//					}
//				}
//			}
		}
	}

	bool playerSpotted() {
		Vector3 toPlayer = Player.instance.transform.position - transform.position;
		if (Vector3.Angle(transform.forward, toPlayer) < FIELD_OF_VIEW_ANGLE) {
			if (toPlayer.sqrMagnitude < spotDistanceSqr) {
				float manhattanDistanceToPlayer = Mathf.Abs(toPlayer.x) + Mathf.Abs(toPlayer.y) + Mathf.Abs(toPlayer.z);
				RaycastHit hitInfo;
				if (Physics.Raycast(transform.position, toPlayer, out hitInfo, manhattanDistanceToPlayer, ~(1 << Layers.ENEMY))) {
					if (hitInfo.collider.gameObject.layer == Layers.PLAYER) {
						return true;
					}
				}
			}
		}
		return false;
	}

	bool playerTracked() {
		Vector3 toPlayer = Player.instance.transform.position - transform.position;
		if (toPlayer.sqrMagnitude < trackDistanceSqr) {
			float manhattanDistanceToPlayer = Mathf.Abs(toPlayer.x) + Mathf.Abs(toPlayer.y) + Mathf.Abs(toPlayer.z);
			RaycastHit hitInfo;
			if (Physics.Raycast(transform.position, toPlayer, out hitInfo, manhattanDistanceToPlayer, ~(1 << Layers.ENEMY))) {
				if (hitInfo.collider.gameObject.layer == Layers.PLAYER) {
					return true;
				}
			}
		}
		return false;
	}
	
//	bool hasClearShot(float range) {
//		Vector3 toPlayer = player.position - transform.position;
//		RaycastHit hitInfo;
//		if (Physics.SphereCast(transform.position, 0.2f, toPlayer, out hitInfo, range + capsuleRadius)) {
//			if (hitInfo.collider.gameObject.layer == 8) {
//				return true;
//			}
//		}
//		if (Physics.Raycast(transform.position, toPlayer, out hitInfo, range + capsuleRadius)) {
//			if (hitInfo.collider.gameObject.layer == 8) {
//				return true;
//			}
//		}
//		return false;
//		//		if (Physics.SphereCast(transform.position, 0.2f, toPlayer, out hitInfo, range + capsuleRadius, 1 << 8)) {
//		//			return true;
//		//		}
//		//		if (Physics.Raycast(transform.position, toPlayer, out hitInfo, range + capsuleRadius, 1 << 8)) {
//		//			return true;
//		//		}
//		//		return false;
//	}

	bool reachedDestination() {
		return agent.remainingDistance == Mathf.Infinity || agent.remainingDistance <= agent.stoppingDistance || isFacingANavMeshObstacle();
	}
	
	bool isFacingANavMeshObstacle() {
		RaycastHit[] hits = Physics.RaycastAll(transform.position, transform.forward, agent.radius + 0.25f);
		foreach (RaycastHit hit in hits) {
			if (hit.transform.GetComponentInChildren<NavMeshObstacle>() != null) {
				return true;
			}
		}
		return false;
	}

}
