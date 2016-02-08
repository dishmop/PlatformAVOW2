using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


public class CircuitSimulator : MonoBehaviour {
	public static CircuitSimulator singleton = null;
	
	public Dictionary<int, Eppy.Tuple<float, float, bool>> bubblePulseLookup = new Dictionary<int, Eppy.Tuple<float, float, bool>>();
	
	public class LoopRecord{
		public LoopRecord(int loopId, int fromNodeIndex){
			this.loopId = loopId;
			this.fromNodeIndex = fromNodeIndex;
		}
		public int loopId;
		public int fromNodeIndex;		// 0 or 1
	};
	
	public enum EdgeType{
		kNormal,
		kBlue
	}
	public bool voltageError;
	public Edge batteryEdge;
	
	
	// Circuir graph is made from nodes and edges
	public class Node{
		public int id;
		public string debugName;
		
		// These two lists are kept in sync - edges[i].nodes[edgeIndices[i] = this
		public List<Edge> edges = new List<Edge>();
		public List<int> edgeIndices = new List<int>();
		
		// graph traversal data
		public bool disabled = false;
		public bool visited = false;
		
		// Results data
		public float resVoltage;
		
		// Debugging
		public string GetID(){return "N" + id;}
		
		// Members for AVOW representation
		public List<Edge> inEdges = new List<Edge>();
		public List<Edge> outEdges = new List<Edge>();
		public float h0;
		public float hWidth;
		public bool hasBeenLayedOut;
		public bool isInBatteryCliqueRep;
		public bool isInBatteryClique;
		public Node repNode;		// All nodes which are connected by zero resistance edges for a group who will all have the same repnode (the node with the lowest ID).
		

	};
	
	public class Edge{
		public int id;
		
		// These two arrays are kept in sync - nodes[i].edges[nodeIndices[i] = this
		public Node[] nodes = new Node[2];
		public int[] nodeIndices = new int[2];
		
		
		// ElectricalComponent properties
		public float voltageRise = 0; // from node 0 to node 1 if a votlage source
		public float resistance = 0; // if a load
		
		
		// Loop data
		public List<LoopRecord> loopRecords = new List<LoopRecord>();
		
		// Graph traversal data
		public bool disabled = false;
		public bool visited = false;
		
		public EdgeType edgeType = EdgeType.kNormal;
		
		// Results data
		public float resFwCurrent;
		
		// Debugging
		public string GetID(){return "E" + id;}
		
		// Members for AVOW representation
		public float hOrder = -1;
		public float h0;
		public float hWidth;
		public Node inNode;
		public Node outNode;
		public bool hasBeenLayedOut;	// False at first and then true after it has been laid out at least once
		public bool isInBatteryClique;
		
		public Node GetOtherNode(Node thisNode){
			if (nodes[0] != thisNode && nodes[1] != thisNode){
				Debug.LogError("Mismatched node");
				return null;
			}
			if (nodes[0] == thisNode){
				return nodes[1];
			}
			else{
				return nodes[0];
			}
			
		}

	};

	public class LoopElement{
		public LoopElement(Edge edge, int fromNodeIndex){
			this.edge = edge;
			this.fromNodeIndex = fromNodeIndex;
		}
			
		public Edge edge;
		public int fromNodeIndex; // 0 or 1
	}
	
	// The Graph structures
	public List<Node> allNodes = new List<Node>();
	public List<Edge> allEdges = new List<Edge>();
	
	// Solver structures
	public List<List<LoopElement>> loops;
	
	// Current solving	
	double epsilon = 0.0001;
	float[] loopCurrents;
	
	public void RegisterPulseEdge(int simEdgeId, float speed, float offset, bool isDirectionAgnositic){
		if (!bubblePulseLookup.ContainsKey(simEdgeId)){
			bubblePulseLookup.Add(simEdgeId, new Eppy.Tuple<float, float, bool>(speed, offset, isDirectionAgnositic));
		}
		else{
			bubblePulseLookup[simEdgeId] = new Eppy.Tuple<float, float, bool>(speed, offset, isDirectionAgnositic);
			
		}
	}
	
