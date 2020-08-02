using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName ="ChunkData", menuName = "ChunkData", order = int.MaxValue)]
public class ChunkData : ScriptableObject
{
    //청크 내 타일 리스트
    public TileData[] TileDataList;

    public void SaveData()
    {
        #if UNITY_EDITOR
        EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssets();
        #endif
    }
}