#region

using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

#endregion

public class PlayerInput : MonoBehaviour {
	public enum AlternateCode {
		ScrollUp = -1,
		ScrollDown = -2
	}

	public static PlayerInput I;

	public int tickCount;

	public bool usingAnalog;

	public bool Enabled;

	public Dictionary<int, int> keyPressTimestamps = new Dictionary<int, int>();

	public Dictionary<int, int> keyReleaseTimestamps = new Dictionary<int, int>();

	PropertyInfo[] properties;

	public static int MoveForward {
		get => PlayerPrefs.GetInt("MoveForward", 119);
		private set => PlayerPrefs.SetInt("MoveForward", value);
	}

	public static int MoveBackward {
		get => PlayerPrefs.GetInt("MoveBackward", 115);
		private set => PlayerPrefs.SetInt("MoveBackward", value);
	}

	public static int MoveRight {
		get => PlayerPrefs.GetInt("MoveRight", 100);
		private set => PlayerPrefs.SetInt("MoveRight", value);
	}

	public static int MoveLeft {
		get => PlayerPrefs.GetInt("MoveLeft", 97);
		private set => PlayerPrefs.SetInt("MoveLeft", value);
	}

	public static int RestartLevel {
		get => PlayerPrefs.GetInt("RestartLevel", 114);
		private set => PlayerPrefs.SetInt("RestartLevel", value);
	}

	public static int PrimaryInteract {
		get => PlayerPrefs.GetInt("PrimaryInteract", 323);
		private set => PlayerPrefs.SetInt("PrimaryInteract", value);
	}

	public static int SecondaryInteract {
		get => PlayerPrefs.GetInt("SecondaryInteract", 324);
		private set => PlayerPrefs.SetInt("SecondaryInteract", value);
	}

	public static int TertiaryInteract {
		get => PlayerPrefs.GetInt("TertiaryInteract", 113);
		private set => PlayerPrefs.SetInt("TertiaryInteract", value);
	}

	public static int Jump {
		get => PlayerPrefs.GetInt("Jump", 32);
		private set => PlayerPrefs.SetInt("Jump", value);
	}

	public static int Pause {
		get => PlayerPrefs.GetInt("Pause", 27);
		private set => PlayerPrefs.SetInt("p", value);
	}

	public struct Keybind {
		public string Name { get; }
		public int DefaultKeyCode { get; }

		public Keybind(string name, int defaultKeyCode)
		{
			Name = name;
			DefaultKeyCode = defaultKeyCode;
		}

		public void Reset() {
			KeyCode = DefaultKeyCode;
		}
		public int KeyCode {
			get => PlayerPrefs.GetInt(Name, DefaultKeyCode); 
			set => PlayerPrefs.SetInt(Name, value);
		}
	}
	
	public List<Keybind> Keybinds = new List<Keybind> {
		new Keybind("MoveForward", 119),// {Name = ,
		new Keybind("MoveBackward", 115),
		new Keybind("MoveRight", 100),
		new Keybind("MoveLeft", 97),
		new Keybind("RestartLevel", 114) ,
		new Keybind("PrimaryInteract", 323),
		new Keybind("SecondaryInteract", 324) ,
		new Keybind("TertiaryInteract", 113),
		new Keybind("Jump",231) ,
		new Keybind("Pause", 27) ,
	};
	
	public void SetKeybind(string name, int keyCode) {
		Keybind keybind = Keybinds[Keybinds.FindIndex(x => x.Name == name)];
		keybind.KeyCode = keyCode;
		Keybinds[Keybinds.FindIndex(x => x.Name == name)] = keybind;
	}
	
	public void UpdateKeybinds() {
		foreach (Keybind keybind in Keybinds) {
			keybind.Reset();
		}
	}

	void Awake () {
		if (I == null) {
			I= this;
		}
	}

	void Start () {
		if (I == null) {
			DontDestroyOnLoad(gameObject);
			I = this;
			properties = typeof(PlayerInput).GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Static | BindingFlags.Public);
			
		} else if (I != this) {
			I.Start();
			Destroy(gameObject);
			return;
		}
		
