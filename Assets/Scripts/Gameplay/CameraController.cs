using UnityEngine;

public class CameraController : MonoBehaviour
{
	[SerializeField]
	Vector2 m_cameraScrollSpeed;

	[SerializeField]
	float m_cameraRotateSpeed;

	// Update is called once per frame
	void Update()
	{
		float horizontalMovement = Input.GetAxis("Horizontal") * m_cameraScrollSpeed.x * Time.deltaTime;
		float verticalMovement = Input.GetAxis("Vertical") * m_cameraScrollSpeed.y * Time.deltaTime;

		Vector3 cameraForward = transform.forward;
		Vector3 flattednedCameraForward = new(cameraForward.x, 0f, cameraForward.z);
		flattednedCameraForward.Normalize();

		transform.position += transform.right * horizontalMovement + flattednedCameraForward * verticalMovement;

		// todo: rotate camera around map center. give illusion of map rotating
		// todo: camera bounds
		float rotation = Input.GetAxis("Rotate") * m_cameraRotateSpeed * Time.deltaTime;
		Vector3 cameraOrientation = transform.rotation.eulerAngles;
		cameraOrientation.y += rotation;
		transform.rotation = Quaternion.Euler(cameraOrientation);
	}
}
