using System;
using System.Xml;

public class MapGridInfo : IClone<MapGridInfo>
{
    public HexPos HexPos;
    public MapGridTypes MapGridType;
    public MapGridColorTypes MapGridColorType;

    public MapGridInfo(HexPos hexPos, MapGridTypes mapGridType, MapGridColorTypes mapGridColorType)
    {
        HexPos = hexPos;
        MapGridType = mapGridType;
        MapGridColorType = mapGridColorType;
    }

    public static MapGridInfo GenerateFromXML(XmlElement mg)
    {
        MapGridTypes mapGridType = (MapGridTypes) Enum.Parse(typeof(MapGridTypes), mg.Attributes["MapGridType"].Value);
        MapGridColorTypes mapGridColorType = (MapGridColorTypes) Enum.Parse(typeof(MapGridColorTypes), mg.Attributes["MapGridColorType"].Value);
        int hexPosX = int.Parse(mg.Attributes["HexPosX"].Value);
        int hexPosY = int.Parse(mg.Attributes["HexPosY"].Value);
        MapGridInfo mgi = new MapGridInfo(new HexPos(hexPosX, hexPosY), mapGridType, mapGridColorType);
        return mgi;
    }

    public void ExportToXML(XmlNode MapInfo_node)
    {
        XmlDocument doc = MapInfo_node.OwnerDocument;
        XmlElement mapGrid_ele = doc.CreateElement("MapGrid");
        MapInfo_node.AppendChild(mapGrid_ele);

        mapGrid_ele.SetAttribute("HexPosX", HexPos.X.ToString());
        mapGrid_ele.SetAttribute("HexPosY", HexPos.Y.ToString());
        mapGrid_ele.SetAttribute("MapGridColorType", MapGridColorType.ToString());
        mapGrid_ele.SetAttribute("MapGridType", MapGridType.ToString());
    }

    public MapGridInfo Clone()
    {
        return new MapGridInfo(HexPos, MapGridType, MapGridColorType);
    }
}