	public  void LookupPulseEdge(int simEdgeId, out float speed, out float offset, out bool isDirectionAgnostic){
		if (!bubblePulseLookup.ContainsKey(simEdgeId)){
			speed = 0;
			offset = 0;
			isDirectionAgnostic = false;
		}
		else{
			Eppy.Tuple<float, float, bool> result = bubblePulseLookup[simEdgeId];
			speed = result.Item1;
			offset = result.Item2;
			isDirectionAgnostic = result.Item3;
		}
	}


	public void ClearCircuit(){
		allNodes.Clear();
		allEdges.Clear();
	}
	
	
	// Setup in and out nodes on each edge.
	// Also mark any eges and nodes which are in the same clique as the battery.
	public void SetupAVOWMembers(){
	

		
		// Battery Clique
		batteryEdge = null;
		foreach(Edge edge in allEdges){
			edge.isInBatteryClique = false;
			if (edge.voltageRise > 0){
				batteryEdge = edge;
			}
		}
		
		
		foreach(Node node in allNodes){
			node.isInBatteryCliqueRep = false;
			node.isInBatteryClique = false;
		}
		if (batteryEdge != null){
			// Start at the battery edge and add any nodes
			Stack<Edge> edgeStack = new Stack<Edge>();
			edgeStack.Push(batteryEdge);
			while (edgeStack.Count != 0){
				Edge thisEdge = edgeStack.Pop();
				
				thisEdge.isInBatteryClique = true;
				
				Node node0 = thisEdge.nodes[0];
				Node node1 = thisEdge.nodes[1];
				
				// Ensure the nodes at either end are also in the clique
				node0.isInBatteryCliqueRep = true;
				node1.isInBatteryCliqueRep = true;
				node0.isInBatteryClique = true;
				node1.isInBatteryClique = true;
				
				foreach (Edge edge in thisEdge.nodes[0].edges){
					if (!edge.isInBatteryClique){
						edgeStack.Push(edge);
					}
				}
				foreach (Edge edge in thisEdge.nodes[1].edges){
					if (!edge.isInBatteryClique){
						edgeStack.Push(edge);
					}
				}
			}
		}
		
		
		// RepNodes
		foreach(Node node in allNodes){
			node.repNode = null;
		}		
		foreach(Node node in allNodes){
			List<Node> nodeGroup = new List<Node>();
			Stack<Node> nodeStack = new Stack<Node>();
			if (node.repNode != null) continue;
			
			nodeStack.Push (node);
			
			// Mark it as added to the stack
			node.repNode = node;
			
			// We keep track of the node we encounter with the minimal id as this will be our representative one
			Node minNode = node;
			
			while (nodeStack.Count != 0){
				Node thisNode = nodeStack.Pop ();
				nodeGroup.Add(thisNode);
				if (thisNode.id < minNode.id){
					minNode = thisNode;
				}
				foreach (Edge edge in thisNode.edges){
					if (MathUtils.FP.Feq(edge.resistance, 0) && MathUtils.FP.Feq(edge.voltageRise, 0)){
						Node otherNode = edge.GetOtherNode(thisNode);
						if (otherNode.repNode == null){
							nodeStack.Push (otherNode);
							otherNode.repNode = otherNode;
						}
						
					}
				}

			}
			// Now we have out list of nodes which can all be represented by a single node
			foreach (Node groupNode in nodeGroup){
				groupNode.repNode = minNode;

			}
		}
		// If we are not a "representative node" then we should not be part of the clique	
		foreach(Node node in allNodes){	
			// We should not have duplicates as part of the battery clique
			if (node != node.repNode){
				node.isInBatteryCliqueRep = false;
			}	
		}	
		
		// If we are subsumed by a rep group, then we should not be part of ther clique
		foreach (Edge edge in allEdges){
			if (edge.nodes[0].repNode == edge.nodes[1].repNode){
				edge.isInBatteryClique = false;
			}
		}
		
		
		// In/Out nodes
		foreach(Edge edge in allEdges){
			edge.hWidth = Mathf.Abs(edge.resFwCurrent);
			if (edge.resFwCurrent > 0){
				edge.inNode = edge.nodes[0];
				edge.outNode = edge.nodes[1];
			}
			else{
				edge.inNode = edge.nodes[1];
				edge.outNode = edge.nodes[0];
			}
		}
	
		
		
	}
	
