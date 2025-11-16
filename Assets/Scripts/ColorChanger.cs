using UnityEngine;

public class MaterialChanger : MonoBehaviour
{
    [SerializeField] private MeshRenderer[] _meshRenderers;

    public void SetMaterial(Material material)
    {
        for (int i = 0; i < _meshRenderers.Length; i++)
        {
            _meshRenderers[i].material = material;
        }
    }
}
