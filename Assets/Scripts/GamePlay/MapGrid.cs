using System;
using UnityEngine;

public class MapGrid : PoolObject
{
    public MapGridColorTypes MapGridColorType = MapGridColorTypes.Red;
    public MapGridTypes MapGridType = MapGridTypes.NormalGrid;
    public int ID = -1;
    [SerializeField] private MeshRenderer MeshRenderer;

    public void Init(MapGridColorTypes colorType, MapGridTypes mapGridType, float radius, Vector3 position, string name, int id)
    {
        Color c = GameManager.Instance.MapSettings.MapGridColors[(int) colorType];
        Material tempMaterial = new Material(MeshRenderer.sharedMaterial);
        tempMaterial.color = c;
        MeshRenderer.sharedMaterial = tempMaterial;
        MapGridColorType = colorType;
        MapGridType = mapGridType;
        transform.localScale = Vector3.one * radius;
        transform.position = position;
        this.name = name;
        ID = id;
    }
}