	// Create a new node and returns its ID
	public int AddNode(string name){
		int id = allNodes.Count();
		Node newNode = new Node();
		newNode.id = id;
		newNode.debugName = name;
		allNodes.Add (newNode);
		return id;
	}
	
	
	
	public int AddVoltageSourceEdge(int node0Id, int node1Id, float voltageRise){
	
		int edgeId = AddEdge(node0Id, node1Id);
		allEdges[edgeId].voltageRise = voltageRise;
		return edgeId;
		
	}
	
	public int AddVoltageSourceEdge(int node0Id, int node1Id, float voltageRise, float resistance){
		
		int edgeId = AddEdge(node0Id, node1Id);
		allEdges[edgeId].voltageRise = voltageRise;
		allEdges[edgeId].resistance = resistance;
		return edgeId;
		
	}
	
	// Creates a new load edge and returns its Id
	public int AddLoadEdge(int node0Id, int node1Id, float resistance, float hOrder, EdgeType edgeType = EdgeType.kNormal){
		
		int edgeId = AddEdge(node0Id, node1Id,  hOrder, edgeType);
		allEdges[edgeId].resistance = resistance;
		return edgeId;
	}
	
	
	// Creates a new edge whic is a perfect conductor and returns its Id
	public int AddConductorEdge(int node0Id, int node1Id){
		
		int edgeId = AddEdge(node0Id, node1Id);
		return edgeId;
	}
	
	
	public CircuitSimulator.Edge GetEdge(int id){
		CircuitSimulator.Edge edge = allEdges.Find(obj=>obj.id == id);
		if (edge == null){
			Debug.Log ("GetEdge (" + id + "): Edge Not found");
		}
		return edge;
	}
	
	
	public void GameUpdate(){
		voltageError = false;
	
	//	Debug.Log ("numNodes = " + allNodes.Count() + ", numEdges = " + allEdges.Count());
	
		if (allEdges.Count < 2 || allNodes.Count == 0) return;
		
		// Check that the graph is valid
		DebugUtils.Assert(ValidateGraph(), "The graph is invalid");

		FindLoops();
		
		//DebugPrintLoops();
		
		RecordLoopsInComponents();

		SolveForCurrents();
		//DebugPrintLoopCurrents();
		
		StoreCurrentsInComponents();
		//DebugPrintComponentCurrents();
		
		CalcVoltages();
		//DebugPrintVoltages();
		
		OrderEdgesByHOrder();
		
		SetupAVOWMembers();
		
	}
	
	void OrderEdgesByHOrder(){
		// Ensure all components are sorted by horder (makes things easier to find
		allEdges.Sort((obj1, obj2) => obj1.hOrder.CompareTo(obj2.hOrder));
	}
	
