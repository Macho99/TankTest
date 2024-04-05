using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

public class CamMover : MonoBehaviour
{
	[SerializeField] float moveSpeed = 4f;
	[SerializeField] float lookSpeed = 5f;
	Vector2 moveInput;
	Vector2 lookInput;
	float yAngle;
	float xAngle;

	private void OnLook(InputValue value)
	{
		lookInput = value.Get<Vector2>();
	}

	private void OnMove(InputValue value)
	{
		moveInput = value.Get<Vector2>();
	}

	private void Update()
	{
		transform.Translate(new Vector3(moveInput.x, 0f, moveInput.y) * moveSpeed * Time.deltaTime, Space.Self);
		yAngle -= lookInput.y * Time.deltaTime * lookSpeed;
		xAngle += lookInput.x * Time.deltaTime * lookSpeed;
		yAngle = Mathf.Clamp(yAngle, -80f, 40f);
		transform.rotation = Quaternion.Euler(yAngle, xAngle, 0f);
	}
}
