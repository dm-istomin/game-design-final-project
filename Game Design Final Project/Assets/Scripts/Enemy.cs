using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : Actor {

	public static void resetNumAlertedEnemies() {
		_numAlertedEnemies = 0;
	}
	static int numAlertedEnemies {
		get {
			return _numAlertedEnemies;
		}
		set {
			if (_numAlertedEnemies > 0 && value <= 0) {
				AudioManager.switchToWorldBGM();
			}
			else if (_numAlertedEnemies <= 0 && value > 0) {
				AudioManager.switchToDangerBGM();
			}
			_numAlertedEnemies = value;
		}
	}
	static int _numAlertedEnemies;

	public enum State { Wait, Wander, Chase, Alert }

	const float FIELD_OF_VIEW_ANGLE = 60f;
	const float ALLOWABLE_DESTINATION_ERROR = 0.25f;
	const float ALLOWABLE_DESTINATION_ERROR_SQR = ALLOWABLE_DESTINATION_ERROR * ALLOWABLE_DESTINATION_ERROR;
	const float ALERT_PAUSE_DURATION = 0.5f;

	[SerializeField] float spotDistance = 5f;
	[SerializeField] float trackDistance = 8f;
	[SerializeField] float waitIntervalLow = 0f;
	[SerializeField] float waitIntervalHigh = 5f;
	[SerializeField] float wanderSpeed = 2f;
	[SerializeField] float chaseSpeed = 4f;
	[SerializeField] float attackInterval = 1f;

	[SerializeField] bool debugVision = false;

	float spotDistanceSqr;
	float trackDistanceSqr;
	float waitTimer;
	float attackTimer;
	float speed;
	float chaseTimer;

	const float MAX_CHASE_TIME_WITHOUT_SEEING_PLAYER = 5f;

	Vector3 destination {
		get {
			return _destination;
		}
		set {
			if (path.Count == 0 || (value - _rawDestination).sqrMagnitude > 0.25f) {
				path = Navigation.findPath(transform.position, value);
//				Debug.Log("Start: " + path[0].ToString() + "\n" + path.Count.ToString() + "   " + _rawDestination.ToString() + " => " + value.ToString() + " => " + path[path.Count - 1].ToString());
				_rawDestination = value;
				_destination = path[path.Count - 1];
			}
		}
	}
	Vector3 _rawDestination;
	Vector3 _destination;
	List<Vector3> path = new List<Vector3>();
	List<Vector3> patrolDestinations;
	int nextPatrolDestinationIndex = 0;
	Vector3 startingPosition;

	float stoppingDistance {
		get {
			return _stoppingDistance;
		}
		set {
			_stoppingDistance = value;
			stoppingDistanceSqr = _stoppingDistance * _stoppingDistance;
		}
	}
	float _stoppingDistance;
	float stoppingDistanceSqr;

	GameObject exclamationPoint;

	State state {
		get {
			return _state;
		}
		set {
			if (_state == State.Chase && value != State.Chase) {
				numAlertedEnemies -= 1;
			}
			exclamationPoint.SetActive(false);
			if (value == State.Wait) {
				waitTimer = Random.Range(waitIntervalLow, waitIntervalHigh);
			}
			else if (value == State.Wander) {
				speed = wanderSpeed;
				if (patrolDestinations.Count == 0) {
					Vector2 rand = Random.insideUnitCircle.normalized * Random.Range(2f, 6f);
					destination = startingPosition + new Vector3(rand.x, rand.y, 0);
				}
				else {
					destination = patrolDestinations[nextPatrolDestinationIndex];
					nextPatrolDestinationIndex += 1;
					if (nextPatrolDestinationIndex >= patrolDestinations.Count) {
						nextPatrolDestinationIndex = 0;
					}
				}
			}
			else if (value == State.Alert) {
				AudioManager.playSFX(AudioManager.instance.alertSfx, 0.5f);
				waitTimer = ALERT_PAUSE_DURATION;
				speed = chaseSpeed;
				destination = Player.instance.transform.position;
				exclamationPoint.SetActive(true);
			}
			else if (value == State.Chase) {
				attackTimer = 0;
				if (_state != State.Chase) {
					numAlertedEnemies += 1;
				}
				chaseTimer = MAX_CHASE_TIME_WITHOUT_SEEING_PLAYER;
			}
			_state = value;
		}
	}
	private State _state;

	new void Awake() {
		base.Awake();
		if (gameObject.layer != Layers.ENEMY) {
			Debug.LogWarning("The enemy '" + name + "' is not set to the Enemy layer");
		}
		exclamationPoint = transform.Find("Exclamation Point").gameObject;
		spotDistanceSqr = spotDistance * spotDistance;
		trackDistanceSqr = trackDistance * trackDistance;
		state = State.Wait;
		stoppingDistance = 0.1f;
		quad = transform.GetChild(0);
		cyl = transform.GetChild(1);
		StartCoroutine(navigationCoroutine());
	}

	void Start() {
		startingPosition = transform.position;
		patrolDestinations = new List<Vector3>();
		foreach (Transform t in transform) {
			if (t.gameObject.layer != Layers.UI) {
				patrolDestinations.Add(t.position);
			}
		}
		Mesh m = quad.GetComponent<MeshFilter>().mesh;
		m.vertices = new Vector3[] {
			new Vector3(0, 0, 0),
			Quaternion.Euler(0, 0, -FIELD_OF_VIEW_ANGLE) * Vector3.up * spotDistance,
			Quaternion.Euler(0, 0, -FIELD_OF_VIEW_ANGLE * 0.75f) * Vector3.up * spotDistance,
			Quaternion.Euler(0, 0, -FIELD_OF_VIEW_ANGLE * 0.5f) * Vector3.up * spotDistance,
			Quaternion.Euler(0, 0, -FIELD_OF_VIEW_ANGLE * 0.25f) * Vector3.up * spotDistance,
			Vector3.up * spotDistance,
			Quaternion.Euler(0, 0, FIELD_OF_VIEW_ANGLE * 0.25f) * Vector3.up * spotDistance,
			Quaternion.Euler(0, 0, FIELD_OF_VIEW_ANGLE * 0.5f) * Vector3.up * spotDistance,
			Quaternion.Euler(0, 0, FIELD_OF_VIEW_ANGLE * 0.75f) * Vector3.up * spotDistance,
			Quaternion.Euler(0, 0, FIELD_OF_VIEW_ANGLE) * Vector3.up * spotDistance,
		};
		m.triangles = new int[] {
			0, 2, 1,
			0, 3, 2,
			0, 4, 3,
			0, 5, 4,
			0, 6, 5,
			0, 7, 6,
			0, 8, 7,
			0, 9, 8,
		};
	}

	new void Update() {
		if (hasControl) {
			stateSpecificUpdates();
		}
		else {
			rigidbody.velocity = Vector2.zero;
		}
		base.Update();
	}

	Transform quad;
	Transform cyl;

	void stateSpecificUpdates() {
		quad.gameObject.SetActive(debugVision && state != State.Chase);
		cyl.gameObject.SetActive(debugVision && state == State.Chase);
		if (debugVision) {
			if (state == State.Chase || state == State.Alert) {
				cyl.localScale = new Vector3(2f * trackDistance, 0.5f, 2f * trackDistance);
			}
			else {
				if (facing == Facing.Up) {
					quad.localRotation = Quaternion.Euler(0, 0, 0);
				}
				else if (facing == Facing.Right) {
					quad.localRotation = Quaternion.Euler(0, 0, -90);
				}
				else if (facing == Facing.Down) {
					quad.localRotation = Quaternion.Euler(0, 0, 180);
				}
				else {
					quad.localRotation = Quaternion.Euler(0, 0, 90);
				}
			}
		}

		if (Time.timeScale == 0f) {
			return; // Skip if the game is paused
		}
		if (state == State.Wait) {
			if (playerSpotted()) {
				state = State.Alert;
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
				state = State.Alert;
			}
			else if (reachedDestination()) {
				state = State.Wait;
			}
		}
		else if (state == State.Alert) {
			waitTimer -= Time.deltaTime;
			if (waitTimer <= 0) {
				state = State.Chase;
			}
		}
		else if (state == State.Chase) {
			if (playerTracked()) {
				chaseTimer = MAX_CHASE_TIME_WITHOUT_SEEING_PLAYER;
				// Updates the destination if the player moved too much from the destination
				if ((destination - Player.instance.transform.position).sqrMagnitude > ALLOWABLE_DESTINATION_ERROR_SQR) {
					destination = Player.instance.transform.position;
				}
				// Checking if he should attack
				attackTimer += Time.deltaTime;
				if (attackTimer > attackInterval) {
					attackTimer = 0;
					if (hasClearShot()) {
						attack();
					}
				}
			}
			else if (reachedDestination()) {
				// Arrived at the last known location of the player and he's still out of sight
				state = State.Wander;
			}
			else {
				chaseTimer -= Time.deltaTime;
				if (chaseTimer <= 0) {
					state = State.Wander; // He can't reach his destination but he hasn't seen the player in a while, go into wander state
				}
			}
		}
	}

	public override void takeDamage(int damage) {
		AudioManager.playSFX(AudioManager.instance.hitSfx);
		if (state == State.Chase) {
			hp -= damage;
		}
		else {
			// Instant kill
			hp = 0;
		}

		if (hp <= 0) {
			if (state == State.Chase) {
				numAlertedEnemies -= 1;
			}
			Destroy(gameObject);
			return;
		}

		animator.SetInteger("HP", hp);
		animator.SetTrigger("Flinch");
		hasControl = false;
	}

	void OnCollisionEnter2D(Collision2D c) {
		if (state == State.Wait || state == State.Wander) {
			if (c.gameObject.layer == Layers.PLAYER) {
				state = State.Alert;
			}		
		}
	}


	const float REACTION_TIME = 0.25f;
	IEnumerator navigationCoroutine() {
		float timeToRecalculateDirection = 0;
		int x = 0;
		int y = 0;
		bool usingPrimaryDirection = true;
		while (true) {
			if (!hasControl) {
				yield return null;
				continue;
			}
			if (state != State.Wait && state != State.Alert && path.Count > 0) {
//				Vector3 toDest = destination - transform.position;
				Vector3 toDest;
				while (true) {
					toDest = path[0] - transform.position;
					if (toDest.sqrMagnitude < 0.0625f) {
						path.RemoveAt(0);
						if (path.Count == 0) {
							break;
						}
//						Debug.Log("Next: " + path[0].ToString() + "\n" + path.Count.ToString() + " remaining");
					}
					else {
						break;
					}
				}
				if (path.Count == 0) {
					x = 0;
					y = 0;
				}
				else {
					timeToRecalculateDirection -= Time.deltaTime;
					if (timeToRecalculateDirection <= 0) {
						// Recalculate direction
						timeToRecalculateDirection = REACTION_TIME;
						x = 0;
						y = 0;
						float toDestXDist = Mathf.Abs(toDest.x);
						float toDestYDist = Mathf.Abs(toDest.y);
						if (toDestXDist > 0 || toDestYDist > 0) {
							float horizontalProbability = toDestXDist / (toDestXDist + toDestYDist);
							if (Random.value < horizontalProbability) {
//							if (horizontalProbability > 0.5f) {
								x = toDest.x > 0 ? 1 : -1;
							}
							else {
								y = toDest.y > 0 ? 1 : -1;
							}
							// Check if he can move in the preferred direction
							RaycastHit2D hitInfo = Physics2D.CircleCast(transform.position, radius - 0.05f, new Vector2(x, y), 0.25f, ~(1 << Layers.ENEMY | 1 << Layers.PLAYER | 1 << Layers.NPC));
							if (hitInfo.collider != null) {
								// Can't move the preferred direction, try the secondary direction
								usingPrimaryDirection = false;
								if (x != 0) {
									x = 0;
									y = toDest.y > 0 ? 1 : -1;
								}
								else {
									y = 0;
									x = toDest.x > 0 ? 1 : -1;
								}
							}
							else {
								usingPrimaryDirection = true;
							}
						}
						// Handling facing
						if (x != 0) {
							facing = x > 0 ? Facing.Right : Facing.Left;
						}
						else if (y != 0) {
							facing = y > 0 ? Facing.Up : Facing.Down;
						}
					}
				}
				///*
				// Preventing passing by the destination between reaction times
				if (usingPrimaryDirection) {
					if (x != 0) {
						if (Mathf.Sign(x) != Mathf.Sign(toDest.x)) {
							x = 0;
							timeToRecalculateDirection = 0;
						}
					}
					else if (y != 0) {
						if (Mathf.Sign(y) != Mathf.Sign(toDest.y)) {
							y = 0;
							timeToRecalculateDirection = 0;
						}
					}
				}
				//*/
				animator.SetFloat("Speed", (x == 0 && y == 0) ? 0 : 1);
				rigidbody.velocity = new Vector2(x * speed, y * speed);
			}
			else {
				animator.SetFloat("Speed", 0);
				rigidbody.velocity = Vector2.zero;
			}
			yield return null;
		}
	}

	bool playerSpotted() {
		if (Player.instance.hidden) {
			return false;
		}
		Vector3 toPlayer = Player.instance.transform.position - transform.position;
		if (Vector3.Angle(getForward(), toPlayer) < FIELD_OF_VIEW_ANGLE) {
			if (toPlayer.sqrMagnitude < spotDistanceSqr) {
				float manhattanDistanceToPlayer = Mathf.Abs(toPlayer.x) + Mathf.Abs(toPlayer.y) + Mathf.Abs(toPlayer.z);
				RaycastHit2D hitInfo = Physics2D.Raycast(transform.position, toPlayer, manhattanDistanceToPlayer, ~(1 << Layers.ENEMY | 1 << Layers.NPC | 1 << Layers.WEAPON));
				if (hitInfo.collider != null) {
					if (hitInfo.collider.gameObject.layer == Layers.PLAYER) {
						return true;
					}
				}
			}
		}
		return false;
	}

	bool playerTracked() {
		if (Player.instance.hidden) {
			return false;
		}
		Vector3 toPlayer = Player.instance.transform.position - transform.position;
		if (toPlayer.sqrMagnitude < trackDistanceSqr) {
			float manhattanDistanceToPlayer = Mathf.Abs(toPlayer.x) + Mathf.Abs(toPlayer.y) + Mathf.Abs(toPlayer.z);
			RaycastHit2D hitInfo = Physics2D.Raycast(transform.position, toPlayer, manhattanDistanceToPlayer, ~(1 << Layers.ENEMY | 1 << Layers.NPC | 1 << Layers.WEAPON));
			if (hitInfo.collider != null) {
				if (hitInfo.collider.gameObject.layer == Layers.PLAYER) {
					return true;
				}
			}
		}
		return false;
	}

	bool hasClearShot() {
		if (!(weapon is MeleeWeapon) && Player.instance.hidden) {
			return false;
		}
		return weapon.hasClearShot(this, Layers.PLAYER);
	}


	bool reachedDestination() {
		return path.Count == 0 || (destination - transform.position).sqrMagnitude < stoppingDistanceSqr;
	}

}
