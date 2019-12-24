using System;
using System.Collections.Generic;
using UnityEngine;

public class Map : PoolObject
{
    public Map GoalMap;
    [SerializeField] private GameObject MapBoard;
    [SerializeField] private MeshRenderer MapBoardMeshRenderer;
    [SerializeField] private Material StartBG;
    [SerializeField] private Material GoalBG;

    public override void PoolRecycle()
    {
        base.PoolRecycle();
        Clear();
    }

    [SerializeField] private int MapRounds = 4; //地图格子圈数

    private float HexRadius; //六边形外接圆半径
    internal const float HexRadius_Round_2 = 16f;
    public bool IsReadOnly = false;

    private float BorderRatio = 0.1f;
    internal float GridScale;
    private float HexagonImageRatio = 4.4f;

    internal MapGridTypes CurrentMapGridType;
    internal MapGridColorTypes CurrentMapGridColorType;

    //六边形坐标，OX和OY为单位向量
    public Vector2 OX;
    public Vector2 OY;
    public Vector2 OZ;

    internal const float SQRT_3 = 1.732f;

    void Awake()
    {
        MapBoardMeshRenderer.material = StartBG;
    }

    private void Clear()
    {
        ClearMapGrids();
        ClearHexagonBorders();
        MoveHistory.Clear();
    }

    private void Reset()
    {
        Clear();
        HexRadius = 3f / (MapRounds + 1) * HexRadius_Round_2;
        GridScale = HexRadius * (1 - BorderRatio) / SQRT_3;
        OX = new Vector2(SQRT_3 / 2 * HexRadius, -0.5f * HexRadius);
        OY = new Vector2(SQRT_3 / 2 * HexRadius, 0.5f * HexRadius);
        OZ = new Vector2(0, HexRadius);
        GenerateHexagonBorders();
    }

    void Start()
    {
    }

    private MapGrid MouseLeftDownMapGrid;
    private MapGrid MouseRightDownMapGrid;

    private MapGrid LastClickMapGrid;
    private MapGrid DragMapGrid;

    private HexPos CurHoverHexPos = HexPos.Empty;
    private HexPos LastHoverHexPos = HexPos.Empty;

