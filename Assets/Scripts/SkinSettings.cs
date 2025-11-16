using UnityEngine;

[CreateAssetMenu(
    fileName = "SkinSettings",
    menuName = "Skin Settings",
    order = 0)]
public class SkinSettings : ScriptableObject
{
    public Snake snakePrefab;
    public Transform detailPrefab;
    public Tail tailPrefab;
    public Material material;

}
