using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
	[HideInInspector]
	public GameObject PlayerObject;

	// 캐릭터의 위에 따라 카메라가 이동하도록 하는 메서드
	void FixedUpdate()
	{
        Vector2 desPos = Vector2.Lerp(transform.position, PlayerObject.transform.position, 0.1f);
        GetComponent<Rigidbody2D>().MovePosition(desPos);
	}
}