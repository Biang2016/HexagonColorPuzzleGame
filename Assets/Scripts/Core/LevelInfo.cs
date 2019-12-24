using System.Xml;

public class LevelInfo : IClone<LevelInfo>
{
    public int LevelID;
    public int MapRounds;
    public int OptimumStep;
    public string LevelName;

    public MapInfo StartMapInfo;
    public MapInfo GoalMapInfo;

    public LevelInfo(int levelId, int mapRounds, int optimumStep, string levelName, MapInfo startMapInfo, MapInfo goalMapInfo)
    {
        LevelID = levelId;
        MapRounds = mapRounds;
        OptimumStep = optimumStep;
        LevelName = levelName;
        StartMapInfo = startMapInfo;
        GoalMapInfo = goalMapInfo;
    }

    public static LevelInfo GenerateFromXML(XmlNode node_LevelInfo)
    {
        int levelID = int.Parse(node_LevelInfo.Attributes["LevelID"].Value);
        int mapRounds = int.Parse(node_LevelInfo.Attributes["MapRounds"].Value);
        int optimumStep = int.Parse(node_LevelInfo.Attributes["OptimumStep"].Value);
        string levelName = node_LevelInfo.Attributes["LevelName"].Value;
        LevelInfo levelInfo = new LevelInfo(levelID, mapRounds, optimumStep, levelName, MapInfo.GenerateFromXML(node_LevelInfo.ChildNodes[0]), MapInfo.GenerateFromXML(node_LevelInfo.ChildNodes[1]));
        return levelInfo;
    }

    public void ExportToXML(XmlElement ele_AllLevel)
    {
        XmlDocument doc = ele_AllLevel.OwnerDocument;
        XmlElement old_node = null;
        foreach (XmlElement card_node in ele_AllLevel.ChildNodes)
        {
            if (card_node.Attributes["LevelID"].Value.Equals(LevelID.ToString()))
            {
                old_node = card_node;
            }
        }

        if (old_node != null)
        {
            ele_AllLevel.RemoveChild(old_node);
        }

        XmlElement level_ele = doc.CreateElement("LevelInfo");
        ele_AllLevel.AppendChild(level_ele);
        level_ele.SetAttribute("LevelID", LevelID.ToString());
        level_ele.SetAttribute("MapRounds", MapRounds.ToString());
        level_ele.SetAttribute("OptimumStep", OptimumStep.ToString());
        level_ele.SetAttribute("LevelName", LevelName);

        XmlElement startMapInfo_ele = doc.CreateElement("LevelStartMapInfo");
        level_ele.AppendChild(startMapInfo_ele);
        XmlElement goalMapInfo_ele = doc.CreateElement("LevelGoalMapInfo");
        level_ele.AppendChild(goalMapInfo_ele);

        StartMapInfo.ExportToXML(startMapInfo_ele);
        GoalMapInfo.ExportToXML(goalMapInfo_ele);
    }

    public LevelInfo Clone()
    {
        return new LevelInfo(LevelID, MapRounds, OptimumStep, LevelName, StartMapInfo.Clone(), GoalMapInfo.Clone());
    }
}