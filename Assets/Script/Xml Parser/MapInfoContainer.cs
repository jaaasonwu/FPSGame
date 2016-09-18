/*
Created by Haoyu Zhai zhaih@student.unimelb.edu.au
the script is used to store the map information readed from
an xml file, especially for the spawn point of the map
*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.IO;

public class MapInfoContainer {
    // the list used to store the spawn points
    public List<Vector3> spawnPoints = new List<Vector3>();

    public const string INFO_PATH = "TextAssets/MapInfo.xml";

    public static MapInfoContainer Load(string mapName, string path)
    {
        using (var stream = new FileStream(path, FileMode.Open))
        {
            MapInfoContainer mapInfo = new MapInfoContainer();
            XmlDocument xml = new XmlDocument();
            xml.Load(stream);
            foreach (XmlNode map in xml["Maps"].ChildNodes)
            {
                // just read the wanted map's information
                if (map.Attributes["name"].Value != mapName)
                    continue;
                foreach(XmlNode spawnPoint in map["SpawnPoints"].ChildNodes)
                {
                    Vector3 point = new Vector3();
                    point.x = float.Parse(spawnPoint["x"].InnerText);
                    point.y = float.Parse(spawnPoint["y"].InnerText);
                    point.z = float.Parse(spawnPoint["z"].InnerText);
                    mapInfo.spawnPoints.Add(point);
                }
            }
            return mapInfo;
        }
    }
}