	// return true if the graph is self-consistent
	bool ValidateGraph(){
		// Check all the nodes
		foreach (Node node in allNodes){
			// the number of edges and the number of edgeIndices should match up
			if (node.edges.Count() != node.edgeIndices.Count()) return false;
			
			// Check each edge has a record of this node being connected to it
			for (int i = 0; i < node.edges.Count(); ++i){
				
				Edge nodeEdge = node.edges[i];
				int nodeEdgeIndex = node.edgeIndices[i];
				
				Node self = nodeEdge.nodes[nodeEdgeIndex];
				if (self != node) return false;
				
			}
		}
		
		// Check all the edges
		foreach (Edge edge in allEdges){
			// the number of edges and the number of edgeIndices should match up
			if (edge.nodes.Count() != edge.nodeIndices.Count()) return false;
			
			// Check each edge has a record of this node being connected to it
			for (int i = 0; i < edge.nodes.Count(); ++i){
				
				Node edgeNode = edge.nodes[i];
				int edgeNodeIndex = edge.nodeIndices[i];
				
				Edge self = edgeNode.edges[edgeNodeIndex];
				if (self != edge) return false;
				
			}
		}
		
		// got to here - then all is well
		return true;
	}
	
	
	// Creates a new voltage source edge and returns its Id
	int AddEdge(int node0Id, int node1Id, float hOrder = -1, EdgeType edgeType = EdgeType.kNormal){
		int id = allEdges.Count();
		
		Edge newEdge = new Edge();
		newEdge.hOrder = hOrder;
		newEdge.id = id;
		newEdge.nodes[0] = allNodes[node0Id];
		newEdge.nodes[1] = allNodes[node1Id];
		newEdge.edgeType = edgeType;
		
		allNodes[node0Id].edges.Add(newEdge);
		allNodes[node1Id].edges.Add(newEdge);
		
		// Match up the indices
		allNodes[node0Id].edgeIndices.Add(0);
		allNodes[node1Id].edgeIndices.Add(1);
		newEdge.nodeIndices[0] = allNodes[node0Id].edges.Count - 1;
		newEdge.nodeIndices[1] = allNodes[node1Id].edges.Count - 1;
		
		allEdges.Add (newEdge);
//		Debug.Log ("Edge Added to Sim: " + newEdge.id);

				
		return id;
	}
	
	void ClearAllLoopData(){
		foreach (Node node in allNodes){
			node.disabled = false;
		}
		foreach (Edge edge in allEdges){
			edge.loopRecords = new List<LoopRecord>();
		}
		loops = new List<List<LoopElement>> ();
	}
	
	void ClearAllVisitedFlags(){
		foreach (Node node in allNodes){
			node.visited = false;
		}
		foreach (Edge edge in allEdges){
			edge.visited = false;
		}
	}
	
	void SimpleTestCase(){
		// Do some test cases
		int node0Id = AddNode("test0");
		int node1Id = AddNode("test1");
		int node2Id = AddNode("test2");
		int cellId = AddVoltageSourceEdge(node0Id, node2Id, 1);
		int resistor1Id = AddConductorEdge(node2Id, node1Id);
		int resistor2Id = AddLoadEdge(node1Id, node0Id, 1, 0);
		int resistor3Id = AddLoadEdge(node1Id, node0Id, 1, 1);
//		int resistor4Id = AddLoadEdge(node2Id, node1Id, 0);
		
		
		Debug.Log("Cell current = " + allEdges[cellId].resFwCurrent);
		Debug.Log("Resistor 1 current = " + allEdges[resistor1Id].resFwCurrent);
		Debug.Log("Resistor 2 current = " + allEdges[resistor2Id].resFwCurrent);
		Debug.Log("Resistor 3 current = " + allEdges[resistor3Id].resFwCurrent);
//		Debug.Log("Resistor 4 current = " + allEdges[resistor4Id].resFwCurrent);
		
		Debug.Log("Node 0 voltage = " + allNodes[node0Id].resVoltage);
		Debug.Log("Node 1 voltage = " + allNodes[node1Id].resVoltage);
		Debug.Log("Node 2 voltage = " + allNodes[node2Id].resVoltage);
		
	}
	
	
	void Start(){
//		SimpleTestCase();

		
	}
	
	void Awake(){
		if (singleton != null) Debug.LogError ("Error assigning singleton");
		singleton = this;
	}
	
