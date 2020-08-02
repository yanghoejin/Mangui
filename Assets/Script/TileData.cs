using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

[Serializable]
public class TileData : MonoBehaviour
{
    public string TileID;
    public bool CantGoInside;
    public bool CantSeeBeyond;
    public SpriteRenderer TartgetSpriteRenderer = null;
    public Sprite[] AnotherSprite = null;
    public string TileIDAfterAction;

    private void Start()
    {
        if (AnotherSprite.Length > 0)
        {
            int num = UnityEngine.Random.Range(0, AnotherSprite.Length);

            if (TartgetSpriteRenderer != null)
                TartgetSpriteRenderer.sprite = AnotherSprite[num];
            else
                GetComponent<SpriteRenderer>().sprite = AnotherSprite[num];
        }
    }
}