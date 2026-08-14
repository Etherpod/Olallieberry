using HarmonyLib;
using ShapeCollision;
using UnityEngine;

namespace Olallieberry;

[HarmonyPatch]
public static class Patches
{
    [HarmonyPatch(typeof(ShapeManager), nameof(ShapeManager.TestCollision))]
    [HarmonyPrefix]
    public static bool ShapeManager_TestCollision(
        ShapeManager __instance,
        ShapeManager.ShapeData detector,
        ShapeManager.ShapeData volume,
        ref bool __result)
    {
        // Only handle collision pairs that we need
        if (volume.type != ShapeManager.ShapeData.Type.Cylinder)
        {
            return true;
        }

        if (detector.type != ShapeManager.ShapeData.Type.Box &&
            detector.type != ShapeManager.ShapeData.Type.Capsule)
        {
            return true;
        }

        if (!Intersection.SphereSphere(
            detector.worldBoundsCenter,
            detector.worldBoundsRadius,
            volume.worldBoundsCenter,
            volume.worldBoundsRadius))
        {
            __result = false;
            return false;
        }

        if (detector.shapeDataDirty)
        {
            __instance.UpdateWorldShapeData(detector);
        }

        if (volume.shapeDataDirty)
        {
            __instance.UpdateWorldShapeData(volume);
        }

        switch (detector.type)
        {
            case ShapeManager.ShapeData.Type.Box:
                __result = BoxCylinder(
                    detector.box.worldCenter,
                    detector.box.worldSize,
                    detector.box.worldAxes,
                    volume.capsule.worldStartPoint,
                    volume.capsule.worldEndPoint,
                    volume.capsule.worldRadius);

                return false;

            case ShapeManager.ShapeData.Type.Capsule:
                __result = CapsuleCylinder(
                    detector.capsule.worldStartPoint,
                    detector.capsule.worldEndPoint,
                    detector.capsule.worldRadius,
                    volume.capsule.worldStartPoint,
                    volume.capsule.worldEndPoint,
                    volume.capsule.worldRadius);

                return false;
        }

        return true;
    }

    private static bool CapsuleCylinder(
        Vector3 capsuleStart,
        Vector3 capsuleEnd,
        float capsuleRadius,
        Vector3 cylinderStart,
        Vector3 cylinderEnd,
        float cylinderRadius)
    {
        ConvexShape a = ConvexShape.Capsule(
            capsuleStart,
            capsuleEnd,
            capsuleRadius);

        ConvexShape b = ConvexShape.Cylinder(
            cylinderStart,
            cylinderEnd,
            cylinderRadius);

        return GjkIntersect(a, b);
    }

    private static bool BoxCylinder(
        Vector3 boxCenter,
        Vector3 boxSize,
        Vector3[] boxAxes,
        Vector3 cylinderStart,
        Vector3 cylinderEnd,
        float cylinderRadius)
    {
        ConvexShape a = ConvexShape.Box(
            boxCenter,
            boxSize,
            boxAxes);

        ConvexShape b = ConvexShape.Cylinder(
            cylinderStart,
            cylinderEnd,
            cylinderRadius);

        return GjkIntersect(a, b);
    }

    private static bool GjkIntersect(
        ConvexShape a,
        ConvexShape b)
    {
        Vector3 direction = b.center - a.center;

        if (direction.sqrMagnitude < 1E-08f)
        {
            direction = Vector3.right;
        }

        GjkSimplex simplex = new();

        Vector3 support =
            GetSupport(a, b, direction);

        simplex.Add(support);
        direction = -support;

        for (int i = 0; i < 32; i++)
        {
            if (direction.sqrMagnitude < 1E-10f)
            {
                return true;
            }

            support = GetSupport(a, b, direction);

            if (Vector3.Dot(support, direction) < 0f)
            {
                return false;
            }

            simplex.Add(support);

            if (NextSimplex(ref simplex, ref direction))
            {
                return true;
            }
        }

        return false;
    }

    private static Vector3 GetSupport(
        ConvexShape a,
        ConvexShape b,
        Vector3 direction)
    {
        return a.GetSupport(direction) -
               b.GetSupport(-direction);
    }

