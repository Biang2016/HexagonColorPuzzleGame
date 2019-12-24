using UnityEngine;

public class MapGrid : PoolObject
{
    [SerializeField] private MeshRenderer MeshRenderer;
    [SerializeField] private SpriteRenderer SelectedBorder;

    public MapGridInfo MapGridInfo;

    void Awake()
    {
        if (SelectedBorder) SelectedBorder.gameObject.SetActive(false);
    }

    public void Init(MapGridInfo mapGridInfo, float radius)
    {
        MapGridInfo = mapGridInfo;
        Color c = GameManager.Instance.MapSettings.MapGridColors[(int) mapGridInfo.MapGridColorType];
        Material tempMaterial = new Material(MeshRenderer.sharedMaterial);
        tempMaterial.color = c;
        MeshRenderer.sharedMaterial = tempMaterial;
        transform.localScale = Vector3.one * radius;
    }

    private bool isSelected = false;

    public bool IsSelected
    {
        get { return isSelected; }
        set
        {
            isSelected = value;
            if (SelectedBorder)
            {
                SelectedBorder.gameObject.SetActive(value);
            }
        }
    }
}