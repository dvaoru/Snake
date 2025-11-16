using System;
using System.Collections.Generic;
using Colyseus;
using Unity.VisualScripting;
using UnityEngine;

public class MultiplayerManager : ColyseusManager<MultiplayerManager>
{
    #region Server
    private const string GameRoomName = "state_handler";
    private ColyseusRoom<State> _room;
    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);
        InitializeClient();
        Connection();
    }

    private async void Connection()
    {
        Dictionary<String, object> joinData = new Dictionary<string, object>()
        {
            {"t", _skinManager.GetRandomType()}
        };
        _room = await client.JoinOrCreate<State>(GameRoomName, joinData);
        _room.OnStateChange += OnChange;
    }

    private void OnChange(State state, bool isFirstState)
    {
        if (isFirstState == false) return;
        _room.OnStateChange -= OnChange;
        state.players.ForEach((key, player) =>
        {
            if (key == _room.SessionId) CreatePlayer(player);
            else CreateEnemy(key, player);
        });

        _room.State.players.OnAdd += CreateEnemy;
        _room.State.players.OnRemove += RemoveEnemy;
    }

    protected override void OnApplicationQuit()
    {
        base.OnApplicationQuit();
        LeaveRoom();
    }

    public void LeaveRoom()
    {
        _room?.Leave();
    }

    public void SendMessageToServer(string key, Dictionary<string, object> data)
    {
        _room.Send(key, data);
    }
    #endregion

    #region Player
    [SerializeField] private PlayerAim _playerAim;
    [SerializeField] private Controller _controllerPrefab;
    [SerializeField] private Snake _snakePrefab;

    [SerializeField] private SkinsManager _skinManager;
    private void CreatePlayer(Player player)
    {
        Vector3 position = new Vector3(player.x, 0, player.z);
        Quaternion quaternion = Quaternion.identity;

        //Snake snake = Instantiate(_snakePrefab, position, quaternion);
        Snake snake = _skinManager.BuildSnake(player.type, position, quaternion);
        snake.Init(player.d);

        PlayerAim aim = Instantiate(_playerAim, position, quaternion);
        aim.Init(snake.Speed);

        Controller controller = Instantiate(_controllerPrefab);
        controller.Init(aim, player, snake);
    }

    #endregion

    #region Enemy
    Dictionary<string, EnemyController> _enemies = new Dictionary<string, EnemyController>();

    private void CreateEnemy(string key, Player player)
    {
        Vector3 position = new Vector3(player.x, 0, player.z);
        
         Snake snake = _skinManager.BuildSnake(player.type, position, Quaternion.identity);
        //Snake snake = Instantiate(_snakePrefab, position, Quaternion.identity);
        snake.Init(player.d);
        EnemyController enemy = snake.AddComponent<EnemyController>();
        enemy.Init(player, snake);
        _enemies.Add(key, enemy);
    }

    private void RemoveEnemy(string key, Player value)
    {
        if (_enemies.ContainsKey(key) == false)
        {
            Debug.LogError("Попытка удаления врага, которого неи в словаре");
            return;
        }
        EnemyController enemy = _enemies[key];
        _enemies.Remove(key);
        enemy.Destroy();


    }
    #endregion
}
