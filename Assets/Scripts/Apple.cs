using System.Collections.Generic;
using Colyseus.Schema;
using UnityEngine;

public class Apple : MonoBehaviour
{

    private Vector2float _apple;
    internal void Init(Vector2float apple)
    {
        _apple = apple;
        _apple.OnChange += OnChange;
    }

    private void OnChange(List<DataChange> changes)
    {
        Debug.Log("Apple OnChange");
        Vector3 position = transform.position;
        foreach (var change in changes)
        {
            switch (change.Field)
            {
                case "x":
                    position.x = (float)change.Value;
                    break;
                case "z":
                    position.z = (float)change.Value;
                    break;
                default:
                    Debug.LogWarning("Яблоко не реагирует на изменение " + change.Field);
                    break;
            }
        }
        
        Debug.Log($"Яблоко переместилось с {transform.position} в {position}");
        transform.position = position;
        gameObject.SetActive(true);
    }

    internal void Destroy()
    {
        if (_apple != null) _apple.OnChange -= OnChange;
        Destroy(gameObject);

    }

    internal void Collect()
    {
        Dictionary<string, object> data = new Dictionary<string, object>()
        {
            {"id", _apple.id}
        };
        MultiplayerManager.Instance.SendMessageToServer("collect", data);
        gameObject.SetActive(false);
    }
}
