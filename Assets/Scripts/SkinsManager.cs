using System.Linq;
using UnityEditor;
using UnityEngine;

public class SkinsManager : MonoBehaviour
{
    [SerializeField] private SkinSettings[] skinsList;

    public int GetRandomType()
    {
        return Random.Range(0, skinsList.Length);
    }

    public Snake BuildSnake(int type, Vector3 position, Quaternion quaternion)
    {
        if (type > skinsList.Length - 1) type = 0;
        var skinSetting = skinsList[type];

        Snake snakePrefab = skinSetting.snakePrefab;
        ChangeMaterial(snakePrefab.gameObject, skinSetting.material);

        Tail tailPrefab = skinSetting.tailPrefab;
        ChangeMaterial(tailPrefab.gameObject, skinSetting.material);

        Transform detailPrefab = skinSetting.detailPrefab;
        ChangeMaterial(detailPrefab.gameObject, skinSetting.material);

        tailPrefab.SetDetailPrefab(detailPrefab);
        snakePrefab.SetTailPrefab(tailPrefab);

        Snake snake = Instantiate(snakePrefab, position, quaternion);
        return snake;
    }

    private void ChangeMaterial(GameObject target, Material material)
    {
        if (target.TryGetComponent<MaterialChanger>(out MaterialChanger changer))
        {
            changer.SetMaterial(material);
        }
    }
}
