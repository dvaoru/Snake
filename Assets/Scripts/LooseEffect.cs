using UnityEngine;

public class LooseEffect : MonoBehaviour
{
    [SerializeField] private GameObject _looseEffect ;

    public void OnDestroy()
    {
        Instantiate(_looseEffect, transform.position, transform.rotation);
    }
}
