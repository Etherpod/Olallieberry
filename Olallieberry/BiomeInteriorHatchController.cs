using UnityEngine;

namespace Olallieberry;

public class BiomeInteriorHatchController : RotatingDoorController
{
    public void OnValidate()
    {
        movingPart = transform;
        openRotation = -85f;
        openingSpeed = 60f;
        closingSpeed = 60f;
    }

    protected override void InitializeMovement()
    {
        if (movingPart == null)
        {
            movingPart = transform;
        }

        base.InitializeMovement();

#if DEBUG
        Open();
#endif
    }
}