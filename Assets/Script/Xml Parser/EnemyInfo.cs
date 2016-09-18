/*
Created by Haoyu Zhai zhaih@student.unimelb.edu.au
this script is to store the infomation that a kind of
enemy have, which should be extracted from an xml file
*/
using UnityEngine;
using System.Collections;
using System.Xml;
using System.Xml.Serialization;

public class EnemyInfo {

    public float baseHP;

    public float moveSpeed;

    public float rotateSpeed;

    public float hateRange;

    public float normalActiveRange;

    public float attractedActiveRange;

    public float attackRange;

}
