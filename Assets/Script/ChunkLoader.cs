using Sirenix.OdinInspector;
using System;
using UnityEditor;
using UnityEngine;

public class ChunkLoader : MonoBehaviour
{
    public GameObject[] ChunkObjectArrayInLoader = null;
    public ChunkData[] ChunkDataArrayInLoader = null;

    [Button]
    public void ChunkLoad()
    {
        for (int i = 0; i < ChunkObjectArrayInLoader.Length; i++)
        {
            ChunkDataArrayInLoader[i].TileDataList = ChunkObjectArrayInLoader[i].GetComponentsInChildren<TileData>();
            ChunkDataArrayInLoader[i].SaveData();
        }
        print("총 " + ChunkObjectArrayInLoader.Length + "개의 청크를 불러 저장했습니다.");
    }
}