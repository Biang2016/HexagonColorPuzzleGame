using UnityEngine;
using UnityEditor;
using System.Xml;
using System;
using System.IO;

public class MapGeneratorEditorWindow : EditorWindow
{
    private MapGenerator MG;
    private MapGridTypes SelectMapGridType;
    private MapGridColorTypes SelectMapGridColorType;

    [MenuItem("Window/MapCreator")]
    static void CreateWindow()
    {
        Rect wr = new Rect(0, 0, 250, 150);
        MapGeneratorEditorWindow window = (MapGeneratorEditorWindow) EditorWindow.GetWindowWithRect(typeof(MapGeneratorEditorWindow), wr, false, "MapCreator");
        window.Show();
    }

    void Awake()
    {
        PrefabManager.Instance.LoadPrefabs_Editor();
        MG = FindObjectOfType<MapGenerator>();
    }

    void OnGUI()
    {
        MG = (MapGenerator) EditorGUILayout.ObjectField(MG, typeof(MapGenerator), true);
        if (MG != null)
        {
            MapGridTypes newMapGridType = (MapGridTypes) EditorGUILayout.EnumPopup(SelectMapGridType, GUILayout.Width(250));
            if (newMapGridType != SelectMapGridType)
            {
                SelectMapGridType = newMapGridType;
                MG.CurrentMapGridType = SelectMapGridType;
            }

            MapGridColorTypes newMapGridColorType = (MapGridColorTypes) EditorGUILayout.EnumPopup(SelectMapGridColorType, GUILayout.Width(250));
            if (newMapGridColorType != SelectMapGridColorType)
            {
                SelectMapGridColorType = newMapGridColorType;
                MG.CurrentMapGridColorType = SelectMapGridColorType;
            }

            if (GUILayout.Button("Clear current map", GUILayout.Width(150)))
            {
                MG.ClearCurrentBoard();
                EditorApplication.RepaintHierarchyWindow();
            }

            if (GUILayout.Button("Reset pool dict", GUILayout.Width(150)))
            {
                GameObjectPoolManager.Instance.ResetPoolDict();
            }

            if (GUILayout.Button("Export this map", GUILayout.Width(150)))
            {
                ExportMapToXML();
            }

            if (GUILayout.Button("Import a map", GUILayout.Width(150)))
            {
                ImportMapFromXML();
            }
        }
        else
        {
            if (GUILayout.Button("Search map generator", GUILayout.Width(150)))
            {
                MG = FindObjectOfType<MapGenerator>();
            }
        }
    }

    private int defaultMapNum = 0;

    private void ExportMapToXML()
    {
        string filePath = EditorUtility.SaveFilePanel("Export map to...", "/Assets", "NewMap_" + defaultMapNum, "xml");
        if (filePath != "")
        {
            XmlDocument xmlDoc = new XmlDocument();
            XmlElement MapGrids = xmlDoc.CreateElement("MapGrids");
            xmlDoc.AppendChild(MapGrids);

            GameObject chessBoard = GameObject.Find("ChessBoard");
            if (chessBoard.transform.childCount > 0)
            {
                GameObject[] allMapGrids = new GameObject[chessBoard.transform.childCount];
                MapGrid[] MGs = new MapGrid[chessBoard.transform.childCount];

                for (int i = 0; i < chessBoard.transform.childCount; i++)
                {
                    allMapGrids[i] = chessBoard.transform.GetChild(i).gameObject;
                    MGs[i] = allMapGrids[i].GetComponent<MapGrid>();
                    XmlElement MapGrid = xmlDoc.CreateElement("MapGrid");
                    MapGrid.SetAttribute("id", i.ToString());
                    MapGrid.SetAttribute("PositionX", string.Format("{0:f4}", allMapGrids[i].transform.position.x / MG.GridScale));
                    MapGrid.SetAttribute("PositionY", string.Format("{0:f4}", allMapGrids[i].transform.position.y / MG.GridScale));
                    MapGrid.SetAttribute("PositionZ", string.Format("{0:f4}", allMapGrids[i].transform.position.z / MG.GridScale));
                    MapGrid.SetAttribute("MapGridType", Enum.GetName(typeof(MapGridTypes), MGs[i].MapGridType));
                    MapGrid.SetAttribute("MapGridColorType", Enum.GetName(typeof(MapGridColorTypes), MGs[i].MapGridColorType));

                    MapGrids.AppendChild(MapGrid);
                }
            }

            StreamWriter sw = new StreamWriter(filePath);
            xmlDoc.Save(sw);
            sw.Close();
            defaultMapNum++;
            EditorUtility.DisplayDialog("Export map", "Success!", "OK");
        }
    }

    private void ImportMapFromXML()
    {
        string path = EditorUtility.OpenFilePanel("Select a XML file", "", "xml");
        if (path == "") return;
        if (MG != null)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(path);
            XmlElement root = xmlDoc.DocumentElement;
            XmlNodeList MapGrids = root.SelectNodes("/MapGrids/MapGrid");

            foreach (XmlElement mg in MapGrids)
            {
                //读MapGrid属性值
                MapGridTypes mapGridType = (MapGridTypes) Enum.Parse(typeof(MapGridTypes), mg.GetAttribute("MapGridType"));
                MapGridColorTypes mapGridColorType = (MapGridColorTypes) Enum.Parse(typeof(MapGridColorTypes), mg.GetAttribute("MapGridColorType"));
                float positionX = float.Parse(mg.GetAttribute("PositionX"));
                float positionY = float.Parse(mg.GetAttribute("PositionY"));
                float positionZ = float.Parse(mg.GetAttribute("PositionZ"));
                Vector3 position = new Vector3(positionX, positionY, positionZ);

                //实例化
                MapGrid newMapGrid = GameObjectPoolManager.Instance.PoolDict[(GameObjectPoolManager.PrefabNames) Enum.Parse(typeof(GameObjectPoolManager.PrefabNames), mapGridType.ToString())].AllocateGameObject<MapGrid>(MG.transform);
                newMapGrid.Init(mapGridColorType, mapGridType, MG.GridScale, position * MG.GridScale, newMapGrid.name + mg.GetAttribute("id"), int.Parse(mg.GetAttribute("id")));
                MG.MapGrids.Add(newMapGrid);
            }
        }
    }
}