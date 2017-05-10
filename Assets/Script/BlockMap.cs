using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockMap : MonoBehaviour {

	public Transform blockPrefab;
	public Vector3 mapSize;
	public Vector3 startBlock;
	List<bool[,]> allFloors;
	Queue<Coord2> shuffledBlockCoords;
	public int seed = 10;
	[RangeAttribute(0,1)]
	public float blockPercent = 1;
	Coord2 startPoint;

	void Start () {
		GenerateMap();
	}

	public void GenerateMap() {
		allFloors = new List<bool[,]>();
		startPoint = new Coord2((int)startBlock.x, (int)startBlock.z);
		if(blockPercent<=0.1f)
			blockPercent = 0.1f;

		int[,] availableBlock = new int[(int)mapSize.y,3];
		int[] availableArea = new int[(int)mapSize.y];

		availableArea[0] = availableArea.GetLength(0);
		for(int i=1; i<availableArea.GetLength(0); i++){
			int result = availableArea[i-1] - Utility.randomNumber(0,2,seed+i);
			if(result == 0)
				result = 1;
			availableArea[i] = result;
		}

		for(int i=0; i<availableBlock.GetLength(0); i++){
			int minResult = (int)Mathf.Sqrt(mapSize.x * availableArea[i]);
			int maxResult = (int)(minResult * minResult * blockPercent);
			if(maxResult < minResult + 1)
				maxResult = minResult + 1;
			if(maxResult > (int)mapSize.x * availableArea[i])
				maxResult = (int)mapSize.x * availableArea[i];
			availableBlock[i,0] = minResult;
			availableBlock[i,1] = maxResult;
		}

		for(int i=0; i<availableBlock.GetLength(0); i++){
			availableBlock[i,2] = Utility.randomNumber(availableBlock[i,0], availableBlock[i,1], seed);
			// print(i + " " + availableBlock[i,0] + " " + availableBlock[i,1] + " " + availableBlock[i,2]);
			// print(i + "availableArea : " + availableArea[i]);
		}
		

		//Inheritance
		string holderName = "Generated Map";
		if(transform.FindChild(holderName)) {
			DestroyImmediate(transform.FindChild(holderName).gameObject);
		}
		Transform mapHolder = new GameObject(holderName).transform;
		mapHolder.parent = transform;

		//set block on first floor
		allFloors.Add(baseBlockAccessible(availableBlock));
		for(int i=1; i<mapSize.y; i++)
		allFloors.Add(blockAccessible(allFloors, i, availableBlock, availableArea));

		for (int y=0; y<mapSize.y; y++){
			for (int x=0; x<mapSize.x; x++){
				for (int z=0; z<mapSize.z; z++){
					if(allFloors[y][x,z]){
						Vector3 blockPosition = CoordToPosition(x, y, z);
						Transform newBlock = Instantiate(blockPrefab, blockPosition + Vector3.up, Quaternion.identity);
						newBlock.parent = mapHolder;
					}
				}
			}
		}
	}

	bool[,] baseBlockAccessible(int[,] availableBlock) {
		bool[,] eachFloorBlocks = new bool[(int)mapSize.x, (int)mapSize.z];
		List<Coord2> list = new List<Coord2> ();

		eachFloorBlocks[startPoint.x, startPoint.z] = true;
		Coord2 block = startPoint;

		int check = 1;
		int count = 0;

		for(int i=0; i<eachFloorBlocks.GetLength(0) * eachFloorBlocks.GetLength(1); i++){
			for(int x=-1; x<=1; x++){
				for(int z=-1; z<=1; z++){		
					int neighbourX = block.x + x;
					int neighbourZ = block.z + z;
					if(Mathf.Abs(x) != Mathf.Abs(z)){
						if(neighbourX >= 0 && neighbourX < eachFloorBlocks.GetLength(0) && neighbourZ >= 0 && neighbourZ < eachFloorBlocks.GetLength(1)) {
							if(!eachFloorBlocks[neighbourX, neighbourZ]) {
								list.Add(new Coord2(neighbourX, neighbourZ));
							}
						}
					}
				}
			}
			list.Add(block);
			int randNum = Utility.randomNumber(list.Count, seed+count);
			Coord2 buf = list[randNum];
			list.Clear();
			if(!eachFloorBlocks[buf.x, buf.z]) {
				eachFloorBlocks[buf.x, buf.z] = true;
				check ++;
			}
			block = buf;
			count++;
			if(check >= availableBlock[0,2])
				break;
		}
		//print(0 + " Count : " + count);
		return eachFloorBlocks;
	}

	bool[,] blockAccessible(List<bool[,]> allFloors, int y, int[,] availableBlock, int[] availableArea) {
		bool[,] beforeFloorBlocks = allFloors[y-1];
		bool[,] canFloorBlocks = new bool[(int)mapSize.x, (int)mapSize.z];
		List<Coord2> list = new List<Coord2> ();
		Queue<Coord2> queue;
		int count = 0;

		for (int xSize=0; xSize<mapSize.x; xSize++){
			for (int zSize=0; zSize<mapSize.z; zSize++){
				if(beforeFloorBlocks[xSize,zSize]){
					for(int x=-1; x<=1; x++){
						for(int z=-1; z<=1; z++){
							int neighbourX = xSize + x;
							int neighbourZ = zSize + z;
							if(Mathf.Abs(x) != Mathf.Abs(z) || x==0 && z==0){
								// if(neighbourX >= 0+availableArea[y] && neighbourX < beforeFloorBlocks.GetLength(0) && neighbourZ >= 0+availableArea[y] && neighbourZ < beforeFloorBlocks.GetLength(1)) {
								if(neighbourX >= 0 && neighbourX < beforeFloorBlocks.GetLength(0) && neighbourZ >= 0 && neighbourZ < beforeFloorBlocks.GetLength(1)) {	
									list.Add(new Coord2(neighbourX, neighbourZ));
								}
							}
						}
					}
				}
			}
		}
		if(list.Count > 0){
			queue = new Queue<Coord2>(Utility.ShuffleArray(list.ToArray(), seed));
			for(int i=0; i<availableBlock[y,2]; i++){
				if(queue.Count == 0)
					break;
				Coord2 buf = queue.Dequeue();
				if(availableArea[0] - availableArea[y] <= buf.z){
					canFloorBlocks[buf.x, buf.z] = true;
					count++;
				}else{
					canFloorBlocks[buf.x, buf.z] = false;
					count--;
				}
			}
		}
		//print(y + " Count : " + count);
		return canFloorBlocks;
	}

	Vector3 CoordToPosition(int x, int y, int z) {
		return new Vector3(-mapSize.x/2 + 0.5f + x, y, -mapSize.z/2 + 0.5f + z);
	}

	public Coord2 GetRandomCoord() {
		Coord2 randomCoord = shuffledBlockCoords.Dequeue ();
		shuffledBlockCoords.Enqueue(randomCoord);
		return randomCoord;
	}
	public struct Coord3 {
		public int x;
		public int y;
		public int z;

		public Coord3(int x, int y, int z) {
			this.x = x;
			this.y = y;
			this.z = z;
		}
	}

	public struct Coord2 {
		public int x;
		public int z;

		public Coord2(int x, int z) {
			this.x = x;
			this.z = z;
		}
	}
}
