
using System.Collections.Generic;
using UnityEngine;

public class Tail : MonoBehaviour
{
    [SerializeField] private Transform _detailPrefab;

    private Transform _head;
    [SerializeField] private float _detailDistance = 1;
    private float _snakeSpeed = 2f;
    private List<Transform> _details = new List<Transform>();
    private List<Vector3> _positionsHistory = new List<Vector3>();
    private List<Quaternion> _rotationHistory = new List<Quaternion>();


    public void Init(Transform head, float speed, int detailCount)
    {
        _head = head;
        _snakeSpeed = speed;
        _details.Add(transform);
        _positionsHistory.Add(_head.position);
        _rotationHistory.Add(_head.rotation);
        _positionsHistory.Add(transform.position);
        _rotationHistory.Add(transform.rotation);
        SetDetailsCount(detailCount);
    }

    public void Destroy()
    {
        for (int i = 0; i < _details.Count; i++)
        {
            Destroy(_details[i].gameObject);
        }
    }

    public void SetDetailsCount(int detailsCount)
    {
        if (detailsCount == _details.Count - 1) return;
        int diff = (_details.Count - 1) - detailsCount;
        if (diff < 1)
        {
            for (int i = 0; i < -diff; i++)
            {
                AddDetail();
            }
        }
        else
        {
            for (int i = 0; i < diff; i++)
            {
                RemoveDetail();
            }
        }
    }

    private void AddDetail()
    {
        Vector3 position = _details[_details.Count - 1].position;
        Quaternion rotation = _details[_details.Count - 1].rotation;
        Transform detail = Instantiate(_detailPrefab, position, rotation);
        _details.Insert(0, detail);
        _positionsHistory.Add(position);
        _rotationHistory.Add(rotation);

    }

    private void RemoveDetail()
    {
        if (_details.Count <= 1)
        {
            Debug.LogError("Пытаемся удалить деталь которой нет");
            return;
        }
        Transform detail = _details[0];
        _details.Remove(detail);
        Destroy(detail.gameObject);
        _positionsHistory.RemoveAt(_positionsHistory.Count - 1);
        _rotationHistory.RemoveAt(_positionsHistory.Count - 1);
    }

    public void Update()
    {
        float distance = (_head.position - _positionsHistory[0]).magnitude;
        while (distance > _detailDistance)
        {
            Vector3 direction = (_head.position - _positionsHistory[0]).normalized;
            _positionsHistory.Insert(0, _positionsHistory[0] + direction * _detailDistance);
            _positionsHistory.RemoveAt(_positionsHistory.Count - 1);

            _rotationHistory.Insert(0, _head.rotation);
            _rotationHistory.RemoveAt(_rotationHistory.Count - 1);
            distance -= _detailDistance;
        }

        for (int i = 0; i < _details.Count; i++)
        {
            float percent = distance / _detailDistance;
            _details[i].position = Vector3.Lerp(_positionsHistory[i + 1], _positionsHistory[i], percent);
            _details[i].rotation = Quaternion.Lerp(_rotationHistory[i + 1], _rotationHistory[i], percent);
        }
    }


}