    private static bool NextSimplex(
        ref GjkSimplex simplex,
        ref Vector3 direction)
    {
        switch (simplex.count)
        {
            case 2:
                return LineSimplex(
                    ref simplex,
                    ref direction);

            case 3:
                return TriangleSimplex(
                    ref simplex,
                    ref direction);

            case 4:
                return TetrahedronSimplex(
                    ref simplex,
                    ref direction);
        }

        direction = -simplex.a;

        return direction.sqrMagnitude < 1E-10f;
    }

    private static bool LineSimplex(
        ref GjkSimplex simplex,
        ref Vector3 direction)
    {
        Vector3 a = simplex.a;
        Vector3 b = simplex.b;

        Vector3 ab = b - a;
        Vector3 ao = -a;

        if (SameDirection(ab, ao))
        {
            direction =
                Vector3.Cross(
                    Vector3.Cross(ab, ao),
                    ab);

            if (direction.sqrMagnitude < 1E-10f)
            {
                return true;
            }
        }
        else
        {
            simplex.count = 1;
            direction = ao;
        }

        return false;
    }

    private static bool TriangleSimplex(
        ref GjkSimplex simplex,
        ref Vector3 direction)
    {
        Vector3 a = simplex.a;
        Vector3 b = simplex.b;
        Vector3 c = simplex.c;

        Vector3 ab = b - a;
        Vector3 ac = c - a;
        Vector3 ao = -a;

        Vector3 abc = Vector3.Cross(ab, ac);

        Vector3 acPerpendicular =
            Vector3.Cross(abc, ac);

        if (SameDirection(acPerpendicular, ao))
        {
            if (SameDirection(ac, ao))
            {
                simplex.b = c;
                simplex.count = 2;

                direction =
                    Vector3.Cross(
                        Vector3.Cross(ac, ao),
                        ac);

                if (direction.sqrMagnitude < 1E-10f)
                {
                    return true;
                }

                return false;
            }

            simplex.count = 2;

            return LineSimplex(
                ref simplex,
                ref direction);
        }

        Vector3 abPerpendicular =
            Vector3.Cross(ab, abc);

        if (SameDirection(abPerpendicular, ao))
        {
            simplex.count = 2;

            return LineSimplex(
                ref simplex,
                ref direction);
        }

        if (SameDirection(abc, ao))
        {
            direction = abc;
        }
        else
        {
            simplex.b = c;
            simplex.c = b;
            direction = -abc;
        }

        return direction.sqrMagnitude < 1E-10f;
    }

    private static bool TetrahedronSimplex(
        ref GjkSimplex simplex,
        ref Vector3 direction)
    {
        Vector3 a = simplex.a;
        Vector3 b = simplex.b;
        Vector3 c = simplex.c;
        Vector3 d = simplex.d;

        Vector3 ao = -a;

        Vector3 abc =
            Vector3.Cross(b - a, c - a);

        if (Vector3.Dot(abc, d - a) > 0f)
        {
            abc = -abc;
        }

        if (SameDirection(abc, ao))
        {
            simplex.count = 3;
            direction = abc;
            return false;
        }

        Vector3 acd =
            Vector3.Cross(c - a, d - a);

        if (Vector3.Dot(acd, b - a) > 0f)
        {
            acd = -acd;
        }

        if (SameDirection(acd, ao))
        {
            simplex.b = c;
            simplex.c = d;
            simplex.count = 3;
            direction = acd;
            return false;
        }

        Vector3 adb =
            Vector3.Cross(d - a, b - a);

        if (Vector3.Dot(adb, c - a) > 0f)
        {
            adb = -adb;
        }

        if (SameDirection(adb, ao))
        {
            simplex.b = d;
            simplex.c = b;
            simplex.count = 3;
            direction = adb;
            return false;
        }

        return true;
    }

    private static bool SameDirection(
        Vector3 direction,
        Vector3 ao)
    {
        return Vector3.Dot(direction, ao) > 0f;
    }

