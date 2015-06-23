using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


public class CircuitSimulator : MonoBehaviour {
	public static CircuitSimulator singleton = null;
	
	public class LoopRecord{
		public LoopRecord(int loopId, int fromNodeIndex){
			this.loopId = loopId;
			this.fromNodeIndex = fromNodeIndex;
		}
		public int loopId;
		public int fromNodeIndex;		// 0 or 1
	};
	
	
	// Circuir graph is made from nodes and edges
	public class Node{
		public int id;
		
		// These two lists are kept in sync - edges[i].nodes[edgeIndices[i] = this
		public List<Edge> edges = new List<Edge>();
		public List<int> edgeIndices = new List<int>();
		
		// graph traversal data
		public bool disabled = false;
		public bool visited = false;
		
		// Results data
		public float resVoltage;
		
		// Debugging
		string GetID(){return "N" + id;}
		
		
		
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
		
		// Results data
		public float resFwCurrent;
		
		// Debugging
		string GetID(){return "E" + id;}
		
		

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
	List<Node> allNodes = new List<Node>();
	List<Edge> allEdges = new List<Edge>();
	
	// Solver structures
	public List<List<LoopElement>> loops;
	
	// Current solving	
	double epsilon = 0.0001;
	float[] loopCurrents;


//	static readonly int kOut = 0;
//	static readonly int kIn = 1;
//	static readonly int kNumDirs = 2;
//	
//	static int ReverseDirection(int dir){ return 1-dir;}


	public void ClearCircuit(){
		allNodes.Clear();
		allEdges.Clear();
	}
	
	// Create a new node and returns its ID
	public int AddNode(){
		int id = allNodes.Count();
		Node newNode = new Node();
		newNode.id = id;
		allNodes.Add (newNode);
		return id;
	}
	
	
	
	public int AddVoltageSourceEdge(int node0Id, int node1Id, float voltageDrop){
	
		int edgeId = AddEdge(node0Id, node1Id);
		allEdges[edgeId].voltageRise = voltageDrop;
		return edgeId;
		
	}
	
	// Creates a new load edge and returns its Id
	public int AddLoadEdge(int node0Id, int node1Id, float resistance){
		
		int edgeId = AddEdge(node0Id, node1Id);
		allEdges[edgeId].resistance = resistance;
		return edgeId;
	}
	
	
	public void Recalc(){
	
		if (allEdges.Count == 0 || allNodes.Count == 0) return;
		
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
	int AddEdge(int node0Id, int node1Id){
		int id = allEdges.Count();
		
		Edge newEdge = new Edge();
		newEdge.id = id;
		newEdge.nodes[0] = allNodes[node0Id];
		newEdge.nodes[1] = allNodes[node1Id];
		
		allNodes[node0Id].edges.Add(newEdge);
		allNodes[node1Id].edges.Add(newEdge);
		
		// Match up the indices
		allNodes[node0Id].edgeIndices.Add(0);
		allNodes[node1Id].edgeIndices.Add(1);
		newEdge.nodeIndices[0] = allNodes[node0Id].edges.Count - 1;
		newEdge.nodeIndices[1] = allNodes[node1Id].edges.Count - 1;
		
		allEdges.Add (newEdge);

				
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
	
	
	void Awake(){
		if (singleton != null) Debug.LogError ("Error assigning singleton");
		singleton = this;
	}
	
	void OnDestroy(){
		singleton = null;
	}

	
	public void FixedUpdate(){
		
		Recalc();

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
//	void DebugPrintLoops(){
//		Debug.Log ("Printing loops");
//		for (int i = 0; i < loops.Count; ++i){
//			AVOWNode lastNode = loops[i][0].fromNode;
//			string loopString = lastNode.GetID ();
//			
//			for (int j = 0; j < loops[i].Count; ++j){
//				AVOWNode nextNode = loops[i][j].component.GetOtherNode(lastNode.gameObject).GetComponent<AVOWNode>();
//				// print the connection and the final node
//				loopString += "=" + loops[i][j].component.GetID () + "=>" + nextNode.GetID ();
//				lastNode = nextNode;
//			}
//			Debug.Log ("Loop(" + i.ToString() + "): " + loopString );
//			
//		}
//	}
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


		// Get any node which is going to be our starting point for all traversals		
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
				thisLoop.Add (new LoopElement(edgeStack.Pop (), nodeStack.Pop ().id));
				
				while(nodeStack.Peek() != loopStartNode){
					thisLoop.Add (new LoopElement(edgeStack.Pop (), nodeStack.Pop ().id));
				}
				loops.Add (thisLoop);
				
				// Now disable one of the components in the loop so this loop is not found again
				loopStartEdge.disabled = true;
			}
			
			
		}
		
		// Quick check - we should have just traversed a spanning tree for the graph
		// so everyone component should have been visited
		foreach (Edge edge in allEdges){
			
			if (!edge.visited && !edge.disabled)
				Debug.Log ("Error: Spanning tree does not visit very node");
		}
		
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
					V[i, 0] = (loopElement.fromNodeIndex == 0) ? loopElement.edge.voltageRise : - loopElement.edge.voltageRise;
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

		
		// We have now visited this cell and the first node. 
		cellEdge.visited = true;
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
				float voltageChange = thisEdge.resFwCurrent * thisEdge.resistance * (thisEdgeIndex == 0 ? 1 : -1);
				
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
					if (!MathUtils.FP.Feq(nextNode.resVoltage, nextNodeVoltage)){
						Debug.LogError ("Voltages accross components are not consitent");
					}
				}
			}
			// If we failed to find an unvisited component, then pop this node off the stack (as there is
			// nothing more we can do here) and work on the node below it
			else{
				nodeStack.Pop ();
			}
		}
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
