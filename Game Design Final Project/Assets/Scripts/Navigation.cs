using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Navigation {
	
	const float MAX_COMPUTATION_TIME = 0.003f;
	const float GRANULARITY = 0.5f;

	static Vector3 getPosFromCoord(Pair coord) {
		return new Vector3(coord.x * GRANULARITY, coord.y * GRANULARITY, 0);
	}

	static Pair getCoordFromPos(Vector3 pos) {
		return new Pair(Mathf.RoundToInt(pos.x / GRANULARITY), Mathf.RoundToInt(pos.y / GRANULARITY));
	}
	
	static Dictionary<Pair, bool> walkableCache = new Dictionary<Pair, bool>();

	public static List<Vector3> findPath(Vector3 start, Vector3 goal) {
		Pair startPair = getCoordFromPos(start);//new Pair(Mathf.RoundToInt(start.x - 0.5f), Mathf.RoundToInt(start.y - 0.5f));
		Pair goalPair = getCoordFromPos(goal);//new Pair(Mathf.RoundToInt(goal.x - 0.5f), Mathf.RoundToInt(goal.y - 0.5f));
		goalPair = findClosestWalkableCoord(goalPair, walkableCache);
		return findPathImplementation(startPair, goalPair, walkableCache);
	}
	
	static Pair findClosestWalkableCoord(Pair target, Dictionary<Pair, bool> walkableCache) {
		HashSet<Pair> closedSet = new HashSet<Pair>();
		List<Pair> openList = new List<Pair>();
		openList.Add(target);
		while (true) {
			Pair curr = openList[0];
			openList.RemoveAt(0);
			if (isWalkable(curr, walkableCache)) {
				return curr;
			}
			closedSet.Add(curr);
			List<Pair> successors = new List<Pair>(new Pair[] {
				new Pair(curr.x - 1, curr.y),
				new Pair(curr.x + 1, curr.y),
				new Pair(curr.x, curr.y - 1),
				new Pair(curr.x, curr.y + 1)
			});
			while (successors.Count > 0) {
				int index = Random.Range(0, successors.Count);
				Pair successor = successors[index];
				successors.RemoveAt(index);
				if (closedSet.Contains(successor)) {
					continue;
				}
				openList.Add(successor);
			}
		}
	}
	
	static List<Vector3> findPathImplementation(Pair start, Pair goal, Dictionary<Pair, bool> walkableCache) {
		float startTime = Time.realtimeSinceStartup;
		HashSet<Node> closedSet = new HashSet<Node>();
		List<Node> openList = new List<Node>();
		openList.Add(new Node(start, goal, null));
		Node finalNode = null;
//		Debug.Break();
//		Debug.Log("Starts at " + start.ToString());
		while (openList.Count > 0 && (Time.realtimeSinceStartup - startTime) < MAX_COMPUTATION_TIME) {
			Node curr = popMin(openList);
			if (curr.coord == goal) {
				finalNode = curr;
//				Debug.Log(curr.coord.ToString() + " == " + goal.ToString() + "\n" + ((curr.prev == null) ? "" : curr.prev.ToString()));
				break;
			}
//			Debug.Log("Explored " + curr.coord.ToString());
			closedSet.Add(curr);
			foreach (Node successor in curr.generateSuccessors(goal)) {
				if (!isWalkable(successor.coord, walkableCache)) {
					continue;
				}
				if (closedSet.Contains(successor)) {
					continue;
				}
				int otherInstance = openList.FindIndex((x) => { return successor.coord == x.coord; });
				if (otherInstance < 0) {
					// Not in open list
					openList.Add(successor);
//					Debug.Log("Potential exploration: " + successor.coord.ToString() + " from " + curr.coord.ToString());
				}
				else {
					// Replace in open list if it is a lower f value than the other instance of it
					if (successor.f < openList[otherInstance].f) {
						openList[otherInstance] = successor;
					}
				}
			}
		}
		if (finalNode == null) {
			// No path found; we need to return a partial path
			if (openList.Count == 0) {
				// It was impossible to reach the goal; select the closest node we found to the goal
//				Debug.Log("closed, couldn't find a path");
				finalNode = popMin(closedSet);
			}
			else {
				// We ran out of time; select the closest node to the goal we've seen so far
//				Debug.Log("open, ran out of time after exploring " + closedSet.Count.ToString() + " nodes and put " + openList.Count.ToString() + " on open");
				finalNode = popMin(openList);
			}
		}
		List<Pair> pathPoints = new List<Pair>();
		getPath(finalNode, pathPoints);
		List<Vector3> destinations = postProcessPaths(pathPoints);
//		Debug.Log("A* took " + (Time.realtimeSinceStartup - startTime).ToString() + " seconds\t" + destinations.Count.ToString() + "\n" + start.ToString() + " " + finalNode.coord.ToString());
		return destinations;
	}
	
	static bool isWalkable(Pair coord, Dictionary<Pair, bool> walkableCache) {
		if (walkableCache.ContainsKey(coord)) {
			return walkableCache[coord];
		}
		Collider2D collider = Physics2D.OverlapCircle(getPosFromCoord(coord), 0.45f, ~(1 << Layers.PLAYER | 1 << Layers.ENEMY | 1 << Layers.NPC));
		bool canWalkThere = (collider == null);
		if (!canWalkThere && collider.gameObject.layer != Layers.WEAPON) {
			walkableCache.Add(coord, canWalkThere); // Only cache colliders that are part of the level geometry
		}
		return canWalkThere;
	}
	
	static Node popMin(List<Node> openList) {
		float minF = float.PositiveInfinity;
		int currentBestIndex = -1;
		for (int i = 0; i < openList.Count; ++i) {
			if (openList[i].f < minF) {
				minF = openList[i].f;
				currentBestIndex = i;
			}
		}
		Node best = openList[currentBestIndex];
		openList.RemoveAt(currentBestIndex);
		return best;
	}
	
	static Node popMin(HashSet<Node> closedSet) {
		float minH = float.PositiveInfinity;
		Node best = null;
		foreach (Node n in closedSet) {
			if (n.h < minH) {
				minH = n.h;
				best = n;
			}
			else if (n.h == minH) {
				if (n.g < best.g) {
					best = n;
				}
			}
		}
		return best;
	}
	
	static void getPath(Node end, List<Pair> pathPoints) {
		if (end != null) {
			pathPoints.Add(end.coord);
			getPath(end.prev, pathPoints);
		}
	}

	// Reverses the path, prunes out useless mid points, and converts the coordinates to world-space positions
	static List<Vector3> postProcessPaths(List<Pair> pathPoints) {
		List<Vector3> destinations = new List<Vector3>();
		if (pathPoints.Count <= 1) {
			for (int i = pathPoints.Count - 1; i >= 0; --i) {
				destinations.Add(getPosFromCoord(pathPoints[i]));
			}
			return destinations;
		}
		// Pruning the path points
		Pair priorPivot = pathPoints[pathPoints.Count - 1];
		bool scanningHorizontally = priorPivot.y == pathPoints[pathPoints.Count - 2].y;
		for (int i = pathPoints.Count - 2; i >= 0; --i) {
			if (scanningHorizontally) {
				if (priorPivot.y != pathPoints[i].y) {
					priorPivot = pathPoints[i + 1];
					destinations.Add(getPosFromCoord(priorPivot));
					scanningHorizontally = false;
				}
			}
			else {
				if (priorPivot.x != pathPoints[i].x) {
					priorPivot = pathPoints[i + 1];
					destinations.Add(getPosFromCoord(priorPivot));
					scanningHorizontally = true;
				}
			}
		}
		destinations.Add(getPosFromCoord(pathPoints[0]));
		return destinations;
	}

	
	
	
	public class Node {
		
		public float g;
		public float h;
		public float f;
		public Pair coord;
		public Node prev;
		
		static float getHeuristic(Pair coord, Pair goal) {
			return Mathf.Abs(coord.x - goal.x) + Mathf.Abs(coord.y - goal.y);
		}
		
		public Node(Pair coord, Pair goal, Node prev) {
			if (prev == null) {
				g = 0;
			}
			else {
				g = prev.g + 1;
			}
			this.coord = coord;
			this.prev = prev;
			h = getHeuristic(coord, goal);
			f = g + h;
		}
		
		public IEnumerable<Node> generateSuccessors(Pair goal) {
			yield return new Node(new Pair(coord.x - 1, coord.y), goal, this);
			yield return new Node(new Pair(coord.x + 1, coord.y), goal, this);
			yield return new Node(new Pair(coord.x, coord.y - 1), goal, this);
			yield return new Node(new Pair(coord.x, coord.y + 1), goal, this);
		}
		
		public override bool Equals(object obj) {
			if (obj is Node) {
				Node other = (Node)obj;
				return other.coord == coord;
			}
			return false;
		}
		
		public override int GetHashCode() {
			return coord.ToString().GetHashCode();
		}
		
	}
	
	public struct Pair {
		public int x;
		public int y;
		
		public Pair(int x, int y) {
			this.x = x;
			this.y = y;
		}
		
		public static bool operator ==(Pair lhs, Pair rhs) {
			return lhs.Equals(rhs);
		}
		
		public static bool operator !=(Pair lhs, Pair rhs) {
			return !lhs.Equals(rhs);
		}
		
		public override bool Equals(object obj) {
			if (obj is Pair) {
				Pair other = (Pair)obj;
				return other.x == x && other.y == y;
			}
			return false;
		} 
		
		public override string ToString() {
			return x.ToString() + "," + y.ToString();
		}
		
		public override int GetHashCode() {
			return ToString().GetHashCode();
		}
	}
	
}