    void Update()
    {
        if (IsReadOnly) return;
        if (UIManager.Instance.IsPeekUIForm<ConfirmPanel>()) return;
        HoverMapHexagon();

        if (HoverHexagon)
        {
            MapGridDict.TryGetValue(HoverHexagon.HexPos.ToString(), out MapGrid hoverMapGrid);

            if (Input.GetMouseButtonDown(0))
            {
                DragMapGrid = hoverMapGrid;
                if (hoverMapGrid != null && MapSettings.IsMoveValid(hoverMapGrid.MapGridInfo.MapGridColorType, hoverMapGrid.MapGridInfo.MapGridType))
                {
                    MouseLeftDownMapGrid = hoverMapGrid;
                }
                else
                {
                    MouseLeftDownMapGrid = null;
                }
            }
            else if (Input.GetMouseButtonDown(1) && GameManager.Instance.IsLevelEditorMode)
            {
                MouseRightDownMapGrid = hoverMapGrid;
            }
            else if (Input.GetMouseButtonUp(0))
            {
                if (MouseLeftDownMapGrid == hoverMapGrid)
                {
                    OnClickHex(HoverHexagon.HexPos);
                    CheckBeatLevel();
                }
                else
                {
                    if (DragMapGrid)
                    {
                        OnDragEnd();
                        DragMapGrid = null;
                        if (LastClickMapGrid)
                        {
                            LastClickMapGrid.IsSelected = false;
                            LastClickMapGrid = null;
                        }
                    }
                }

                MouseLeftDownMapGrid = null;
            }
            else if (Input.GetMouseButtonUp(1) && GameManager.Instance.IsLevelEditorMode)
            {
                if (MouseRightDownMapGrid == hoverMapGrid)
                {
                    OnRightClickHex(HoverHexagon.HexPos);
                    MouseRightDownMapGrid = null;
                }
            }

            if (Input.GetMouseButton(0))
            {
                if (!LastHoverHexPos.Equals(HoverHexagon.HexPos))
                {
                    OnDrag(HoverHexagon.HexPos);
                }
            }

            if (!CurHoverHexPos.Equals(HoverHexagon.HexPos))
            {
                LastHoverHexPos = CurHoverHexPos;
                CurHoverHexPos = HoverHexagon.HexPos;
            }
        }
        else
        {
            ClearPaintHexagons();
            LastHoverHexPos = HexPos.Empty;
            CurHoverHexPos = HexPos.Empty;

            if (Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1))
            {
                if (LastClickMapGrid)
                {
                    LastClickMapGrid.IsSelected = false;
                    LastClickMapGrid = null;
                }
            }
        }
    }

    private List<Hexagon> PaintHexagons = new List<Hexagon>();

    private void OnDrag(HexPos hexPos)
    {
        if (DragMapGrid)
        {
            if (!MapSettings.IsDragValid(DragMapGrid.MapGridInfo.MapGridColorType, DragMapGrid.MapGridInfo.MapGridType)) return;
            HexPos.Directions direction = DragMapGrid.MapGridInfo.HexPos.DirectionTo(hexPos);
            HexPos delta = new HexPos();
            ClearPaintHexagons();
            switch (direction)
            {
                case HexPos.Directions.None:
                {
                    return;
                }
                case HexPos.Directions.OX:
                {
                    delta = HexPos.Hex_OX;
                    break;
                }
                case HexPos.Directions.OX_Rev:
                {
                    delta = -1 * HexPos.Hex_OX;
                    break;
                }
                case HexPos.Directions.OY:
                {
                    delta = HexPos.Hex_OY;
                    break;
                }
                case HexPos.Directions.OY_Rev:
                {
                    delta = -1 * HexPos.Hex_OY;
                    break;
                }
                case HexPos.Directions.OZ:
                {
                    delta = HexPos.Hex_OZ;
                    break;
                }
                case HexPos.Directions.OZ_Rev:
                {
                    delta = -1 * HexPos.Hex_OZ;
                    break;
                }
            }

            HexPos nextGrid = DragMapGrid.MapGridInfo.HexPos + delta;
            while (CheckInsideMap(nextGrid))
            {
                Hexagon hexagon = DrawHexagon(nextGrid, GameManager.Instance.MapSettings.MapGridColors[(int) DragMapGrid.MapGridInfo.MapGridColorType], 0.3f);
                PaintHexagons.Add(hexagon);
                nextGrid += delta;
            }
        }
    }

    private void OnDragEnd()
    {
        if (PaintHexagons.Count > 0)
        {
            UIManager.Instance.GetBaseUIForm<InGamePanel>().StepUsed++;
            Record();
        }

        foreach (Hexagon ph in PaintHexagons)
        {
            MapGridDict.TryGetValue(ph.HexPos.ToString(), out MapGrid originMG);
            if (originMG)
            {
                if (originMG.MapGridInfo.MapGridType == MapGridTypes.NormalGrid)
                {
                    MapGridColorTypes mixedColor = MapSettings.ColorMix(originMG.MapGridInfo.MapGridColorType, DragMapGrid.MapGridInfo.MapGridColorType);
                    MapGridTypes mixedType = MapSettings.MixedMapGridTypeDict[mixedColor];
                    RemoveMapGrid(originMG, true);
                    MapGrid mapGrid = DrawMapGrid(new MapGridInfo(ph.HexPos, mixedType, mixedColor));
                    AddMapGrid(mapGrid);
                }
            }
            else
            {
                MapGridColorTypes mixedColor = DragMapGrid.MapGridInfo.MapGridColorType;
                MapGridTypes mixedType = MapSettings.MixedMapGridTypeDict[mixedColor];
                MapGrid mapGrid = DrawMapGrid(new MapGridInfo(ph.HexPos, mixedType, mixedColor));
                AddMapGrid(mapGrid);
            }
        }

        if (PaintHexagons.Count > 0)
        {
            CheckBeatLevel();
        }

        ClearPaintHexagons();
    }

    private void ClearPaintHexagons()
    {
        foreach (Hexagon h in PaintHexagons)
        {
            h.PoolRecycle();
        }

        PaintHexagons.Clear();
    }

    private void OnClickHex(HexPos hexPos)
    {
        if (MapGridDict.ContainsKey(hexPos.ToString()))
        {
            MapGrid newClickedMapGrid = MapGridDict[hexPos.ToString()];
            if (LastClickMapGrid)
            {
                if (LastClickMapGrid == newClickedMapGrid)
                {
                    LastClickMapGrid.IsSelected = false;
                    LastClickMapGrid = null;
                    return;
                }
                else
                {
                    LastClickMapGrid.IsSelected = false;
                    LastClickMapGrid = newClickedMapGrid;
                    LastClickMapGrid.IsSelected = true;
                }
            }
            else
            {
                LastClickMapGrid = newClickedMapGrid;
                LastClickMapGrid.IsSelected = true;
            }
        }
        else
        {
            if (LastClickMapGrid)
            {
                if (LastClickMapGrid.MapGridInfo.HexPos.IsAdjacentTo(hexPos) || GameManager.Instance.IsLevelEditorMode)
                {
                    Record();
                    RemoveMapGrid(LastClickMapGrid, false);
                    LastClickMapGrid.MapGridInfo.HexPos = hexPos;
                    AddMapGrid(LastClickMapGrid);
                    LastClickMapGrid.IsSelected = false;
                    LastClickMapGrid = null;
                    UIManager.Instance.GetBaseUIForm<InGamePanel>().StepUsed++;
                }
            }
        }
    }

    private void OnRightClickHex(HexPos hexPos)
    {
        List<MapGrid> removeMapGrids = new List<MapGrid>();
        foreach (KeyValuePair<string, MapGrid> kv in MapGridDict)
        {
            if (kv.Value.MapGridInfo.HexPos.Equals(hexPos))
            {
                removeMapGrids.Add(kv.Value);
            }
        }

        if (removeMapGrids.Count == 0)
        {
            MapGrid mapGrid = DrawMapGrid(new MapGridInfo(hexPos, CurrentMapGridType, CurrentMapGridColorType));
            AddMapGrid(mapGrid);
        }
        else
        {
            foreach (MapGrid rmg in removeMapGrids)
            {
                RemoveMapGrid(rmg, true);
            }
        }
    }

    #region  MapGrid

    public LevelInfo CurrentLevelInfo;

    public Transform MapGridContainer;
    public Dictionary<string, MapGrid> MapGridDict = new Dictionary<string, MapGrid>();

    private MapGrid DrawMapGrid(MapGridInfo mgi)
    {
        MapGrid mg = GameObjectPoolManager.Instance.PoolDict[(GameObjectPoolManager.PrefabNames) Enum.Parse(typeof(GameObjectPoolManager.PrefabNames), mgi.MapGridType.ToString())].AllocateGameObject<MapGrid>(MapGridContainer);
        mg.Init(mgi, GridScale);
        mg.transform.localPosition = GetGridPosByHexPos(MapGridContainer, mg.MapGridInfo.HexPos);
        return mg;
    }

    private void AddMapGrid(MapGrid mg)
    {
        MapGridDict.Add(mg.MapGridInfo.HexPos.ToString(), mg);
        CurrentMapInfo.MapGridInfos.Add(mg.MapGridInfo);
        mg.transform.localPosition = GetGridPosByHexPos(MapGridContainer, mg.MapGridInfo.HexPos);
    }

    private void RemoveMapGrid(MapGrid mg, bool recycle)
    {
        MapGridDict.Remove(mg.MapGridInfo.HexPos.ToString());
        CurrentMapInfo.MapGridInfos.Remove(mg.MapGridInfo);
        if (recycle) mg.PoolRecycle();
    }

    private MapTypes currentMapType = MapTypes.Start;

    private MapTypes CurrentMapType
    {
        get { return currentMapType; }
        set
        {
            currentMapType = value;
            MapBoardMeshRenderer.material = currentMapType == MapTypes.Start ? StartBG : GoalBG;
        }
    }

    public void ReloadLevel()
    {
        LoadLevel(CachedLevelInfo, CurrentMapType);
    }

    public LevelInfo CachedLevelInfo;
    public MapInfo CurrentMapInfo;

    public void LoadLevel(LevelInfo levelInfo, MapTypes mapType)
    {
        CachedLevelInfo = levelInfo.Clone();
        if (mapType == MapTypes.Start)
        {
            UIManager.Instance.ShowUIForms<InGamePanel>().Init(levelInfo);
        }

        CurrentMapType = mapType;
        CurrentLevelInfo = levelInfo;
        MapRounds = levelInfo.MapRounds;
        Reset();
        LoadMapInfo(CurrentMapType == MapTypes.Start ? CurrentLevelInfo.StartMapInfo : CurrentLevelInfo.GoalMapInfo);
    }

    private void LoadMapInfo(MapInfo mapInfo)
    {
        CurrentMapInfo = mapInfo.Clone();
        ClearMapGrids();
        foreach (MapGridInfo mgi in CurrentMapInfo.MapGridInfos)
        {
            MapGrid mg = DrawMapGrid(mgi);
            MapGridDict.Add(mg.MapGridInfo.HexPos.ToString(), mg);
        }
    }

    public void SwapShowStartAndGoal()
    {
        CurrentMapType = CurrentMapType == MapTypes.Start ? MapTypes.Goal : MapTypes.Start;
        LoadMapInfo(CurrentMapType == MapTypes.Start ? CurrentLevelInfo.StartMapInfo : CurrentLevelInfo.GoalMapInfo);
    }

    private void ClearMapGrids()
    {
        foreach (KeyValuePair<string, MapGrid> kv in MapGridDict)
        {
            kv.Value.PoolRecycle();
        }

        MapGridDict.Clear();
    }

    private Stack<MapInfo> MoveHistory = new Stack<MapInfo>();

    private void Record()
    {
        MapInfo lastMove = CurrentMapInfo.Clone();
        MoveHistory.Push(lastMove);
    }

    public void Undo()
    {
        if (MoveHistory.Count != 0)
        {
            LoadMapInfo(MoveHistory.Pop());
            UIManager.Instance.GetBaseUIForm<InGamePanel>().StepUsed--;
        }
    }

    private void CheckBeatLevel()
    {
        if (GameManager.Instance.IsLevelEditorMode) return;
        bool levelBeat = true;
        foreach (MapGridInfo mgi in CurrentLevelInfo.GoalMapInfo.MapGridInfos)
        {
            MapGridDict.TryGetValue(mgi.HexPos.ToString(), out MapGrid mg);
            if (!mg)
            {
                levelBeat = false;
                break;
            }

            if (mg.MapGridInfo.MapGridColorType != mgi.MapGridColorType || mg.MapGridInfo.MapGridType != mgi.MapGridType)
            {
                levelBeat = false;
                break;
            }
        }

        if (levelBeat)
        {
            ConfirmPanel cp = UIManager.Instance.ShowUIForms<ConfirmPanel>();
            cp.Initialize("You beat this level!", "Continue", "Play again", delegate
            {
                cp.CloseUIForm();
                GameManager.Instance.LoadLevel(CurrentLevelInfo.LevelID + 1);
            }, delegate
            {
                cp.CloseUIForm();
                GameManager.Instance.LoadLevel(CurrentLevelInfo.LevelID);
            });
        }
    }

    #endregion

    public Vector3 GetGridPosByHexPos(Transform parent, HexPos hexPos)
    {
        Vector2 hexCenterPosition = hexPos.X * OX + hexPos.Y * OY;
        Vector3 orthPos = new Vector3(hexCenterPosition.x, parent.position.y, hexCenterPosition.y);
        return orthPos;
    }

    public Vector3[] DrawHexOutline(Vector3 point, out HexPos hexPos)
    {
        Vector2 planeExactPosition;
        Vector3[] hexVertex = new Vector3[7];
        planeExactPosition.x = point.x;
        planeExactPosition.y = point.z;
        hexPos = ConvertOrthToHexPosition(OX, OY, planeExactPosition);
        return hexVertex;
    }

    public HexPos ConvertOrthToHexPosition(Vector2 OX, Vector2 OY, Vector2 orth_Position)
    {
        HexPos res = new HexPos();
        float nADDm;
        float nMINUSm;
        nADDm = orth_Position.x / OX.x;
        nMINUSm = orth_Position.y / OX.y;
        float n;
        float m;
        n = (nADDm + nMINUSm) / 2;
        m = (nADDm - nMINUSm) / 2;

        int n_floor = Mathf.FloorToInt(n);
        int m_floor = Mathf.FloorToInt(m);
        float Local_X = n - n_floor;
        float Local_Y = m - m_floor;

        if (Local_X <= -2 * Local_Y + 1 && Local_Y <= -2 * Local_X + 1)
        {
            res.X = n_floor;
            res.Y = m_floor;
        }
        else if (Local_Y >= -2 * Local_X + 2 && Local_X >= -2 * Local_Y + 2)
        {
            res.X = n_floor + 1;
            res.Y = m_floor + 1;
        }
        else if (Local_X >= Local_Y)
        {
            res.X = n_floor + 1;
            res.Y = m_floor;
        }
        else if (Local_X <= Local_Y)
        {
            res.X = n_floor;
            res.Y = m_floor + 1;
        }

        return res;
    }

    public bool CheckInsideMap(HexPos hexPos)
    {
        return Mathf.Abs(hexPos.X) <= MapRounds && Mathf.Abs(hexPos.Y) <= MapRounds && Mathf.Abs(hexPos.X + hexPos.Y) <= MapRounds;
    }

    #region Hexagon

    public Transform HexagonContainer;
    private Hexagon HoverHexagon;

    private void HoverMapHexagon()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 100, GameManager.Instance.Layer_MapBoard))
        {
            if (hit.collider.gameObject == MapBoard)
            {
                DrawHexOutline(hit.point, out HexPos hexPos);
                if (CheckInsideMap(hexPos))
                {
                    HoverHexagon?.PoolRecycle();
                    if (Input.GetMouseButton(0))
                    {
                        HoverHexagon = DrawHexagon(hexPos, Color.clear, 0f);
                    }
                    else
                    {
                        HoverHexagon = DrawHexagon(hexPos, Color.white, 0.5f);
                    }
                }
                else
                {
                    HoverHexagon?.PoolRecycle();
                    HoverHexagon = null;
                }
            }
        }
    }

    private Hexagon DrawHexagon(HexPos hexPos, Color color, float alpha)
    {
        Vector3 pos = GetGridPosByHexPos(HexagonContainer, hexPos);
        Hexagon newHexagon = GameObjectPoolManager.Instance.PoolDict[GameObjectPoolManager.PrefabNames.Hexagon].AllocateGameObject<Hexagon>(HexagonContainer);
        newHexagon.Init(hexPos, pos, HexRadius / HexagonImageRatio, new Color(color.r, color.g, color.b, alpha));
        return newHexagon;
    }

    #endregion

    #region HexagonBorder

    public Transform HexagonBorderContainer;
    internal List<HexagonBorder> HexagonBorders = new List<HexagonBorder>();

    public void GenerateHexagonBorders()
    {
        int nodeCount = (MapRounds + 1) * MapRounds * 3 + 1 + 12;
        Vector2[] nodePositions = new Vector2[nodeCount];
        HexPos[] nodeHexPositions = new HexPos[nodeCount];
        Vector2[] directions = new[] {OX, OY, OZ, -OX, -OY, -OZ, OX};
        int index = 0;
        nodePositions[index++] = Vector2.zero;
        nodeHexPositions[index++] = new HexPos(0, 0);

        for (int round = 1; round <= MapRounds; round++)
        {
            for (int i = 0; i < 6; i++)
            {
                nodePositions[index++] = round * directions[i];
                for (int middle = 1; middle <= round - 1; middle++)
                {
                    nodePositions[index++] = ((middle) * directions[i + 1] + (round - middle) * directions[i]);
                }
            }
        }

        for (int i = 0; i < nodeCount; i++)
        {
            Vector2 nodePosition = nodePositions[i];
            HexPos nodeHexPosition = nodeHexPositions[i];
            HexagonBorder hex = GameObjectPoolManager.Instance.PoolDict[GameObjectPoolManager.PrefabNames.HexagonBorder].AllocateGameObject<HexagonBorder>(HexagonBorderContainer);
            HexagonBorders.Add(hex);
            hex.Init(nodeHexPosition, new Vector3(nodePosition.x, HexagonBorderContainer.transform.position.y, nodePosition.y), HexRadius / HexagonImageRatio);
        }
    }

    public void ClearHexagonBorders()
    {
        foreach (HexagonBorder hexagon in HexagonBorders)
        {
            hexagon.PoolRecycle();
        }

        HexagonBorders.Clear();
    }

    #endregion
}

public enum MapTypes
{
    Start,
    Goal,
}