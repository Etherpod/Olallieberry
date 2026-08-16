using UnityEngine;

namespace Olallieberry;

public class BeachTidesController : MonoBehaviour
{
    [Header("Water Level")]
    public float floorHeight = 0f;
    public float lowerWaterHeight = 0.48f;
    public float upperWaterHeight = 20.8f;
    public float ceilingHeight = 32f;

    [Min(0f)]
    public float riseSpeed = 1f;

    private float _waterHeight;

    public void Awake()
    {
        _waterHeight = lowerWaterHeight;
        SetWaterHeight(_waterHeight);
    }

    public void Update()
    {
        _waterHeight = Mathf.MoveTowards(
            _waterHeight,
            upperWaterHeight,
            riseSpeed * Time.deltaTime);

        SetWaterHeight(_waterHeight);
    }

    public void SetWaterHeight(float waterHeight)
    {
        float height = waterHeight - floorHeight;
        float scaleY = Mathf.Clamp01(height / ceilingHeight);

        var scale = transform.localScale;
        scale.y = scaleY;
        transform.localScale = scale;
    }
}