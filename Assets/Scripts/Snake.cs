using UnityEngine;



public class Snake : MonoBehaviour
{

    public float Speed { get { return _speed; } }
    [SerializeField] private int _playerLayer = 6;

    [field: SerializeField] public Transform _head {get; private set;}
    [SerializeField] private Tail _tailPrefab;
    [SerializeField] private float _speed = 2f;
    private Tail _tail;

    public void Init(int detailCount, bool isPlayer = false)
    {
        if (isPlayer)
        {
            gameObject.layer = _playerLayer;
            var childrens = GetComponentsInChildren<Transform>();
            foreach (var item in childrens)
            {
                item.gameObject.layer = _playerLayer;
            }
        }
        _tail = Instantiate(_tailPrefab, transform.position, Quaternion.identity);
        _tail.Init(_head, _speed, detailCount, _playerLayer, isPlayer);
    }

    public void SetDetailCount(int detailCount)
    {
        _tail.SetDetailsCount(detailCount);
    }

    public void SetTailPrefab(Tail tail)
    {
        _tail = tail;
    }

    public Tail GetTail()
    {
        return _tail;
    }

    public void Destroy(string clientId)
    {
        string json = "";
        var detailPositions = _tail.GetDetailPositions();
        detailPositions.id = clientId;
        json = JsonUtility.ToJson(detailPositions);
        MultiplayerManager.Instance.SendMessageToServer("gameOver", json);
        _tail.Destroy();
        Destroy(gameObject);
    }
    private void Update()
    {
        Move();

    }



    private void Move()
    {
        transform.position += _head.forward * Time.deltaTime * _speed;
    }


    private Vector3 _targetDirection = Vector3.zero;

    public void SetRotation(Vector3 pointToLook)
    {
        _head.LookAt(pointToLook);
    }




}
