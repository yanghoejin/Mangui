using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions.Must;

public class Database : MonoBehaviour
{
    public static Database Instance = null;

    public GameObject[] DefaultGroundObject = null;

    //각 타일의 정보가 담겨있는 목록. 게임 실행할 떄, TileObjectList 목록에서 자동으로 추출함
    [HideInInspector]
    public List<TileData> TileDataList = null;
    //실제로 배치할 게임오브젝트 목록
    public List<GameObject> TileObjectList = null;
    //배치할 때 참고할 청크 정보
    public List<ChunkData> ChunkDataList = null;
    //청크 오브젝트 목록. 이곳에서 청크데이터를 뽑음. 인게임에서 사용하지는 않음
    public List<GameObject> ChunkObjectList = null;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);

        //시작하면서 여러곳에 사용될 데이터 리스트 TileDataList를 만든다
        LoadTileDataFromObject();
    }

    //이거 나중에 버튼으로 만들어도 좋을 듯? 그럴려면 스크립트블오브젝트로 만들어야 하는데 약간 복잡하고 귀찮다
    private void LoadTileDataFromObject()
    {
        for (int i = 0; i < TileObjectList.Count; i++)
        {
            TileDataList.Add(TileObjectList[i].GetComponent<TileData>());
        }
    }

    [Button]
    private void CheckTileNameToTileID()
    {
        for (int i = 0; i < TileObjectList.Count; i++)
        {
            if (TileObjectList[i].GetComponent<TileData>().TileID != TileObjectList[i].name)
            {
                Debug.LogError(TileObjectList[i].name + "의 오브젝트 TileID가 '" + TileObjectList[i].GetComponent<TileData>().TileID + "'으(로) 잘못 연결되었습니다.");
            }
        }
        print("TileID 검사를 모두 마쳤습니다.");
    }

    [Button]
    public void LoadChunkDataFromObject()
    {
        for (int i = 0; i < ChunkDataList.Count; i++)
        {
            ChunkDataList[i].TileDataList = ChunkObjectList[i].GetComponentsInChildren<TileData>();
            ChunkDataList[i].SaveData();
            if (!string.Equals(ChunkDataList[i].name, ChunkObjectList[i].name))
            {
                Debug.LogError(ChunkObjectList[i] + "를 " + ChunkDataList[i] + "에 넣으려고 합니다.");
            }
            if (ChunkDataList[i].TileDataList.Length > 100)
            {
                Debug.LogError(ChunkDataList[i] + "에 타일 수가 너무 많습니다. " + ChunkDataList[i].TileDataList.Length + "개가 포함되어 있습니다.");
                FindWronhTile(ChunkDataList[i]);
            }
            else if (ChunkDataList[i].TileDataList.Length < 100)
            {
                Debug.LogError(ChunkDataList[i] + "에 타일 수가 너무 적습니다. " + ChunkDataList[i].TileDataList.Length + "개가 포함되어 있습니다.");
            }
        }
        print("총 " + ChunkDataList.Count + "개의 청크를 불러 저장했습니다.");
    }
    public void FindWronhTile(ChunkData wrongChunk)
    {
        bool[,] checkChunk = new bool[10, 10];

        for (int i = 0; i < wrongChunk.TileDataList.Length; i++)
        {
            int x = (int)wrongChunk.TileDataList[i].transform.position.x;
            int y = (int)wrongChunk.TileDataList[i].transform.position.y;
            if (checkChunk[x, y] == false)
            {
                checkChunk[x, y] = true;
            }
            else
            {
                Debug.LogError("중복된 타일의 위치는 (" + x + "," + y + ") 입니다.");
            }
        }
    }

    public GameObject FindTiles(string tileID)
    {
        for (int i=0; i< TileObjectList.Count; i++)
        {
            if (tileID == TileObjectList[i].GetComponent<TileData>().TileID)
            {
                return TileObjectList[i];
            }
        }
        Debug.LogError(tileID + "타일을 찾을 수 없습니다.");
        return TileObjectList[0];
    }
}