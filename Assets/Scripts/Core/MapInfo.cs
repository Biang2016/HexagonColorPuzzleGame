using System.Collections.Generic;
using System.Xml;

public class MapInfo : IClone<MapInfo>
{
    public List<MapGridInfo> MapGridInfos = new List<MapGridInfo>();

    public static MapInfo GenerateFromXML(XmlNode node_MapInfo)
    {
        MapInfo mapInfo = new MapInfo();
        XmlNodeList mapGrids = node_MapInfo.SelectNodes("MapGrid");
        if (mapGrids != null)
        {
            foreach (XmlElement mg in mapGrids)
            {
                mapInfo.MapGridInfos.Add(MapGridInfo.GenerateFromXML(mg));
            }
        }

        return mapInfo;
    }

    public void ExportToXML(XmlElement ele_MapInfo)
    {
        ele_MapInfo.RemoveAll();
        foreach (MapGridInfo mgi in MapGridInfos)
        {
            mgi.ExportToXML(ele_MapInfo);
        }
    }

    public MapInfo Clone()
    {
        MapInfo newMapInfo = new MapInfo();
        newMapInfo.MapGridInfos = CloneVariantUtils.List(MapGridInfos);
        return newMapInfo;
    }
}