using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputController : MonoBehaviour
{
    [HideInInspector]
    public bool CanInput = true;
    [HideInInspector]
	public PlayerController PlayerCtrl = null;

    public float InputDelay = 0.3f;

    //연속 키 입력을 방지하기 위한 장치
    private bool[] inputDelay = new bool[10];

    private void Update()
    {
        if (CanInput)
        {
            if (Input.GetKey(KeyCode.UpArrow) && !inputDelay[0])
            {
                inputDelay[0] = true;
                StartCoroutine(InputWaitForSecond(0));
                PlayerCtrl.Move(Vector2.up);
            }
            else if (Input.GetKey(KeyCode.DownArrow) && !inputDelay[0])
            {
                inputDelay[0] = true;
                StartCoroutine(InputWaitForSecond(0));
                PlayerCtrl.Move(-Vector2.up);
            }
            else if (Input.GetKey(KeyCode.RightArrow) && !inputDelay[0])
            {
                inputDelay[0] = true;
                StartCoroutine(InputWaitForSecond(0));
                PlayerCtrl.Move(Vector2.right);
            }
            else if (Input.GetKey(KeyCode.LeftArrow) && !inputDelay[0])
            {
                inputDelay[0] = true;
                StartCoroutine(InputWaitForSecond(0));
                PlayerCtrl.Move(-Vector2.right);
            }
        }
    }

    IEnumerator InputWaitForSecond(int i)
    {
        yield return new WaitForSeconds(InputDelay);
        inputDelay[i] = false;
    }
}
