using System;
using UnityEngine;

public class PlayerAim : MonoBehaviour
{

    [SerializeField] private float _overlapRadius = 0.85f;
    [SerializeField] private float _rotationSpeed = 90f;
    private Vector3 _targetDirection = Vector3.zero;
    private float _speed;

    private Transform _snakeHead;


    public void Init(Transform snakeHead, float speed)
    {
        _speed = speed;
        _snakeHead = snakeHead;
    }
    public void Update()
    {
        Rotate();
        Move();
    }

    public void FixedUpdate()
    {
        CheckCollision();
    }

    private void CheckCollision()
    {
        Collider[] colliders = Physics.OverlapSphere(_snakeHead.position, _overlapRadius);
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i].TryGetComponent(out Apple apple))
            {
                apple.Collect();
            }
        }
    }

    private void Rotate()
    {
        Quaternion targetRotation = Quaternion.LookRotation(_targetDirection);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, Time.deltaTime * _rotationSpeed);
    }

    private void Move()
    {
        transform.position += transform.forward * _speed * Time.deltaTime;
    }

    public void SetTargetDirection(Vector3 pointToLook)
    {
        Debug.Log("SetTargetDirection " + pointToLook);
        _targetDirection = pointToLook - transform.position;
    }

    public void GetMoveInfo(out Vector3 position)
    {
        position = transform.position;
    }
}
