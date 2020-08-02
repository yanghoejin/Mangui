using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ChunkType
{
    Wasteland, Grassland, HouseSmall, HouseLarge
}

public class ChunkLawData : MonoBehaviour
{
    public ChunkType ChunkType;
}