/*
Created by Hoayu Zhai zhaih@student.unimelb.edu.au
the script used to read the enemy info out of the xml
file
*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.IO;

public class EnemyInfoContainer {

    // the collection that will store the info
    public Dictionary<string, EnemyInfo> enemyInfos = new Dictionary<string, EnemyInfo>();

    // the xml info path
    public const string INFO_PATH = "TextAssets/EnemyInfo.xml";

    // use this function to get the enemy infos
    public static EnemyInfoContainer Load(string path)
    {
        using (var stream = new FileStream(path, FileMode.Open))
        {
            EnemyInfoContainer infos = new EnemyInfoContainer();
            XmlDocument xml = new XmlDocument();
            xml.Load(stream);
            foreach(XmlNode enemy in xml["Enemies"].ChildNodes)
            {
                string name = enemy.Attributes["name"].Value;
                EnemyInfo enemyInfo = new EnemyInfo();
                enemyInfo.baseHP = float.Parse(enemy["baseHP"].InnerText);
                enemyInfo.moveSpeed = float.Parse(enemy["moveSpeed"].InnerText);
                enemyInfo.rotateSpeed = float.Parse(enemy["rotateSpeed"].InnerText);
                enemyInfo.hateRange = float.Parse(enemy["hateRange"].InnerText);
                enemyInfo.attackRange = float.Parse(enemy["attackRange"].InnerText);
                enemyInfo.attackDamage = float.Parse(enemy["attackDamage"].InnerText);
                enemyInfo.normalActiveRange = float.Parse(enemy["normalActiveRange"].InnerText);
                enemyInfo.attractedActiveRange = float.Parse(enemy["attractedActiveRange"].InnerText);
                enemyInfo.attackSpeed = float.Parse(enemy["attackSpeed"].InnerText);
                enemyInfo.attackMethod = enemy["attackMethod"].InnerText;
                infos.enemyInfos[name] = enemyInfo;
            }
            return infos;
        }
    }
}
