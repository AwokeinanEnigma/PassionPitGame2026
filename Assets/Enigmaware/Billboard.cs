using UnityEngine;

public class Billboard : MonoBehaviour
{
	private Camera _mainCamera;

	void Start() {
		_mainCamera = Camera.main;
	}

	void Update()
	{
		Vector3 direction = _mainCamera.transform.position - transform.position;
		Quaternion targetRotation = Quaternion.LookRotation(-direction);
		transform.localRotation = targetRotation;
//		Debug.Log(targetRotation);
	}
}
