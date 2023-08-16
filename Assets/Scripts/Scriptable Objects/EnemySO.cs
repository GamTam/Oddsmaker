using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Enemy", menuName = "RPG Battle/Enemy")]
public class EnemySO : ScriptableObject
{
    public string Name;
    public int Level;
    public int HP;
    public int Pow;
    public int Def;
    public int Speed;
    public int EXP;
    [TextArea(3, 5)] public string[] Description;
}