		properties = typeof(PlayerInput).GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Static | BindingFlags.Public);
		tickCount = 0;
		keyPressTimestamps = new Dictionary<int, int>();
		keyReleaseTimestamps = new Dictionary<int, int>();
	}

	void Update () {
		for (int i = 0; i < properties.Length; i++) {
			int num = (int)properties[i].GetValue(null, null);
			if (num != Pause && Time.timeScale == 0f) {
				continue;
			}

			if (num > 0) {
				if (Input.GetKeyDown((KeyCode)num)) {
					keyPressTimestamps[num] = tickCount;
				}

				if (Input.GetKeyUp((KeyCode)num)) {
					keyReleaseTimestamps[num] = tickCount;
				}

				continue;
			}

			switch (num) {
			case -1:
				if (Input.mouseScrollDelta.y > 0f) {
					keyPressTimestamps[num] = tickCount;
				}

				break;
			case -2:
				if (Input.mouseScrollDelta.y < 0f) {
					keyPressTimestamps[num] = tickCount;
				}

				break;
			}
		}
	}

	void FixedUpdate () {
		usingAnalog = Mathf.Abs(Input.GetAxis("Joy 1 Y")) > 0.1f || Mathf.Abs(Input.GetAxis("Joy 1 X")) > 0.1f;
		tickCount++;
	}

	int GetPressTimestamp (int key) {
		if (keyPressTimestamps.TryGetValue(key, out var value)) {
			return value;
		}

		return -1;
	}

	public int SincePressed (int key) {
		int pressTimestamp = GetPressTimestamp(key);
		if (pressTimestamp == -1) {
			return int.MaxValue;
		}

		return tickCount - pressTimestamp;
	}

	int GetReleaseTimestamp (int key) {
		if (keyReleaseTimestamps.TryGetValue(key, out var value)) {
			return value;
		}

		return -1;
	}

	public int SinceReleased (int key) {
		int releaseTimestamp = GetReleaseTimestamp(key);
		if (releaseTimestamp == -1) {
			return int.MaxValue;
		}

		return tickCount - releaseTimestamp;
	}

	public void ConsumeBuffer (int key) {
		keyPressTimestamps[key] = -1;
		keyReleaseTimestamps[key] = -1;
	}

	public bool IsKeyPressed (int key) {
		return SincePressed(key) < SinceReleased(key);
	}

	public float GetAxisStrafeRight () {
		float num = !Input.GetKey((KeyCode)MoveRight) ? IsKeyPressed(MoveLeft) ? -1 : 0 : (float)(!IsKeyPressed(MoveLeft) ? 1 : 0);
		if (num == 0f) {
			num = Input.GetAxis("Joy 1 X");
		}

		return num;
	}

	public float GetAxisStrafeForward () {
		float num = !Input.GetKey((KeyCode)MoveForward) ? IsKeyPressed(MoveBackward) ? -1 : 0 : (float)(!IsKeyPressed(MoveBackward) ? 1 : 0);
		if (num == 0f) {
			num = 0f - Input.GetAxis("Joy 1 Y");
		}

		return num;
	}

	public void SimulateKeyPress (int key) {
		keyPressTimestamps[key] = tickCount;
	}

	public void SimulateKeyRelease (int key) {
		keyReleaseTimestamps[key] = tickCount;
	}

	public static int GetBindByName (string name) {
		PropertyInfo[] properties = typeof(PlayerInput).GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Static | BindingFlags.Public);
		foreach (PropertyInfo obj in properties) {
			KeyCode result = (KeyCode)obj.GetValue(null, null);
			if (obj.Name == name) {
				return (int)result;
			}
		}

		return 0;
	}

	static string MouseStringToClick (string strung) {
		if (strung == "Mouse1") {
			return "Right Click";
		}
		if (strung == "Mouse0") {
			return "Left Click";
		}

		return strung;
	}

	public static string GetBindName (int key) {
		if (key >= 0) {
			KeyCode keyCode = (KeyCode)key;
			return MouseStringToClick(keyCode.ToString());
		}

		AlternateCode alternateCode = (AlternateCode)key;
		return alternateCode.ToString();
	}

	public static void SetBind (string key, KeyCode bind) {
		SetBind(key, (int)bind);
	}

	public static void SetBind (string key, AlternateCode bind) {
		SetBind(key, (int)bind);
	}

	static void SetBind (string key, int bind) {
		PropertyInfo[] properties = typeof(PlayerInput).GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Static | BindingFlags.Public);
		foreach (PropertyInfo propertyInfo in properties) {
			if ((int)propertyInfo.GetValue(null, null) == bind) {
				propertyInfo.SetValue(null, KeyCode.None);
			}
		}

		typeof(PlayerInput).GetProperty(key).SetValue(null, bind);
	}

	public static void ResetBindsToDefault () {
		PropertyInfo[] properties = typeof(PlayerInput).GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Static | BindingFlags.Public);
		for (int i = 0; i < properties.Length; i++) {
			PlayerPrefs.DeleteKey(properties[i].Name);
		}
	}
}