using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms;

public class PlayerController : MonoBehaviour
{
    public bool CanMove = true;
    private Vector2 preDestination;
    private Vector2 destination;
    //시야 체크용
    private bool canSee = true;

    [Range(0.1f,0.9f)]
    public float MoveSpeed = 0.5f;

    private void Awake()
    {
        //시작하면서 플레이어 콘트롤러를 직접 붙여줌
        GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraController>().PlayerObject = gameObject;
        GameObject.Find("Controllers").GetComponent<InputController>().PlayerCtrl = this;
        transform.position = new Vector3(10, 10, 0);
        destination = new Vector3(10, 10, 0);
    }

    private void Update()
    {
        if ((Vector2)transform.position != destination)
        {
            //이동 중
            Vector2 desPos = Vector2.Lerp(transform.position, destination, MoveSpeed);
            //Vector2 desPos = Vector2.MoveTowards(transform.position, destination, MoveSpeed);
            GetComponent<Rigidbody2D>().MovePosition(desPos);
        }
        else
        {
            //도착한 상태
            CanMove = true;
            //플레이어 위치 int로 보정
            transform.position = new Vector3(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y), 0);
            //도착 후 시야 확인. 이게 왜 타이밍 맞게 잘 되는지 모르겠음
            if (canSee)
                CheckFieldOfView();
        }
    }

    public void Move(Vector2 dir)
    {
        if (CanMove)
        {
            //가려고 하는 목표지점
            preDestination = new Vector2(transform.position.x + dir.x, transform.position.y + dir.y);
            //맵 밖으로 나가는 게 아니라면
            if (TileController.Instance.CheckGoOutOfMap(preDestination))
            {
                //목표 지점은 이동할 수 있다면
                if (!TileController.Instance.CheckCanGoInside(preDestination))
                {
                    CanMove = false;
                    destination = preDestination;
                    canSee = true;
                }
            }
        }
    }

    //시야 요청. 멈춰있을 때 시야를 요청하는데, 한 번만 요청해야 함
    private void CheckFieldOfView()
    {
        canSee = false;
        FogContoroller.Instance.ClearFogAroundPoint(destination, 4);
    }
}