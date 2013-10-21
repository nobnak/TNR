using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Linq;

public class CubeMatrixGenerator {
	
	[MenuItem("Custom/GenerateCubeMatrix")]
	public static void GenerateCubeMatrix() {
		var parent = GameObject.Find("CubeMatrix");
		var cubeMatrix = parent.GetComponent<CubeMatrix>();
		
		var children = parent.GetComponentsInChildren<Transform>().Where((t) => t.gameObject != parent).Select((t) => t.gameObject);
		foreach (var child in children)
			GameObject.DestroyImmediate(child);
		
		cubeMatrix.index2cube = new GameObject[16 * 16];
		
		var dx = cubeMatrix.size / 16;
		var offset = dx * 0.5f;
		for (var y = 0; y < 16; y++) {
			for (var x = 0; x < 16; x++) {
				var pos = new Vector3(offset + dx * x, offset + dx * y, 0f);
				var cube = (GameObject)GameObject.Instantiate(cubeMatrix.cubepref);
				cube.transform.parent = cubeMatrix.transform;
				cube.transform.localPosition = pos;
				cube.name = string.Format("Cube {0:d2}x{1:d2}", x, y);
				cubeMatrix[x, y] = cube;
			}
		}
	}
}
