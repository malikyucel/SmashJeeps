 using UnityEngine;

public class CharacterSelectVisual : MonoBehaviour
{
    [SerializeField] private MeshRenderer _baseMashRenderer;

    private Material _material;

    private void Awake()
    {
        _material = new Material(_baseMashRenderer.material);
        _baseMashRenderer.material = _material;
    }

    public void SetPlayerColor(Color color)
    {
        _material.color = color;
    }


}
