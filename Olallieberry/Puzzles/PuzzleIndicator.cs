using UnityEngine;

namespace Olallieberry.Puzzles;

[ExecuteAlways]
[RequireComponent(typeof(Renderer))]
public class PuzzleIndicator : MonoBehaviour
{
	private static readonly int PropId_Albedo = Shader.PropertyToID("_Color");
	private static readonly int PropId_EmissionColor = Shader.PropertyToID("_EmissionColor");
	
	[SerializeField] private bool isLit = false;
	[SerializeField] private float activationSpeed = 1;
	[SerializeField] private float deactivationSpeed = 1;
	[SerializeField] private Color albedo;
	[SerializeField] private Color emissionColor;

	private Renderer renderer = null;
	private MaterialPropertyBlock matProps = null;

	private float activationAmount = 0;

	private void OnEnable()
	{
		if (renderer == null)
		{
			renderer = gameObject.GetRequiredComponent<Renderer>();
		}
		
		matProps = new MaterialPropertyBlock();
	}

	private void Update()
	{
		if (renderer is null || matProps is null) return;
		
		if (isLit && activationAmount < 1)
		{
			activationAmount = Mathf.Clamp01(activationAmount + activationSpeed*Time.deltaTime);
		} else if (!isLit && 0 < activationAmount)
		{
			activationAmount = Mathf.Clamp01(activationAmount - deactivationSpeed*Time.deltaTime);
		}
		
		var emission = Color.Lerp(Color.black, emissionColor, activationAmount);
		matProps.SetColor(PropId_Albedo, albedo);
		matProps.SetColor(PropId_EmissionColor, emission);
		renderer.SetPropertyBlock(matProps);
	}

	public void Activate()
	{
		isLit = true;
	}

	public void Deactivate()
	{
		isLit = false;
	}
}