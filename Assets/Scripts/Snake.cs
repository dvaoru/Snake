using System;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;
using UnityEngine.UIElements;


public class Snake : MonoBehaviour
{

    public float Speed { get { return _speed; } }

    [SerializeField] private Transform _head;
    [SerializeField] private Tail _tailPrefab;
    [SerializeField] private float _speed = 2f;
    private Tail _tail;

    public void Init(int detailCount)
    {
        _tail = Instantiate(_tailPrefab, transform.position, Quaternion.identity);
        _tail.Init(_head, _speed, detailCount);
    }

    public void SetDetailCount(int detailCount)
    {
        _tail.SetDetailsCount(detailCount);
    }


    public void Destroy()
    {
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