	void OnDestroy(){
		singleton = null;
	}

	
	public void FixedUpdate(){

		
	//	Recalc();

	}
	
//	
//	void DebugPrintGraph(){
//		Debug.Log ("Print graph");
//		Debug.Log ("Nodes");
//		foreach (GameObject nodeGO in graph.allNodes){
//			Debug.Log("Node " + nodeGO.GetComponent<AVOWNode>().GetID());			
//		}
//		Debug.Log ("Components");
//		foreach (GameObject go in graph.allComponents){
//			AVOWComponent component = go.GetComponent<AVOWComponent>();
//			AVOWNode node0 = component.node0GO.GetComponent<AVOWNode>();
//			AVOWNode node1 = component.node1GO.GetComponent<AVOWNode>();
//			if (component.GetCurrent(component.node0GO) > 0){
//				Debug.Log("Component " + component.GetID() + "/" + component.hOrder + ": from " + node1.GetID() + " to " + node0.GetID() + " resistance = " + ((component.type == AVOWComponent.Type.kLoad) ? component.GetResistance().ToString () : "NULL") + " current = " + component.fwCurrent);
//			}
//			else{
//				Debug.Log("Component " + component.GetID() + "/" + component.hOrder + ": from " + node1.GetID() + " to " + node0.GetID() + " resistance = " + ((component.type == AVOWComponent.Type.kLoad) ? component.GetResistance().ToString () : "NULL") + " current = " + component.fwCurrent);
//			}
//		}
//	}
//	
	void DebugPrintLoops(){
		Debug.Log ("Printing loops");
		for (int i = 0; i < loops.Count; ++i){
			Node lastNode = loops[i][0].edge.nodes[loops[i][0].fromNodeIndex];
			string loopString = lastNode.GetID ();
			
			for (int j = 0; j < loops[i].Count; ++j){
				Node nextNode = loops[i][j].edge.nodes[1 - loops[i][j].fromNodeIndex];
				
				// print the connection and the final node
				loopString += "=>" + loops[i][j].edge.GetID () + "=>" + nextNode.GetID ();
				lastNode = nextNode;
			}
			Debug.Log ("Loop(" + i.ToString() + "): " + loopString );
			
		}
	}
//	
//	void DebugPrintLoopCurrents(){
//		Debug.Log("Printing loop currents");
//		for (int i = 0; i < loopCurrents.Length; ++i){
//			Debug.Log ("Loop " + i + ": " + loopCurrents[i]);
//			
//		}
//	}
//	

	
	
	// Finds a set of indendpent loops in the graph
	void FindLoops(){
		loops = new List<List<LoopElement>>();


		// Get any node which is going to be our starting point for all traversals	(this doesn't work if the first node is not part
		// of the circuit - or if we have multiple disjoint circuits) - but ok for the moment as we insist that the cell is put in first. 
		Node startNode = allNodes[0];
		
		
		// We have no components disabled then as we find loops, we disable one component at a time
		// until we can't find any more loops
		ClearAllLoopData();
				
		// We finish this loop when there are no loops left
		bool finished = false;
		while (!finished){
			ClearAllVisitedFlags();
			
			// Create a stack of nodes which we use to traverse the graph
			Stack<Node> nodeStack = new Stack<Node>();
			Stack<Edge> edgeStack = new Stack<Edge>();
			
			
			nodeStack.Push(startNode);
			
			// We finish this loop when we have found a loop (or we are sure there are none)
			bool foundLoop = false;
			while (!finished && !foundLoop){
			
				// We visit our current node
				Node currentNode = nodeStack.Peek();
				currentNode.visited = true;
				
				// Go through each connections from here in order and find one that has not yet been traversed
				Edge nextEdge = null;
				int nextEdgeIndex = -1;
				for (int i = 0; i < currentNode.edges.Count(); ++i){
					Edge edge = currentNode.edges[i];
					int edgeIndex =currentNode.edgeIndices[i];
					if (!edge.visited && !edge.disabled){
						nextEdge = edge;
						nextEdgeIndex = edgeIndex;
						nextEdge.visited = true;
						edgeStack.Push (nextEdge);
						break;
					}
				}
				
				// If we found an untraversed connection then this is our next point in our path
				if (nextEdge != null){
					Node nextNode = nextEdge.nodes[1 - nextEdgeIndex];
					
					if (nextNode == null){
						Debug.LogError ("Failed to find other end of connection when finding loops");
					}
					nodeStack.Push(nextNode);
					
					// If this node has already been visited, then we have found a loop
					if (nextNode.visited){
						foundLoop = true;
					}
					
				}
				// If we have not managed to find an untraversed connection, pop our current stack element
				else {
					nodeStack.Pop ();
					// if this is the last element in the node stack, then there will be no component associated with it
					if (edgeStack.Count > 0)
						edgeStack.Pop ();
				}
				
				// If there is nothing left on the stack then we have traversed the whole graph and not found a loop
				if (nodeStack.Count == 0){
					finished = true;
				}

			}
			if (foundLoop){
				
				// If we found a loop, then record it
				// We do this by stepping backwards down our stack until we find our current node again - this ensures thast we just get the loop
				// and no stragglers
				Node loopStartNode = nodeStack.Peek();
				Edge loopStartEdge = edgeStack.Peek();
				
				List<LoopElement> thisLoop = new List<LoopElement>();
				Edge addEdge = edgeStack.Pop ();
				thisLoop.Add (new LoopElement(addEdge, (addEdge.nodes[0].id == nodeStack.Pop ().id) ? 0 : 1));
				
				while(nodeStack.Peek() != loopStartNode){
					addEdge = edgeStack.Pop ();
					thisLoop.Add (new LoopElement(addEdge, (addEdge.nodes[0].id == nodeStack.Pop ().id) ? 0 : 1));
				}
				loops.Add (thisLoop);
				
				// Now disable one of the components in the loop so this loop is not found again
				loopStartEdge.disabled = true;
			}
			
			
		}
		
		// Quick check - we should have just traversed a spanning tree for the graph
		// so everyone component should have been visited
//		foreach (Edge edge in allEdges){
//			
//			if (!edge.visited && !edge.disabled)
//				Debug.Log ("Error: Spanning tree does not visit very node");
//		}
//  	THIS IS NOT A VALID CHECK! AS YOU MAY HAVE NODES WHICH ARE NOT CONNECTED - WAS FINE FOR AVOW
		
	}
	
	
	void RecordLoopsInComponents(){
		for (int i = 0; i < loops.Count; ++i){
			for (int j = 0; j < loops[i].Count; ++j){
				loops[i][j].edge.loopRecords.Add (new LoopRecord(i, loops[i][j].fromNodeIndex));
			}
		}
	
	}
	
