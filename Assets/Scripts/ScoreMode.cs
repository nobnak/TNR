using UnityEngine;
using System.Collections;
using TouchScript.Gestures;

public class ScoreMode : MonoBehaviour {
	public enum State { Off = 0, On = 1 }
	public int tempo = 75;
	public int loop = 16;
	
	public StateMaterials[] stateMaterials;
	public AudioClip[] tones;
	
	private CubeMatrix _matrix;
	private System.DateTime _standardTime;
	private double _step2mills, _mills2step;
	private int _lastX, _currX;
	private CubeInfo[,] _cubeInfos;
	private int _cubeLayerMask;
	private AudioSource[] _audioSources;
	
	void Awake() {
		_standardTime = HighResTime.UtcNow;
		_step2mills = 60000.0 / (4 * tempo);
		_mills2step = 1.0 / _step2mills;
		_cubeInfos = new CubeInfo[16, 16];
		_lastX = _currX = GetCurrX();
		_cubeLayerMask = (1 << LayerMask.NameToLayer("CubeMatrix"));
		_audioSources = new AudioSource[32];
		for (var i = 0; i < _audioSources.Length; i++)
			_audioSources[i] = gameObject.AddComponent<AudioSource>();
	}

	void Start () {
		_matrix = GetComponent<CubeMatrix>();
		for (var y = 0; y < 16; y++) {
			for (var x = 0; x < 16; x++) {
				var cube = _matrix[x, y];
				var cubeInfo = new CubeInfo(){ state = State.Off, materials = stateMaterials[0], renderer = cube.GetComponent<Renderer>() };
				_cubeInfos[x, y] = cubeInfo;
			}
		}
	}
	
	void Update () {
		UpdateInput();
		UpdateCubeMatrix();
	}

	void UpdateInput () {
		if (Input.GetMouseButtonUp(0)) {
			var screenPos = Input.mousePosition;
			RaycastHit hit;
			if (Physics.Raycast(Camera.main.ScreenPointToRay(screenPos), out hit, _cubeLayerMask)) {
				var cube = hit.transform.gameObject;
				var index = _matrix[cube];
				Debug.Log(index);
				FlipState(index.x, index.y);
				UpdateMaterial(index.x, index.y);
			}
		}
	}

	void UpdateCubeMatrix () {
		_currX = GetCurrX ();
		if (_currX == _lastX)
			return;
		
		for (var y = 0; y < 16; y++) {
			UpdateMaterial(_lastX, y);
			UpdateMaterial(_currX, y);
		}
		
		_lastX = _currX;
	}
	
	void FlipState(int x, int y) {
		var cubeInfo = _cubeInfos[x, y];
		var nextState = cubeInfo.state == State.Off ? State.On : State.Off;
		cubeInfo.state = nextState;
		cubeInfo.materials = stateMaterials[(int)nextState];
	}
	void UpdateMaterial(int x, int y) {
		var cubeInfo = _cubeInfos[x, y];
		var active = (x == _currX);
		cubeInfo.renderer.sharedMaterial = active ? cubeInfo.materials.matActive : cubeInfo.materials.matDefault;
	}

	int GetCurrX () {
		var ticks = HighResTime.UtcNow - _standardTime;
		var steps = ticks.TotalMilliseconds * _mills2step;
		return (int)(steps % loop);
	}
	
	[System.Serializable]
	public class StateMaterials {
		public Material matDefault;
		public Material matActive;
	}
	
	public class CubeInfo {
		public State state;
		public StateMaterials materials;
		public Renderer renderer;
	}
}
