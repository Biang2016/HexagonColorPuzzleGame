using UnityEngine;

public class HexagonBorder : PoolObject
{
    [SerializeField] private SpriteRenderer SpriteRenderer;

    public HexPos HexPos;

    public void Init(HexPos hexPos, Vector3 position, float radius)
    {
        HexPos = hexPos;
        transform.localPosition = position;
        transform.localScale = Vector3.one * radius;
    }
}