	bool SolveForCurrents(){
	
		// Create arrays needed to solve equation coeffs
		double [,] R = new double[loops.Count, loops.Count];
		double [,] V = new double[loops.Count, 1];
		
		// For through each loop in turn (for each row in the matrices)
		for (int i = 0; i < loops.Count; ++i){
			// For each connection in the loop, check the resistance and any voltage drop
			for (int j = 0; j < loops[i].Count; ++j){
			
				LoopElement loopElement = loops[i][j];
				
				// deal with things differently depending on whether this is a load or a voltage source
				if (!MathUtils.FP.Feq(loopElement.edge.resistance, 0)){
					float resistance = loopElement.edge.resistance;
					
					// For each component in the loop, add in resistances for each loop that passes through it
					foreach (LoopRecord record in loopElement.edge.loopRecords){
						if (record.fromNodeIndex == loopElement.fromNodeIndex){
							R[i, record.loopId] += resistance;
						}
                        else{
							R[i, record.loopId] -= resistance;
						}
					
					}
				}
				if (!MathUtils.FP.Feq(loopElement.edge.voltageRise, 0)){
					V[i, 0] = (loopElement.fromNodeIndex == 0) ? loopElement.edge.voltageRise : -loopElement.edge.voltageRise;
				}
			}
		}  
		
		// Currents
		double[,] I = new double[0,0];
		
		// IF we do not have full rankm then find a solution using the Moore-Pensrose Pseudo-inverse
		// Method taken from here: http://en.wikipedia.org/wiki/Moore%E2%80%93Penrose_pseudoinverse#The_iterative_method_of_Ben-Israel_and_Cohen
		// search for "A computationally simple and accurate way to compute the pseudo inverse " ont his page
		
		
		//		if (R.GetLength (0) != R.GetLength (1)){
		//			Debug.LogError ("Matrix is not square, yet we expect it to be!");
		//		}
		//		
		
		// First we need to calc the SVD of the matrix
		double[] W = null;	// S matrix as a vector (leading diagonal)
		double[,] U = null;
		double[,] Vt = null;
		alglib.svd.rmatrixsvd(R, R.GetLength (0), R.GetLength (1), 2, 2, 2, ref W, ref U, ref Vt);
		
		double[,] S = new double[W.GetLength (0), W.GetLength (0)];
		
		for (int i = 0; i < R.GetLength (0); ++i){
			S[i,i] = W[i];
		}
		//				MathUtils.Matrix.SVD(R, out S, out U, out Vt);
		// Log the results
		
		//		double[,] testR0 = MathUtils.Matrix.Multiply(U, S);
		//		double[,] testR1 = MathUtils.Matrix.Multiply(testR0, Vt);
		
		double[,] Ut = MathUtils.Matrix.Transpose(U);
		double[,] Vtt = MathUtils.Matrix.Transpose (Vt);
		
		// Get the psuedo inverse of the U matrix (which is diagonal)
		// The way we do this is by taking the recipricol of each diagonal element, leaving the (close to) zero's in place
		// and transpose (actually we don't need to transpose because we always have square matricies)
		
		// I assume this gets initialised with zeros
		double[,] SInv = new double[S.GetLength(0), S.GetLength(0)];
		
		for (int i = 0; i < S.GetLength(0); ++i){
			if (Math.Abs (S[i,i]) > epsilon){
				SInv[i,i] = 1.0 / S[i,i];
				
			}					
		}
		
		//Rinv = Vtt Uinv St
		double[,] RInvTemp = MathUtils.Matrix.Multiply (Vtt, SInv);
		double[,] RInv = MathUtils.Matrix.Multiply (RInvTemp, Ut);
		
		//		// Test thast we have a psueoinverse
		//		double[,] res0 = MathUtils.Matrix.Multiply(R, RInv);
		//		double[,] res1 = MathUtils.Matrix.Multiply(res0, R);
		
		I = new double[loops.Count, 1];
		I = MathUtils.Matrix.Multiply(RInv, V); 
		
//		// Check that we get V - if not we have an unsolvable set of equations and 
//		// it means we have a loop of zero resistance with a voltage different in it
//		double[,] testV = MathUtils.Matrix.Multiply(R, I);
//		
//		bool failed = false;
//		for (int i = 0; i < loops.Count; ++i){
//			// f we find a bad loop
//			if (Math.Abs (V[i, 0] - testV[i, 0]) > epsilon){
//				// Then follow this loop finding all the voltage sources and put them in Emergency mode
//				for (int j = 0; j < loops[i].Count; ++j){
//					BranchAddress thisAddr = loops[i][j];
//					CircuitElement thisElement = Circuit.singleton.GetElement(new GridPoint(thisAddr.x, thisAddr.y));
//					if (Mathf.Abs (thisElement.GetVoltageDrop(thisAddr.dir, true)) > epsilon){
//						thisElement.TriggerEmergency();
//						failed = true;
//					}
//					BranchAddress oppAddr = GetOppositeAddress(thisAddr);
//					CircuitElement oppElement = Circuit.singleton.GetElement(new GridPoint(oppAddr.x, oppAddr.y));
//					if (Mathf.Abs (oppElement.GetVoltageDrop(oppAddr.dir, false)) > epsilon){
//						oppElement.TriggerEmergency();
//						failed = true;
//					}
//				}
//			}
//		}
//		if (failed) return false;
		
		
		
		loopCurrents = new float[loops.Count];
		if (I.GetLength(0) != 0){
			for (int i = 0; i < loops.Count; ++i){
				loopCurrents[i] = (float)I[i,0];
			}
		}
		// all went well
		return true;
		
	}	
	
	
	void StoreCurrentsInComponents(){
		foreach (Edge edge in allEdges){
			float totalCurrent = 0;
			foreach (LoopRecord record in edge.loopRecords){
				totalCurrent += loopCurrents[record.loopId] * (record.fromNodeIndex == 0 ? 1 : -1);
			}
			edge.resFwCurrent = totalCurrent;
		}
	}
	
//	void DebugPrintComponentCurrents(){
//		Debug.Log ("Print component currents");
//		foreach (GameObject componentGO in graph.allComponents){
//			AVOWComponent component = componentGO.GetComponent<AVOWComponent>();
//			Debug.Log ("Component " + component.GetID() + ": from " + component.node0GO.GetComponent<AVOWNode>().GetID () + " to " + component.node1GO.GetComponent<AVOWNode>().GetID() + ": current = " + component.fwCurrent + "A");
//			
//		}
//	}
	
