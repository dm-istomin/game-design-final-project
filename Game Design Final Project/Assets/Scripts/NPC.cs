using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NPC : Actor {

	public enum State { Wait, Wander }

	[SerializeField] float waitIntervalLow = 0f;
	[SerializeField] float waitIntervalHigh = 5f;
	[SerializeField] float speed = 1f;
	[SerializeField] string message;

	float waitTimer;

	Vector3 destination {
		get {
			return _destination;
		}
		set {
			if (path.Count == 0 || (value - _rawDestination).sqrMagnitude > 0.25f) {
				path = Navigation.findPath(transform.position, value);
				_rawDestination = value;
				_destination = path[path.Count - 1];
			}
		}
	}
	Vector3 _rawDestination;
	Vector3 _destination;
	List<Vector3> path = new List<Vector3>();
	Vector3 startingPosition;

	State state {
		get {
			return _state;
		}
		set {
			if (value == State.Wait) {
				waitTimer = Random.Range(waitIntervalLow, waitIntervalHigh);
			}
			else if (value == State.Wander) {
				Vector2 rand = Random.insideUnitCircle.normalized * Random.Range(2f, 6f);
				destination = startingPosition + new Vector3(rand.x, rand.y, 0);
			}
			_state = value;
		}
	}
	private State _state;

	new void Awake() {
		base.Awake();
		if (gameObject.layer != Layers.NPC) {
			Debug.LogWarning("The npc " + name + " is not set to the NPC layer");
		}
		state = State.Wait;
		StartCoroutine(navigationCoroutine());
	}

	void Start() {
		startingPosition = transform.position;
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

	void stateSpecificUpdates() {
		if (Time.timeScale == 0f) {
			return; // Skip if the game is paused
		}
		if (state == State.Wait) {
			waitTimer -= Time.deltaTime;
			if (waitTimer <= 0) {
				state = State.Wander;
			}
		}
		else if (state == State.Wander) {
			if (reachedDestination()) {
				state = State.Wait;
			}
		}
	}

	bool reachedDestination() {
		return path.Count == 0;
	}

	public override void takeDamage(int damage) {
		Vector3 toPlayer = Player.instance.transform.position - transform.position;
		if (Mathf.Abs(toPlayer.x) > Mathf.Abs(toPlayer.y)) {
			facing = Mathf.Sign(toPlayer.x) > 0 ? Facing.Right : Facing.Left;
		}
		else {
			facing = Mathf.Sign(toPlayer.y) > 0 ? Facing.Up : Facing.Down;
		}
		talk(message);
	}

	void talk(string text) {
		Dialog.show(text);
	}

	const float REACTION_TIME = 0.15f;
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
			if (state != State.Wait && path.Count > 0) {
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
				/*
				// Preventing passing by the destination between reaction times
				if (usingPrimaryDirection) {
					if (x != 0) {
						if (Mathf.Sign(x) != Mathf.Sign(toDest.x)) {
							x = 0;
						}
					}
					else if (y != 0) {
						if (Mathf.Sign(y) != Mathf.Sign(toDest.y)) {
							y = 0;
						}
					}
				}
				*/
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

}
