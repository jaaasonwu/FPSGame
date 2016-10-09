/*
Created by Haoyu Zhai zhaih@student.unimelb.edu.au
this script is to store the infomation that a kind of
enemy have, which should be extracted from an xml file
*/
using UnityEngine;
using System.Collections;
using System.Xml;
using System.Xml.Serialization;

// This class can be serialized to save enemy information
public class EnemyInfo {
    public int id;
    public float hp;
    public string type;
    public float hateRange;

    public float normalActiveRange;

    public float attractedActiveRange;

    public float attackRange;

    public float attackDamage;

    public float attackSpeed;

    public string attackMethod;
}
