using System.Collections.Generic;
using Colyseus.Schema;
using Unity.VisualScripting;
using UnityEngine;

public class Controller : MonoBehaviour
{
    private Snake _snake;
    [SerializeField] private float _cameraOffsetY = 15f;
    [SerializeField] private Transform _cursor;
    private Camera _camera;
    private Plane _plane;
    private Player _player;
    private PlayerAim _playerAim;

    private MultiplayerManager _multiplayerManager;
    public void Init(PlayerAim aim, Player player, Snake snake)
    {
        _multiplayerManager = MultiplayerManager.Instance;
        _playerAim = aim;
        _player = player;
        _snake = snake;
        _camera = Camera.main;
        _plane = new Plane(Vector3.up, Vector3.zero);

        _snake.AddComponent<CameraManager>().Init(_cameraOffsetY);
        _player.OnChange += OnChange;

    }

    public void Update()
    {
        if (Input.GetMouseButton(0))
        {
            MoveCursor();
            _playerAim.SetTargetDirection(_cursor.position);
        }

        SendMove();
    }

    private void SendMove()
    {
        _playerAim.GetMoveInfo(out Vector3 position);
        Dictionary<string, object> data = new Dictionary<string, object>()
        {
            {"x", position.x},
            {"z", position.z}
        };
        _multiplayerManager.SendMessageToServer("move", data);
    }

    private void SendSkin(byte skinType)
    {
          Dictionary<string, byte> data = new Dictionary<string, byte>()
          {
            {"t", skinType}  
          };
          _multiplayerManager.SendMessage("skin", data);
    }

    private void MoveCursor()
    {
        Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
        _plane.Raycast(ray, out float distance);
        Vector3 point = ray.GetPoint(distance);
        _cursor.position = point;
    }

    private void OnChange(List<DataChange> changes)
    {
        Vector3 position = _snake.transform.position;
        for (int i = 0; i < changes.Count; i++)
        {
            switch (changes[i].Field)
            {
                case "x":
                    position.x = (float)changes[i].Value;
                    break;
                case "z":
                    position.z = (float)changes[i].Value;
                    break;
                case "d":
                    _snake.SetDetailCount((byte)changes[i].Value);
                    break;
                default:
                    Debug.LogWarning("Не обрабатывается изменение поля " + changes[i].Field);
                    break;
            }
        }
        _snake.SetRotation(position);
    }

    public void Destroy()
    {
        _player.OnChange -= OnChange;
        _snake.Destroy();
    }

}
