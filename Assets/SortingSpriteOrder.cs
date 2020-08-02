using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class SortingSpriteOrder : MonoBehaviour
{
    [SerializeField]
    private bool _isMoving = false;
    private SpriteRenderer _spriteRenderer = null;
    [SerializeField]
    private Transform _basePosition = null;

    private void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();

        if (_basePosition == null)
            _basePosition = transform;

        SortingOrder();
    }

    private void Update()
    {
        //안움직는 것도 업데이트를 돌리는데, 괜찮은건가?
        if (_isMoving)
        {
            SortingOrder();
        }
    }

    private void SortingOrder()
    {
        _spriteRenderer.sortingOrder = CalculateOrder();
    }
    private int CalculateOrder()
    {
        float pos = _basePosition.position.y * 100;
        return -(int)pos;
    }
}