	void CalcVoltages(){
		ClearAllVisitedFlags();
		
		// Need to start with the cell for some reason (not sure if this is necessary)
		Edge cellEdge = null;
		foreach (Edge edge in allEdges){
			if (!MathUtils.FP.Feq (edge.voltageRise, 0)){
				cellEdge = edge;
				break;
			}
		}
		
		// We (arbitrarily) set this to be zero volts
		cellEdge.nodes[0].resVoltage = 0;
		cellEdge.nodes[0].visited = true;		
		Stack<Node> nodeStack = new Stack<Node>();
		
		nodeStack.Push(cellEdge.nodes[0]);
		
		while(nodeStack.Count != 0){
			Node lastNode = nodeStack.Peek();
			
			//Find a component attached to this node which has not yet been visited
			Edge thisEdge = null;
			int thisEdgeIndex = -1;
			for (int i = 0; i < lastNode.edges.Count(); ++i){
				if (!lastNode.edges[i].visited){
					thisEdge = lastNode.edges[i];
					thisEdgeIndex = lastNode.edgeIndices[i];	// index that this nodes is in the edge's node list
				}
			}
			
//			Old version of the above
//			Edge thisEdge = lastNode.edges.Find (obj => !obj.loopVisited);
			
			// If we found one, then work out the voltage on the other end and either push it onto the stack (we it is a new node)
			// or just check we are being consistent if we have been there before
			if (thisEdge != null){
				thisEdge.visited = true;
				
				// Calc the voltage at the other end of this component
				float voltageChange = (-thisEdge.voltageRise + thisEdge.resFwCurrent * thisEdge.resistance) * (thisEdgeIndex == 0 ? 1 : -1);
				
				// Get the node at the other end
				Node nextNode = thisEdge.nodes[1 - thisEdgeIndex];
				
				// Shouldn't this be +?
				float nextNodeVoltage = lastNode.resVoltage - voltageChange;
				
				// If we have not yet visited this node, then set the voltage on it and add it to the stack
				if (!nextNode.visited){
					nextNode.resVoltage = nextNodeVoltage;
					nextNode.visited = true;
					nodeStack.Push (nextNode);	
				}
				// Otherwise, assert that the voltage is the same as what we just caluclated
				else{
					if (!MathUtils.FP.Feq (nextNode.resVoltage, nextNodeVoltage)){
						voltageError = true;
						Debug.Log("Voltage error!");
					}
				}
			}
			// If we failed to find an unvisited component, then pop this node off the stack (as there is
			// nothing more we can do here) and work on the node below it
			else{
				nodeStack.Pop ();
			}
		}
//		
//		// Now need to sort out voltages of nodes that have not ben visited (because they are not on a loop)
//		// For this by finding edges which have a known voltasge at one end and not at the other 
//		// do this iteratively until none are found
//		bool maybeSomeLeft = true;
//		while (maybeSomeLeft){
//			// If we go through the whole thing and there are no changes, then there is no maybe some left
//			maybeSomeLeft = false;
//			foreach(Edge edge in allEdges){
//				Node node0 = edge.nodes[0];
//				Node node1 = edge.nodes[1];
//				if (node0.visited && !node1.visited){
//					maybeSomeLeft = true;
//					node1.resVoltage = node0.resVoltage;
//					node1.visited = true;
//					
//				}
//				else if (!node0.visited && node1.visited){
//					maybeSomeLeft = true;
//					node0.resVoltage = node1.resVoltage;
//					node0.visited = true;
//				}
//			}
//		}
	}
	
//	void DebugPrintVoltages(){
//		Debug.Log ("Printing voltages");
//
//		foreach (GameObject nodeGO in graph.allNodes){
//			AVOWNode node = nodeGO.GetComponent<AVOWNode>();
//			Debug.Log ("Node " + node.GetID() + ": " + node.voltage + "V");
//		}
//	}
//	



	
}
