using Sirenix.OdinInspector.Editor.Validation;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class FogContoroller : MonoBehaviour
{
    public static FogContoroller Instance = null;

    [SerializeField]
    private GameObject FogObject = null;
    //안개 배열의 크기
    private int fogArraySize;
    private SpriteRenderer[,] FogSpriteGrid = null;
    //임시 리스트. 찾은 타일 위치를 임시로 저장하고 비움
    private List<Vector2> tempPosList = new List<Vector2>();
    //임시 리스트2. 거리 비교해 가까운 순서대로 재정렬할 때 사용
    private List<float> tempPosList2 = new List<float>();
    //임시 리스트3. 직선이 지나치는 타일의 리턴용 리스트
    private List<Vector2> tempListTileBelowLine = new List<Vector2>();
    //기준점에서 최대 시야에 위치한 타일 목록. 이 목록을 사용해 직선을 그림
    private List<Vector2> OutermostTileList = new List<Vector2>();
    //계산이 완료된 직선 내 타일 목록
    private List<Vector2> ListTileBelowLine = new List<Vector2>();

    //디버그용 변수
    private Vector2 debugStartPos;
    private List<Vector2> debugTargetPosList = new List<Vector2>();

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        //맵 크기를 받아와 배열 초기화
        fogArraySize = GameRule.Instance.ChunkNumberOnMap * GameRule.Instance.TileNumberOnChunk;
        FogSpriteGrid = new SpriteRenderer[fogArraySize, fogArraySize];

        //안개 초기 배치
        FogInitialGeneration();
    }

    //디버그 선 긋기
    private void OnDrawGizmos()
    {
        for (int i = 0; i < debugTargetPosList.Count; i++)
        {
            Gizmos.color = new Color(0.5f, 1, 0.5f, 1);
            Gizmos.DrawLine(debugStartPos, debugTargetPosList[i]);
        }
    }

    //안개 타일 초기 배치하기
    private void FogInitialGeneration()
    {
        for (int i = 0; i < fogArraySize; i++)
        {
            for (int j = 0; j < fogArraySize; j++)
            {
                Vector3 pos = new Vector3(i, j, 0);
                GameObject fogObject = Instantiate(FogObject, pos, Quaternion.identity);
                fogObject.transform.parent = transform;
                SpriteRenderer spriteRenderer = fogObject.GetComponent<SpriteRenderer>();
                spriteRenderer.color = new Color(0, 0, 0, 1f);
                FogSpriteGrid[i, j] = spriteRenderer;
            }
        }
    }

    //플레이어 위치를 중심으로 최외곽 타일을 구한 후, 그 사이 시야를 밝히기
    public void ClearFogAroundPoint(Vector2 playerPos, int sight)
    {
        //최외곽타일 리스트를 초기화하고
        OutermostTileList.Clear();

        //시야로 쓰일 사각형을 먼저 구하자
        Vector2 minPos = new Vector2((int)playerPos.x - sight, (int)playerPos.y - sight);
        Vector2 maxPos = new Vector2((int)playerPos.x + sight, (int)playerPos.y + sight);

        //먼저 다시 안개를 채우고
        FillFog(minPos, maxPos);

        //네 번의 for문으로 최외곽 타일을 구함
        for (int i = (int)minPos.x; i < (int)maxPos.x; i++)
        {
            OutermostTileList.Add(FindTilesInsideMap(i, minPos.y));
        }
        for (int i = (int)minPos.x+1; i < (int)maxPos.x+1; i++)
        {
            OutermostTileList.Add(FindTilesInsideMap(i, maxPos.y));
        }
        for (int i = (int)minPos.y; i < (int)maxPos.y; i++)
        {
            OutermostTileList.Add(FindTilesInsideMap(maxPos.x, i));
        }
        for (int i = (int)minPos.y+1; i < (int)maxPos.y+1; i++)
        {
            OutermostTileList.Add(FindTilesInsideMap(minPos.x, i));
        }

        //디버그 라인 초기화
        debugStartPos = new Vector2(playerPos.x + 0.5f, playerPos.y + 0.5f);
        debugTargetPosList.Clear();

        //중심점부터 최외곽타일 직선 내 타일은 모두 밝히기
        for (int i = 0; i < OutermostTileList.Count; i++)
        {
            //최외곽 타일 하나를 사용해, 사이에 있는 타일을 목록에 넣음
            //선분이 두 타일 사이로 빠져나갈 수 있기 때문에, 여기서 점을 상하좌우로 약간 움직여 총 4번 판정함
            for (int j = 1; j < 5; j++)
            {
                ListTileBelowLine.Clear();
                ListTileBelowLine = SearchingForTileLocationsBelowLine(playerPos, OutermostTileList[i], j);
                MakeClearTileBeforreColliding(ListTileBelowLine);
            }
        }
        //마지막으로 플레이어가 서있는 위치 밝히기
        FogSpriteGrid[(int)playerPos.x, (int)playerPos.y].color = new Color(255, 255, 255, 0.0f);
    }
    //하나의 라인에서 충돌하기 전까지 타일 밝히기
    private void MakeClearTileBeforreColliding(List<Vector2> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int x = (int)list[i].x;
            int y = (int)list[i].y;
            FogSpriteGrid[x, y].color = new Color(255, 255, 255, 0.0f);
            //너머로 볼 수 없다면 멈춰서 그 뒤의 타일을 밝히지 않는다
            if (TileController.Instance.TileGrid[x, y].CantSeeBeyond)          
                return;
        }
    }
    //맵 내부 타일을 찾아 반환하기
    private Vector2 FindTilesInsideMap(float a, float b)
    {
        float x = Mathf.Clamp(a, 0, fogArraySize-1);
        float y = Mathf.Clamp(b, 0, fogArraySize-1);
        return new Vector2(x, y);
    }
    //주변 안개 타일 깔기
    public void FillFog(Vector2 minPos, Vector2 maxPos)
    {
        ////맵 전체 안개를 어둡게 만들고
        //for (int i = 0; i < FogArraySize; i++)
        //{
        //    for (int j = 0; j < FogArraySize; j++)
        //    {
        //        //Vector2 tilePos = FindTilesInsideMap(i, j);
        //        FogSpriteGrid[i, j].color = new Color(0, 0, 0, 1f);
        //    }
        //}

        //캐릭터 시야만큼만 안개를 어둡게 만들고
        for (int i = (int)minPos.x - 1; i < (int)maxPos.x + 2; i++)
        {
            for (int j = (int)minPos.y - 1; j < (int)maxPos.y + 2; j++)
            {
                Vector2 tilePos = FindTilesInsideMap(i, j);
                FogSpriteGrid[(int)tilePos.x, (int)tilePos.y].color = new Color(0, 0, 0, 0.9f);
            }
        }
    }

    //시작점과 목표점 사이의 배치된 타일 위치 찾아서 묶어둔 리스트로 반환
    private List<Vector2> SearchingForTileLocationsBelowLine(Vector2 startPos, Vector2 targetPos, int sort)
    {
        //시작점과 도착점에 0.5f를 더해 위치 보정
        Vector2 startPos2 = new Vector2(startPos.x + 0.5f, startPos.y + 0.5f);
        Vector2 targetPos2;
        if (sort == 1)
            targetPos2 = new Vector2(targetPos.x + 0.7f, targetPos.y + 0.5f);
        else if(sort == 2)
            targetPos2 = new Vector2(targetPos.x + 0.3f, targetPos.y + 0.5f);
        else if (sort == 3)
            targetPos2 = new Vector2(targetPos.x + 0.5f, targetPos.y + 0.7f);
        else
            targetPos2 = new Vector2(targetPos.x + 0.5f, targetPos.y + 0.3f);

        //디버그 라인에 목표점 추가
        debugTargetPosList.Add(new Vector2(targetPos2.x, targetPos2.y));

        //임시 리스트 초기화
        ClearTempPosList(2);

        float AbsX = math.abs(targetPos.x - startPos.x);
        float AbsY = math.abs(targetPos.y - startPos.y);
        //두 점이 기울기가 없는 직선으로 연결되어 있다면
        if (AbsX < 0.5f || AbsY < 0.5f)
        {
            //가로 직선이면서
            if (AbsY < 0.5f)
            {
                if (startPos2.x <= targetPos2.x)
                    FindTilesBelowStraightLine(startPos, targetPos, "e");
                else
                    FindTilesBelowStraightLine(startPos, targetPos, "w");
            }
            //세로 직선이면서
            else if (AbsX < 0.5f)
            {
                if (startPos2.y <= targetPos2.y)
                    FindTilesBelowStraightLine(startPos, targetPos, "n");
                else
                    FindTilesBelowStraightLine(startPos, targetPos, "s");
            }
        }
        //두 점이 기울기를 가진 직선으로 연결되어 있다면
        else
        {
            float slopeX, slopeY;

            //x축 기울기 계산
            slopeX = (targetPos2.y - startPos2.y) / (targetPos2.x - startPos2.x);
            //y축 기울기 계산
            slopeY = (targetPos2.x - startPos2.x) / (targetPos2.y - startPos2.y);

            //목표점이 어디 사분면에 위치해 있는지 확인. 각 분면마다 다르게 검토
            if (startPos2.x <= targetPos2.x)
            {
                if (startPos2.y <= targetPos2.y)
                {
                    //print("1사분면 검사");
                    FindTilesIncreasingToAxisX(startPos2, targetPos2, slopeX, slopeY);
                    FindTilesIncreasingToAxisY(startPos2, targetPos2, slopeX, slopeY);
                }
                else
                {
                    //print("4사분면 검사");
                    FindTilesIncreasingToAxisX(startPos2, targetPos2, slopeX, slopeY);
                    FindTilesDecreasingToAxisY(startPos2, targetPos2, slopeX, slopeY);
                }
            }
            else
            {
                if (startPos2.y <= targetPos2.y)
                {
                    //print("2사분면 검사");
                    FindTilesDecreasingToAxisX(startPos2, targetPos2, slopeX, slopeY);
                    FindTilesIncreasingToAxisY(startPos2, targetPos2, slopeX, slopeY);
                }
                else
                {
                    //print("3사분면 검사");
                    FindTilesDecreasingToAxisX(startPos2, targetPos2, slopeX, slopeY);
                    FindTilesDecreasingToAxisY(startPos2, targetPos2, slopeX, slopeY);
                }
            }
            //찾은 타일을 거리에 따라 재정렬
            RearrangeAccordingToDistance(startPos);
        }
        
        //만들어진 리스트를 반환함
        return tempListTileBelowLine;
    }

    //직선 검토
    private void FindTilesBelowStraightLine(Vector2 startPos, Vector2 targetPos, string sort)
    {
        if (sort == "e")
        {
            //(int)startPos.x+1에서 +1을 해야지 시작점을 포함하지 않음
            for (int i = (int)startPos.x+1; i <= (int)targetPos.x; i++)
            {
                tempListTileBelowLine.Add(new Vector2(i, startPos.y));
            }
        }
        else if(sort == "w")
        {
            for (int i = (int)startPos.x-1; i >= (int)targetPos.x; i--)
            {
                tempListTileBelowLine.Add(new Vector2(i, startPos.y));
            }
        }
        else if (sort == "n")
        {
            for (int i = (int)startPos.y+1; i <= (int)targetPos.y; i++)
            {
                tempListTileBelowLine.Add(new Vector2(startPos.x, i));
            }
        }
        else if (sort == "s")
        {
            for (int i = (int)startPos.y-1; i >= (int)targetPos.y; i--)
            {
                tempListTileBelowLine.Add(new Vector2(startPos.x, i));
            }
        }
    }
    //대각선 x축 증가 검토
    private void FindTilesIncreasingToAxisX(Vector2 startPos, Vector2 targetPos, float slopeX, float slopeY)
    {
        ClearTempPosList(1);
        //처음에는 x에 0.51만 더해 y위치를 찾음
        //왜0.5가 아니라 0.51이냐면, 정확하게 경계선에 걸치면 어디 면인지 판단하기 어렵기 때문
        float x = startPos.x + 0.51f;
        float y = startPos.y + slopeX * 0.51f;
        //첫 점을 임시 리스트에 넣음
        tempPosList.Add(new Vector2(x, y));

        //그 다음에 1씩 반복 증가시킴
        for (float i = x + 1; i < targetPos.x; i++)
        {
            y = y + slopeX;
            tempPosList.Add(new Vector2(i, y));
        }
        //리스트의 위치를 보정해 새로 담음
        for (int p = 0; p < tempPosList.Count; p++)
        {
            tempListTileBelowLine.Add(CorrectionPosSpace(tempPosList[p]));
        }
    }
    //대각선 x축 감소 검토
    private void FindTilesDecreasingToAxisX(Vector2 startPos, Vector2 targetPos, float slopeX, float slopeY)
    {
        ClearTempPosList(1);

        float x = startPos.x - 0.51f;
        float y = startPos.y - slopeX * 0.51f;
        
        tempPosList.Add(new Vector2(x, y));
        
        for (float i = x - 1; i > targetPos.x; i--)
        {
            y = y - slopeX;
            tempPosList.Add(new Vector2(i, y));
        }
        for (int p = 0; p < tempPosList.Count; p++)
        {
            tempListTileBelowLine.Add(CorrectionPosSpace(tempPosList[p]));
        }
    }
    //대각선 y축 증가 검토
    private void FindTilesIncreasingToAxisY(Vector2 startPos, Vector2 targetPos, float slopeX, float slopeY)
    {
        //완전 대각선이라면 y축으로 검사할 필요없음. 검사를 하게되면 같은 타일을 중복으로 리스트에 넣음
        if (slopeY == 1)
            return;

        ClearTempPosList(1);

        float x = startPos.x + slopeY * 0.51f;
        float y = startPos.y + 0.51f;

        tempPosList.Add(new Vector2(x, y));

        for (float i = y + 1; i < targetPos.y; i++)
        {
            x = x + slopeY;
            tempPosList.Add(new Vector2(x, i));
        }
        for (int p = 0; p < tempPosList.Count; p++)
        {
            tempListTileBelowLine.Add(CorrectionPosSpace(tempPosList[p]));
        }
    }
    //대각선 y축 감소 검토
    private void FindTilesDecreasingToAxisY(Vector2 startPos, Vector2 targetPos, float slopeX, float slopeY)
    {
        if (slopeY == 1)
            return;

        ClearTempPosList(1);

        float x = startPos.x - slopeY * 0.51f;
        float y = startPos.y - 0.51f;

        tempPosList.Add(new Vector2(x, y));

        for (float i = y - 1; i > targetPos.y; i--)
        {
            x = x - slopeY;
            tempPosList.Add(new Vector2(x, i));
        }
        for (int p = 0; p < tempPosList.Count; p++)
        {
            tempListTileBelowLine.Add(CorrectionPosSpace(tempPosList[p]));
        }
    }
    //직선으로 찾은 타일을 거리순으로 재배열하기
    private void RearrangeAccordingToDistance(Vector2 startPos)
    {
        //각 타일의 거리를 구해 리스트에 넣음
        for (int a = 0; a < tempListTileBelowLine.Count; a++)
        {
            float distance = Vector2.Distance(startPos, tempListTileBelowLine[a]);
            tempPosList2.Add(distance);
        }

        //
        int minIndex;
        int i, j;
        for (i = 0; i < tempPosList2.Count-1; i++)
        {
            minIndex = i;
            for (j = i+1; j < tempPosList2.Count; j++)
            {
                if (tempPosList2[j] < tempPosList2[minIndex])
                {
                    minIndex = j;
                }
            }
            //스왑 i와 minindex 값을 교체
            SwapTwoIndexValuesInTheList(i, minIndex);
        }
    }
    //목록의 두 인덱스의 값을 교체하기
    private void SwapTwoIndexValuesInTheList(int a, int b)
    {
        float x = tempPosList2[a];
        float y = tempPosList2[b];
        tempPosList2.RemoveAt(a);
        tempPosList2.Insert(a, y);
        tempPosList2.RemoveAt(b);
        tempPosList2.Insert(b, x);

        Vector2 p = tempListTileBelowLine[a];
        Vector2 q = tempListTileBelowLine[b];
        tempListTileBelowLine.RemoveAt(a);
        tempListTileBelowLine.Insert(a, q);
        tempListTileBelowLine.RemoveAt(b);
        tempListTileBelowLine.Insert(b, p);
    }

    //임시 배열을 초기화시킴
    private void ClearTempPosList(int sort)
    {
        if (sort <= 1)
            tempPosList.Clear();
        else if (sort >= 2)
        {
            tempPosList.Clear();
            tempListTileBelowLine.Clear();
            tempPosList2.Clear();
        }
    }
    //적당히 찾은 위치값을 내림으로 보정함
    private Vector2 CorrectionPosSpace(Vector2 pos)
    {
        return new Vector2(Mathf.Floor(pos.x), Mathf.Floor(pos.y));
    }
}