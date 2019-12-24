using System.Collections.Generic;
using System.IO;
using System.Xml;
using UnityEngine;

public class AllLevels
{
    private static string LevelsXMLFile => Application.streamingAssetsPath + "/Configs/Levels.xml";

    public static SortedDictionary<int, LevelInfo> LevelDict = new SortedDictionary<int, LevelInfo>();

    public static void Reset()
    {
        LevelDict.Clear();
    }

    private static void addLevel(LevelInfo levelInfo)
    {
        if (!LevelDict.ContainsKey(levelInfo.LevelID))
        {
            LevelDict.Add(levelInfo.LevelID, levelInfo);
        }
        else
        {
            LevelDict[levelInfo.LevelID] = levelInfo;
        }
    }

    public static void AddAllLevels()
    {
        Reset();

        string text;
        using (StreamReader sr = new StreamReader(LevelsXMLFile))
        {
            text = sr.ReadToEnd();
        }

        XmlDocument doc = new XmlDocument();
        doc.LoadXml(text);
        XmlElement node_AllLevels = doc.DocumentElement;
        for (int i = 0; i < node_AllLevels.ChildNodes.Count; i++)
        {
            XmlNode node_LevelInfo = node_AllLevels.ChildNodes.Item(i);
            LevelInfo levelInfo = LevelInfo.GenerateFromXML(node_LevelInfo);
            addLevel(levelInfo);
        }
    }

    public static void RefreshLevelXML(LevelInfo levelInfo)
    {
        levelInfo = levelInfo.Clone();
        if (LevelDict.ContainsKey(levelInfo.LevelID))
        {
            LevelDict[levelInfo.LevelID] = levelInfo;
        }
        else
        {
            LevelDict.Add(levelInfo.LevelID, levelInfo);
        }

        string text;
        using (StreamReader sr = new StreamReader(LevelsXMLFile))
        {
            text = sr.ReadToEnd();
        }

        XmlDocument doc = new XmlDocument();
        doc.LoadXml(text);
        XmlElement allLevels = doc.DocumentElement;
        levelInfo.ExportToXML(allLevels);
        SortedDictionary<int, XmlElement> levelNodesDict = new SortedDictionary<int, XmlElement>();
        foreach (XmlElement node in allLevels.ChildNodes)
        {
            levelNodesDict.Add(int.Parse(node.Attributes["LevelID"].Value), node);
        }

        allLevels.RemoveAll();
        foreach (KeyValuePair<int, XmlElement> kv in levelNodesDict)
        {
            allLevels.AppendChild(kv.Value);
        }

        using (StreamWriter sw = new StreamWriter(LevelsXMLFile))
        {
            doc.Save(sw);
        }
    }
}