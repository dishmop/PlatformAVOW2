using UnityEngine;


public class Directions{
	public const int kUp = 				0;
	public const int kRight = 			1;
	public const int kDown = 			2;
	public const int kLeft = 			3;
	public const int kNull = 			4;
	public const int kNumDirections = 	4;
	
	
	public static Vector3 GetDirVec(int dir){
		
		switch (dir){
		case Directions.kUp:
			return new Vector3(0, 1, 0);
		case Directions.kRight:
			return new Vector3(1, 0, 0);
		case Directions.kDown:
			return new Vector3(0, -1, 0);
		case Directions.kLeft:
			return new Vector3(-1, 0, 0);
		}
		return Vector3.zero;
	}
	
	
	public static bool IsInSameDirection(Vector3 start, Vector3 end, int dir){
		switch (dir){
		case Directions.kUp:{
			return (end.y > start.y);
		}
		case Directions.kRight:{
			return (end.x > start.x);
		}
		case Directions.kDown:{
			return (end.y < start.y);
		}
		case Directions.kLeft:{
			return (end.x < start.x);
		}
		}
		return false;
	}
	
	public static bool IsOppositeDirection(int dir0, int dir1){
		return ((dir0 - dir1) + Directions.kNumDirections) % Directions.kNumDirections == 2;
	}
	
	
	public static int GetDirectionTowards(Vector3 startPos, Vector3 endPos, int ignoreDir){
		for (int i = 0; i < kNumDirections; ++i){
			int testDir = i;
			if (testDir == ignoreDir) continue;
			
			if (IsInSameDirection(startPos, endPos,  testDir)) return testDir;
		}
		return Directions.kNull;
		
	}
	
	public static int GetDominantDirection(Vector3 dirVec){
		float bestScore = -1;
		int bestDir = kNull;
		dirVec.Normalize();

		for (int i = 0; i < kNumDirections; ++i){
			Vector3 testDir = GetDirVec(i);
			float score = Vector3.Dot(dirVec, testDir);
			if (score > bestScore){
				bestScore = score;
				bestDir = i;
			}
		}
		return bestDir;
			
		
	}
	
	public static int GetStrictDirection(Vector3 startPos, Vector3 endPos){
		if (MathUtils.FP.Feq(startPos.x, endPos.x)){
			if (MathUtils.FP.Feq(startPos.y, endPos.y)) return Directions.kNull;
			if (endPos.y > startPos.y) return Directions.kUp;
			if (endPos.y < startPos.y) return Directions.kDown;
			
		}
		if (MathUtils.FP.Feq(startPos.y, endPos.y)){
			if (MathUtils.FP.Feq(startPos.x, endPos.x)) return Directions.kNull;
			if (endPos.x > startPos.x) return Directions.kRight;
			if (endPos.x < startPos.x) return Directions.kLeft;
		}
		DebugUtils.Assert(false, "GetStrictDirection - passed a non virtical or horizontal vector");
		return Directions.kNull;
		
	}
	
	
	public static int CalcOppDir(int dir){
		return (dir + 2) % Directions.kNumDirections;
	}

}
