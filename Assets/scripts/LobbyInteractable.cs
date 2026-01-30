using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class LobbyInteractable : MonoBehaviour
{
    public enum InteractionType
    {
        GameStart,
        Ranking,
        CharacterCustomization,
        Shop,
        Profile,
        Options,
        Tutorial,
        None
    }

    [Header("Settings")]
    public InteractionType type = InteractionType.None;
    public string displayName = "Interactable";
    public Color highlightColor = Color.yellow;

    [Header("Events")]
    public UnityEvent OnInteract;

    private Renderer _renderer;
    private Color _originalColor;
    private Material _material;

    private void Start()
    {
        _renderer = GetComponent<Renderer>();
        if (_renderer != null)
        {
            _material = _renderer.material; // 인스턴스 생성
            if (_material.HasProperty("_Color"))
            {
                _originalColor = _material.color;
            }
            else if (_material.HasProperty("_BaseColor")) // URP 등
            {
                _originalColor = _material.GetColor("_BaseColor");
            }
        }
    }

    private void OnMouseEnter()
    {
        if (_material != null)
        {
            if (_material.HasProperty("_Color"))
            {
                _material.color = highlightColor;
            }
            else if (_material.HasProperty("_BaseColor"))
            {
                _material.SetColor("_BaseColor", highlightColor);
            }
        }
        
        // TODO: 마우스 커서 변경이나 툴팁 표시 추가 가능
        Debug.Log($"Hover: {displayName}");
    }

    private void OnMouseExit()
    {
        if (_material != null)
        {
            if (_material.HasProperty("_Color"))
            {
                _material.color = _originalColor;
            }
            else if (_material.HasProperty("_BaseColor"))
            {
                _material.SetColor("_BaseColor", _originalColor);
            }
        }
    }

    private void OnMouseDown()
    {
        Debug.Log($"Click: {displayName} ({type})");
        OnInteract?.Invoke();
        
        // SchoolLobbyManager.Instance?.OnInteract(type); // [FIX] Removed deprecated call
    }
}
