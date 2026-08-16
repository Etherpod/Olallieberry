using UnityEngine;

namespace Olallieberry;

public class BiomeInteriorHatchController : MonoBehaviour
{
    [Header("Hatch")]
    public float openAngle = -85f;
    public float rotationSpeed = 180f;

    private Quaternion _closedRotation;
    private Quaternion _openRotation;
    private bool _open;

    public void Awake()
    {
        _closedRotation = transform.localRotation;
        _openRotation = _closedRotation * Quaternion.Euler(0f, openAngle, 0f);
        
#if DEBUG
        Open();
#endif
    }

    public void Update()
    {
        Quaternion targetRotation = _open ? _openRotation : _closedRotation;

        transform.localRotation = Quaternion.RotateTowards(
            transform.localRotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );
    }

    public void Open()
    {
        _open = true;
    }

    public void Close()
    {
        _open = false;
    }

    public void SetOpen(bool open)
    {
        _open = open;
    }

    public void Toggle()
    {
        _open = !_open;
    }
}