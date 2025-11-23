using System;
using System.Collections.Generic;
using System.Linq;
using Colyseus;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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
        Debug.Log("Мультиплеер подключение");
        Dictionary<String, object> joinData = new Dictionary<string, object>()
        {
            {"t", _skinManager.GetRandomType()},
            {"login", PlayerSettings.Instance.Login}
        };
        var client = new ColyseusClient("wss://snakeserver-4nd6.onrender.com");
        _room = await client.JoinOrCreate<State>(GameRoomName, joinData);
        _room.OnStateChange += OnChange;
    }

    private void OnChange(State state, bool isFirstState)
    {
        if (isFirstState == false) return;
        _room.OnStateChange -= OnChange;
        state.players.ForEach((key, player) =>
        {
            if (key == _room.SessionId) CreatePlayer(key, player);
            else CreateEnemy(key, player);
        });

        _room.State.players.OnAdd += CreateEnemy;
        _room.State.players.OnRemove += RemoveEnemy;

        _room.State.apples.ForEach(CreateApple);
        _room.State.apples.OnAdd += (key, apple) => CreateApple(apple);
        _room.State.apples.OnRemove += RemoveApple;
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
        Debug.Log("SendMessage " + key + " " + data.Keys);
        _room.Send(key, data);
    }

    public void SendMessageToServer(string key, string data)
    {
        Debug.Log("SendMessage " + key + " " + data);
        _room.Send(key, data);
    }
    #endregion

    #region Player
    [SerializeField] private PlayerAim _playerAim;
    [SerializeField] private Controller _controllerPrefab;
    [SerializeField] private Snake _snakePrefab;

    [SerializeField] private SkinsManager _skinManager;
    private void CreatePlayer(string clientId, Player player)
    {
        Vector3 position = new Vector3(player.x, 0, player.z);
        Quaternion quaternion = Quaternion.identity;

        Snake snake = Instantiate(_snakePrefab, position, quaternion);
        //Snake snake = _skinManager.BuildSnake(player.type, position, quaternion);
        snake.Init(player.d, player.login, true);

        PlayerAim aim = Instantiate(_playerAim, position, quaternion);
        aim.Init(snake._head, snake.Speed);

        Controller controller = Instantiate(_controllerPrefab);
        controller.Init(clientId, aim, player, snake);

        AddLeader(clientId, player);
    }

    #endregion

    #region Enemy
    Dictionary<string, EnemyController> _enemies = new Dictionary<string, EnemyController>();

    private void CreateEnemy(string clientId, Player player)
    {
        Vector3 position = new Vector3(player.x, 0, player.z);

        Snake snake = _skinManager.BuildSnake(player.type, position, Quaternion.identity);
        //Snake snake = Instantiate(_snakePrefab, position, Quaternion.identity);
        snake.Init(player.d, player.login);
        EnemyController enemy = snake.AddComponent<EnemyController>();
        enemy.Init(clientId, player, snake);
        _enemies.Add(clientId, enemy);
        AddLeader(clientId, player);
    }

    private void RemoveEnemy(string key, Player value)
    {
        RemoveLeader(key);
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

    #region Apple

    [SerializeField] private Apple _applePrefab;
    private Dictionary<Vector2float, Apple> _apples = new Dictionary<Vector2float, Apple>();
    private void CreateApple(Vector2float vector2float)
    {
        Vector3 position = new Vector3(vector2float.x, 0, vector2float.z);
        var apple = Instantiate(_applePrefab, position, Quaternion.identity);
        apple.Init(vector2float);
        _apples.Add(vector2float, apple);
    }

    private void RemoveApple(int key, Vector2float vector2float)
    {
        if (_apples.ContainsKey(vector2float) == false) return;
        var apple = _apples[vector2float];
        _apples.Remove(vector2float);
        apple.Destroy();
    }
    #endregion

    #region Leaderbord
    private class LoginScorePair
    {
        public string login;
        public float score;
    }

    [SerializeField] private Text _text;
    private Dictionary<string, LoginScorePair> _leaders = new Dictionary<string, LoginScorePair>();

    private void AddLeader(string sesionId, Player player)
    {
        if (_leaders.ContainsKey(sesionId)) return;
        _leaders.Add(sesionId, new LoginScorePair()
        {
            login = player.login,
            score = player.score
        });
        UpdateLeaderboard();
    }

    private void RemoveLeader(string sessionId)
    {
        if (_leaders.ContainsKey(sessionId) == false) return;
        _leaders.Remove(sessionId);
        UpdateLeaderboard();
    }

    public void UpdateScore(string sessionId, int score)
    {
        if (_leaders.ContainsKey(sessionId) == false) return;
        _leaders[sessionId].score = score;
        UpdateLeaderboard();
    }

    private void UpdateLeaderboard()
    {
        var topCount = Mathf.Clamp(_leaders.Count, 0, 10);
        var top = _leaders.OrderByDescending(pair => pair.Value.score).Take(topCount);
        var text = "";
        var i = 1;
        foreach (var item in top)
        {
            text += $"{i}. {item.Value.login}: {item.Value.score}\n";
        }
        _text.text = text;
    }
    #endregion

    #region RestartScreen
    [SerializeField] private GameObject _restartScreen;
    [SerializeField] private TextMeshProUGUI _scoreText;

    public void ShowRestartScreen(int playerScore)
    {
        _scoreText.text = playerScore.ToString();
        _restartScreen.SetActive(true);
    }

    public async void RestartGame()
    {

        _enemies.Clear();
        _apples.Clear();
        _leaders.Clear();

        await _room.Leave();
        SceneManager.LoadScene("Lobby");
    }
    #endregion

}
