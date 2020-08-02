using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GameRule : MonoBehaviour
{
    public static GameRule Instance = null;

    //한 청크의 가로세로 타일 개수
    public int TileNumberOnChunk;
    //한 맵에 배치될 청크 개수
    public int ChunkNumberOnMap;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }
}