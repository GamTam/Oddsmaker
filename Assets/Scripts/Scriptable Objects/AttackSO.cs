using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;

[CreateAssetMenu(fileName = "New Attack", menuName = "RPG Battle/Attack")]
public class AttackSO : ScriptableObject
{
    public string Name;
    public int Strength;
    public int Cost;
    public DamageTypes CostType;
}