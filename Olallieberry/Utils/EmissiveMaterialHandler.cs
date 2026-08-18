using UnityEngine;

namespace Olallieberry.Utils;

public class EmissiveMaterialHandler
{
	private static readonly int PropId_Albedo = Shader.PropertyToID("_Color");
	private static readonly int PropId_EmissionColor = Shader.PropertyToID("_EmissionColor");

	private readonly Renderer renderer;
	private readonly Color albedo;
	private readonly Color emissionColor;
	private readonly int materialIndex;
	private readonly float onSpeed;
	private readonly float offSpeed;
	
	private readonly MaterialPropertyBlock propertyBlock;

	private bool enabled = false;
	private bool turningOn = true;
	private float emissionFactor = 0;

	public float EmissionFactor => emissionFactor;
	public bool IsOn => !enabled && turningOn;
	public bool IsOff => !enabled && !turningOn;
	public bool IsChanging => enabled;
	public bool IsTurningOn => turningOn;
	public bool IsTurningOff => !turningOn;

	public EmissiveMaterialHandler(
		Renderer renderer,
		Color albedo,
		Color emissionColor,
		int materialIndex = 0,
		bool startOn = false,
		float onSpeed = 1f,
		float offSpeed = 1f
	)
	{
		this.renderer = renderer;
		this.albedo = albedo;
		this.emissionColor = emissionColor;
		this.materialIndex = materialIndex;
		this.onSpeed = onSpeed;
		this.offSpeed = offSpeed;
		
		emissionFactor = startOn ? 1f : 0f;

		propertyBlock = new MaterialPropertyBlock();
		ApplyProperties();
	}

	public void On(bool immediately = false)
	{
		turningOn = true;
		enabled = !immediately;
		if (immediately) emissionFactor = 1f;
	}

	public void Off(bool immediately = false)
	{
		turningOn = false;
		enabled = !immediately;
		if (immediately) emissionFactor = 1f;
	}

	public void Update()
	{
		if (!enabled) return;

		emissionFactor += Time.deltaTime * (turningOn ? onSpeed : -offSpeed);
		if (turningOn && 1f <= emissionFactor || !turningOn && emissionFactor <= 0f) enabled = false;
		emissionFactor = Mathf.Clamp(emissionFactor, 0f, 1f);

		ApplyProperties();
	}

	private void ApplyProperties()
	{
		var emission = Color.Lerp(Color.black, emissionColor, emissionFactor);

		propertyBlock.SetColor(PropId_Albedo, albedo);
		propertyBlock.SetColor(PropId_EmissionColor, emission);

		renderer.SetPropertyBlock(propertyBlock, materialIndex);
	}
}