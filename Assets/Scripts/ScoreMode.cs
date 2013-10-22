using UnityEngine;
using System.Collections;
using TouchScript.Gestures;

public class ScoreMode : MonoBehaviour {
	public enum State { Off = 0, On = 1 }
	
	public int tempo = 300;
	public int height = 16;
	public int loop = 16;
	public int nAudioBuffers = 2;
	
	public StateMaterials[] stateMaterials;
	public AudioClip[] tones;
	
	private CubeMatrix _matrix;
	private Metronome _metronome;
	private int _lastX;
	private CubeInfo[,] _cubeInfos;
	private int _cubeLayerMask;
	private AudioSource[] _audioSources;
	
	void Awake() {
		_metronome = new Metronome(HighResTime.UtcNow, 60 * HighResTime.SECOND2TICK / tempo, loop);
		_metronome.Update();
		_lastX = _metronome.x;
		_cubeInfos = new CubeInfo[loop, height];
		_cubeLayerMask = (1 << LayerMask.NameToLayer("CubeMatrix"));
		_audioSources = new AudioSource[nAudioBuffers * height];
		for (var i = 0; i < _audioSources.Length; i++)
			_audioSources[i] = gameObject.AddComponent<AudioSource>();
	}

	void Start () {
		_matrix = GetComponent<CubeMatrix>();
		for (var y = 0; y < height; y++) {
			for (var x = 0; x < loop; x++) {
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
				FlipState(index.x, index.y);
				UpdateMaterial(index.x, index.y);
			}
		}
	}

	void UpdateCubeMatrix () {
		_metronome.Update();
		if (_metronome.x == _lastX)
			return;
		
		var nextX = (_metronome.x + 1) % loop;
		for (var y = 0; y < height; y++) {
			UpdateMaterial(_lastX, y);
			UpdateMaterial(_metronome.x, y);
			var cubeInfo = _cubeInfos[nextX, y];
			if (cubeInfo.state == State.On)
				PlayScheduled(nextX, y, _metronome.Step2DspTime(_metronome.steps + 1));
		}
		
		_lastX = _metronome.x;
	}
	
	void FlipState(int x, int y) {
		var cubeInfo = _cubeInfos[x, y];
		var nextState = cubeInfo.state == State.Off ? State.On : State.Off;
		cubeInfo.state = nextState;
		cubeInfo.materials = stateMaterials[(int)nextState];
	}
	void UpdateMaterial(int x, int y) {
		var cubeInfo = _cubeInfos[x, y];
		var active = (x == _metronome.x);
		cubeInfo.renderer.sharedMaterial = active ? cubeInfo.materials.matActive : cubeInfo.materials.matDefault;
	}
	void PlayScheduled(int x, int y, double scheduledTime) {
		var audio = _audioSources[y + (x % nAudioBuffers) * height];
		audio.clip = tones[y];
		audio.PlayScheduled(scheduledTime);
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
	
	public class Metronome {
		public long ticks;
		public long steps;
		public int x;
		public double dspTime;
		
		public System.DateTime standardTime;
		public long step2tick;
		public int loop;
		
		public Metronome(System.DateTime standardTime, long step2tick, int loop) {
			this.standardTime = standardTime;
			this.step2tick = step2tick;
			this.loop = loop;
		}
		
		public void Update() {
			ticks = (HighResTime.UtcNow - standardTime).Ticks;
			steps = Tick2Step(ticks);
			x = Step2X(steps);
			dspTime = AudioSettings.dspTime;
		}
		
		public long Tick2Step(long ticks) {
			return ticks / step2tick;
		}
		public long Step2Tick(long steps) {
			return steps * step2tick;
		}
		public int Step2X(long steps) {
			return (int)(steps % loop);
		}
		public double Step2DspTime(long steps) {
			var nextTick = Step2Tick(steps);
			return dspTime + (nextTick - ticks) * HighResTime.TICK2SECOND;
		}
	}
}
