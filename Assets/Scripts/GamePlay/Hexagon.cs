using UnityEngine;

public class Hexagon : PoolObject
{
    [SerializeField] private SpriteRenderer SpriteRenderer;
    public HexPos HexPos;

    public void Init(HexPos hexPos, Vector3 position, float radius,Color color)
    {
        HexPos = hexPos;
        transform.localPosition = position;
        transform.localScale = Vector3.one * radius;
        SpriteRenderer.color = color;
    }
}