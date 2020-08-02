using ES3Types;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.Rendering;
using UnityEngine.Tilemaps;

public class TileController : MonoBehaviour
{
    public static TileController Instance = null;

    //인게임에서 사용하는 맵2차원배열
    public TileData[,] TileGrid = null;
    private string[,] TileGridForSaveing = null;

    //청크 로드용
    private ChunkData tmpChunkData = null;
    //타일 배치전에 청크로 먼저 크게 배치함
    private ChunkData[,] tmpChunkGrid = null;

    private int mapSize;

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
        //맵을 초기 생성했을 때, 맵을 로드했을 때, 맵을 다시 배치할 때 초기화를 어떻게 구분할까?
        //이거는 무조건 필요함.
        mapSize = GameRule.Instance.ChunkNumberOnMap * GameRule.Instance.TileNumberOnChunk;

        //세이브가 되어 있지 않으면 맵을 만들어라
        bool isSave = ES3.Load("MapSave", false);
        if (!isSave)
        {
            MapInitialGeneration();
        }
        else
        {
            print("맵 데이터를 로드했습니다.");
            MapLoad();
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            print("세이브했습니다.");
            MapSave();
        }

        else if (Input.GetKeyDown(KeyCode.R))
        {
            print("세이브 데이터를 초기화시켰습니다.");
            SaveDataInitialization();
        }
    }

    private void MapInitialGeneration()
    {
        //먼저 청크 크기에 맞춰 청크배열을 초기화함
        mapSize = GameRule.Instance.ChunkNumberOnMap * GameRule.Instance.TileNumberOnChunk;
        int chunkNumberOnMap = GameRule.Instance.ChunkNumberOnMap;
        int tileNumberOnChunk = GameRule.Instance.TileNumberOnChunk;
        //타일 크기에 맞춰 초기화
        GridInitialization();

        tmpChunkGrid = new ChunkData[chunkNumberOnMap, chunkNumberOnMap];

        //먼저 청크를 임시 배열에 넣어둠.이걸 가지고 뒤에 타일을 심을 예정
        for (int i = 0; i < chunkNumberOnMap; i++)
        {
            for (int j = 0; j < chunkNumberOnMap; j++)
            {
                //임시. 무작위로 청크를 선택해서 청크 배열에 넣음
                //나중에 이 단계에서 청크를 정교하게 선택하는 로직이 들어가야 함
                int randomNum = Random.Range(0, Database.Instance.ChunkDataList.Count);
                tmpChunkData = Database.Instance.ChunkDataList[randomNum];
                tmpChunkGrid[i, j] = tmpChunkData;
            }
        }

        for (int a = 0; a < chunkNumberOnMap; a++)
        {
            for (int b = 0; b < chunkNumberOnMap; b++)
            {
                //한 청크가 보유하고 있는 타일 개수만큼 반복한다. foreach문을 써도 좋음
                int length = tmpChunkGrid[a, b].TileDataList.Length;
                for (int c = 0; c < length; c++)
                {
                    //a와b는 청크의 위치. 여기에 한 청크의 너비만큼 곱한 후, 세부 위치를 더해서 최종 위치를 구함
                    int x = (int)tmpChunkGrid[a, b].TileDataList[c].transform.position.x + (a * tileNumberOnChunk);
                    int y = (int)tmpChunkGrid[a, b].TileDataList[c].transform.position.y + (b * tileNumberOnChunk);
                    Vector3 pos = new Vector3(x, y, 0);

                    GameObject obj = Instantiate(Database.Instance.FindTiles(tmpChunkGrid[a, b].TileDataList[c].TileID));
                    obj.transform.position = pos;
                    obj.transform.parent = transform;
                    TileGrid[x, y] = obj.GetComponent<TileData>();
                }
            }
        }
        //바닥에 기본 바닥 스프라이트를 크게 깖
        //이거는 일단 임시 구현
        GameObject ground = Instantiate(Database.Instance.DefaultGroundObject[0]);
    }

    private void MapSave()
    {
        int mapSize = GameRule.Instance.ChunkNumberOnMap * GameRule.Instance.TileNumberOnChunk;
        for (int a = 0; a < mapSize; a++)
        {
            for (int b = 0; b < mapSize; b++)
            {
                TileGridForSaveing[a, b] = TileGrid[a, b].TileID;
            }
        }
        ES3.Save<bool>("MapSave", true);
        ES3.Save("MapGrid", TileGridForSaveing);
    }
    private void MapLoad()
    {
        //배열 초기화시킴
        GridInitialization();

        TileGridForSaveing = (string[,])ES3.Load("MapGrid");

        int mapSize = GameRule.Instance.ChunkNumberOnMap * GameRule.Instance.TileNumberOnChunk;
        //로드한 데이터를 기반으로 실제로 배치해보자
        for (int x = 0; x < mapSize; x++)
        {
            for (int y = 0; y < mapSize; y++)
            {
                GameObject obj = Instantiate(Database.Instance.FindTiles(TileGridForSaveing[x, y]));
                obj.transform.position = new Vector3(x,y,0);
                obj.transform.parent = transform;
                TileGrid[x, y] = obj.GetComponent<TileData>();
            }
        }
        //바닥에 기본 바닥 스프라이트를 크게 깖
        //이거는 일단 임시 구현
        GameObject ground = Instantiate(Database.Instance.DefaultGroundObject[0]);
    }
    //세이브 데이터 초기화
    private void SaveDataInitialization()
    {
        ES3.Save<bool>("MapSave", false);
    }

    //배열 초기화
    private void GridInitialization()
    {
        int mapSize = GameRule.Instance.ChunkNumberOnMap * GameRule.Instance.TileNumberOnChunk;
        TileGrid = new TileData[mapSize, mapSize];
        TileGridForSaveing = new string[mapSize, mapSize];
    }

    //맵 밖으로 이동하려고 하는지 확인
    public bool CheckGoOutOfMap(Vector2 targetPos)
    {
        if (targetPos.x < 0 || targetPos.x > mapSize - 1 || targetPos.y < 0 || targetPos.y > mapSize - 1)
        {
            print("맵 밖("+ targetPos + ")으로 이동하려고 합니다.");
            return false;
        }
        return true;
    }
    //이동할 수 있는 타일인지 확인
    public bool CheckCanGoInside(Vector2 targetPos)
    {
        return TileGrid[(int)targetPos.x, (int)targetPos.y].CantGoInside;
    }
}