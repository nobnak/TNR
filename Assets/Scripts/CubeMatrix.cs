using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CubeMatrix : MonoBehaviour {
	public float size;
	public GameObject cubepref;
	public GameObject[] index2cube;
	
	private Dictionary<GameObject, Pos> _cube2index;
	
	void Awake() {
		_cube2index = new Dictionary<GameObject, Pos>();
		for (var i = 0; i < index2cube.Length; i++)
			_cube2index[index2cube[i]] = new Pos(){ x = i / 16, y = i % 16 };
	}
	
	public GameObject this[int x, int y] {
		get { return index2cube[y + x * 16]; }
		set { index2cube[y + x * 16] = value; }
	}
	public Pos this[GameObject cube] {
		get { return _cube2index[cube]; }
	}
	
	public struct Pos {
		public int x, y;
	}
}
