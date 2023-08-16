using System;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.AddressableAssets;

[CreateAssetMenu(fileName = "New Evidence", menuName = "Ace Attorney/Evidence")]
[Serializable]
public class EvidenceSO : ScriptableObject
{
    public string Name;
    [ShowAssetPreview(64, 64)] public Sprite Icon;
    [ShowAssetPreview(128, 128)] [AllowNesting] public Sprite[] CheckImages;
    [TextArea(3, 4)]public string Description;   
}