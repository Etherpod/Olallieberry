using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;

namespace Olallieberry;

[RequireComponent(typeof(NomaiTextLine))]
public class CircleTextLine : MonoBehaviour
{
	public NomaiTextLine line;

	public void Start()
	{
		line = this.GetRequiredComponent<NomaiTextLine>();
		enabled = false;
        Olallieberry.Instance.ModHelper.Events.Unity.FireInNUpdates(SetupCirclePoints, 2);
    }

	public void SetupCirclePoints()
	{
		if (line == null) return;
        line._points =
        [
            new Vector3(0f, 0.1775f, 0f),
            new Vector3(0.125f, 0.19f, 0f),
            new Vector3(0.25f, 0.2275f, 0f),
            new Vector3(0.375f, 0.29f, 0f),
            new Vector3(0.5f, 0.3925f, 0f),
            new Vector3(0.625f, 0.5575f, 0f),
            new Vector3(0.6875f, 0.6925f, 0f),
            new Vector3(0.73125f, 0.87f, 0f),
            new Vector3(0.74f, 0.995f, 0f),
            new Vector3(0.73125f, 1.12f, 0f),
            new Vector3(0.6875f, 1.2975f, 0f),
            new Vector3(0.625f, 1.435f, 0f),
            new Vector3(0.5f, 1.6f, 0f),
            new Vector3(0.375f, 1.7025f, 0f),
            new Vector3(0.25f, 1.765f, 0f),
            new Vector3(0.125f, 1.8025f, 0f),
            new Vector3(0f, 1.815f, 0f),
            new Vector3(-0.125f, 1.8025f, 0f),
            new Vector3(-0.25f, 1.765f, 0f),
            new Vector3(-0.375f, 1.7025f, 0f),
            new Vector3(-0.5f, 1.6f, 0f),
            new Vector3(-0.625f, 1.435f, 0f),
            new Vector3(-0.6875f, 1.2975f, 0f),
            new Vector3(-0.725f, 1.12f, 0f),
            new Vector3(-0.73375f, 0.995f, 0f),
            new Vector3(-0.725f, 0.87f, 0f),
            new Vector3(-0.6875f, 0.6925f, 0f),
            new Vector3(-0.625f, 0.5575f, 0f),
            new Vector3(-0.5f, 0.3925f, 0f),
            new Vector3(-0.375f, 0.29f, 0f),
            new Vector3(-0.25f, 0.2275f, 0f),
            new Vector3(-0.125f, 0.19f, 0f),
            new Vector3(0f, 0.1775f, 0f)
        ];
        line.CalculateLengthAndCenter();
    }
}