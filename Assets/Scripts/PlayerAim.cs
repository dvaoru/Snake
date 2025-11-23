using System;
using UnityEngine;

public class PlayerAim : MonoBehaviour
{

    [SerializeField] private LayerMask _collisionLayer;
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
        CheckExit();
    }



    public void FixedUpdate()
    {
        CheckCollision();
    }

    private void CheckCollision()
    {
        Collider[] colliders = Physics.OverlapSphere(_snakeHead.position, _overlapRadius, _collisionLayer);
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i].TryGetComponent(out Apple apple))
            {
                apple.Collect();
            }
            else
            {
                if (colliders[i].GetComponentInParent<Snake>())
                {
                    Debug.Log(colliders[i].name);
                    Transform enemy = colliders[i].transform;
                    float playerAngle = Vector3.Angle(enemy.position - _snakeHead.position, _snakeHead.forward);
                    float enemyAngle = Vector3.Angle(_snakeHead.position - enemy.position, enemy.forward);
                    if (playerAngle < enemyAngle + 5)
                    {
                        GameOver();
                    }
                }
                else
                {
                    GameOver();
                }
            }
        }
    }

    private void GameOver()
    {
        FindAnyObjectByType<Controller>().Destroy();
        Destroy(gameObject);
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

    private void CheckExit()
    {
        if ((Mathf.Abs(_snakeHead.position.x) > 128) || (Mathf.Abs(_snakeHead.position.z) > 128))
        {
            GameOver();
        }
    }
    public void SetTargetDirection(Vector3 pointToLook)
    {
        // Debug.Log("SetTargetDirection " + pointToLook);
        _targetDirection = pointToLook - transform.position;
    }

    public void GetMoveInfo(out Vector3 position)
    {
        position = transform.position;
    }
}