    private enum ConvexShapeType
    {
        Capsule,
        Cylinder,
        Box
    }

    private struct ConvexShape
    {
        public ConvexShapeType type;

        public Vector3 center;

        public Vector3 start;
        public Vector3 end;
        public float radius;

        public Vector3 size;
        public Vector3[] axes;

        public static ConvexShape Capsule(
            Vector3 start,
            Vector3 end,
            float radius)
        {
            return new ConvexShape
            {
                type = ConvexShapeType.Capsule,
                start = start,
                end = end,
                radius = radius,
                center = (start + end) * 0.5f
            };
        }

        public static ConvexShape Cylinder(
            Vector3 start,
            Vector3 end,
            float radius)
        {
            return new ConvexShape
            {
                type = ConvexShapeType.Cylinder,
                start = start,
                end = end,
                radius = radius,
                center = (start + end) * 0.5f
            };
        }

        public static ConvexShape Box(
            Vector3 center,
            Vector3 size,
            Vector3[] axes)
        {
            return new ConvexShape
            {
                type = ConvexShapeType.Box,
                center = center,
                size = size,
                axes = axes
            };
        }

        public Vector3 GetSupport(Vector3 direction)
        {
            switch (type)
            {
                case ConvexShapeType.Capsule:
                    return GetCapsuleSupport(direction);

                case ConvexShapeType.Cylinder:
                    return GetCylinderSupport(direction);

                case ConvexShapeType.Box:
                    return GetBoxSupport(direction);
            }

            return center;
        }

        private Vector3 GetCapsuleSupport(
            Vector3 direction)
        {
            Vector3 point =
                Vector3.Dot(start, direction) >
                Vector3.Dot(end, direction)
                    ? start
                    : end;

            float sqrMagnitude =
                direction.sqrMagnitude;

            if (sqrMagnitude > 1E-10f)
            {
                point +=
                    direction /
                    Mathf.Sqrt(sqrMagnitude) *
                    radius;
            }

            return point;
        }

        private Vector3 GetCylinderSupport(
            Vector3 direction)
        {
            Vector3 axis = end - start;

            float axisSqrMagnitude =
                axis.sqrMagnitude;

            if (axisSqrMagnitude < 1E-10f)
            {
                float directionSqrMagnitude =
                    direction.sqrMagnitude;

                if (directionSqrMagnitude > 1E-10f)
                {
                    return center +
                           direction /
                           Mathf.Sqrt(
                               directionSqrMagnitude) *
                           radius;
                }

                return center;
            }

            Vector3 axisNormal =
                axis /
                Mathf.Sqrt(axisSqrMagnitude);

            Vector3 point =
                Vector3.Dot(direction, axisNormal) >= 0f
                    ? end
                    : start;

            Vector3 radialDirection =
                direction -
                axisNormal *
                Vector3.Dot(
                    direction,
                    axisNormal);

            float radialSqrMagnitude =
                radialDirection.sqrMagnitude;

            if (radialSqrMagnitude > 1E-10f)
            {
                point +=
                    radialDirection /
                    Mathf.Sqrt(
                        radialSqrMagnitude) *
                    radius;
            }

            return point;
        }

        private Vector3 GetBoxSupport(
            Vector3 direction)
        {
            Vector3 extents =
                new(
                    Mathf.Abs(size.x),
                    Mathf.Abs(size.y),
                    Mathf.Abs(size.z));

            extents *= 0.5f;

            Vector3 result = center;

            for (int i = 0; i < 3; i++)
            {
                float sign =
                    Vector3.Dot(
                        direction,
                        axes[i]) >= 0f
                        ? 1f
                        : -1f;

                result +=
                    axes[i] *
                    extents[i] *
                    sign;
            }

            return result;
        }
    }

    private struct GjkSimplex
    {
        public Vector3 a;
        public Vector3 b;
        public Vector3 c;
        public Vector3 d;

        public int count;

        public void Add(Vector3 point)
        {
            d = c;
            c = b;
            b = a;
            a = point;

            if (count < 4)
            {
                count++;
            }
        }
    }
}