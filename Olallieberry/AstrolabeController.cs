using System;
using DitzyExtensions.Collection;
using UnityEngine;

namespace Olallieberry;

public class AstrolabeController : MonoBehaviour
{
	[SerializeField] private Transform[] rings;
	[SerializeField] private float[] ringTilts;
	[SerializeField] private float[] ringSpeeds;

	private void OnEnable()
	{
		rings?.ForEach((r, i) =>
		{
			r.Rotate(Vector3.up, ringTilts[i], Space.Self);
		});
	}

	private void FixedUpdate()
	{
		// if (OWTime.IsPaused()) return;
		
		rings?.ForEach((r, i) =>
		{
			r.Rotate(Vector3.right, Time.fixedDeltaTime * 360 / ringSpeeds[i], Space.Self);
		});
	}
}
