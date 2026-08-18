using System;
using System.Collections.Generic;
using System.Linq;
using DitzyExtensions.Collection;
using Olallieberry.Utils;
using UnityEngine;
using TimeZone = Olallieberry.TimeZones.TimeZone;

namespace Olallieberry.Puzzles;

[ExecuteAlways]
public class HatchPuzzleA : MonoBehaviour
{
	public delegate void PuzzleSolvedEvent(HatchPuzzleA puzzle);

	public event PuzzleSolvedEvent OnPuzzleSolved;

	[SerializeField] [ColorUsage(false, false)]
	private Color beadAlbedo;

	[SerializeField] [ColorUsage(false, false)]
	private Color poleAlbedo;

	[SerializeField] [ColorUsage(false, false)]
	private Color buttonAlbedo;

	[SerializeField] [ColorUsage(false, true)]
	private Color beadEmissiveColor;

	[SerializeField] [ColorUsage(false, true)]
	private Color poleEmissiveColor;
	
	[SerializeField] [ColorUsage(false, true)]
	private Color beadSuccessEmissiveColor;

	[SerializeField] [ColorUsage(false, true)]
	private Color buttonSuccessEmissiveColor;

	[SerializeField] private TimeZone[] timezones;
	[SerializeField] private Renderer[] beads;
	[SerializeField] private Renderer[] poles;
	[SerializeField] private Transform[] targets;
	[SerializeField] private InteractReceiver[] buttons;
	[SerializeField] private Renderer console;
	[Header("Debug")] [SerializeField] private bool startLit = false;

	private readonly Dictionary<int, SingleInteractionVolume.PressInteractEvent> buttonHandlers = new();

	private EmissionHandler[] mainEmissionHandlers = [];
	private EmissiveMaterialHandler[] successEmissionHandlers = [];
	private EmissiveMaterialHandler[] buttonEmissionHandlers = [];
	private float[] buttonRepromptTimes = [];
	private bool[] solvedPoles = [];

	private void OnEnable()
	{
		buttons.ForEach((b, i) =>
		{
			buttonHandlers[i] = () => ButtonPressed(i);
			b.OnPressInteract += buttonHandlers[i];
			b.SetPromptText("HATCH_PUZZLE_PROMPT");
		});
		
		timezones.ForEach(tz => tz.OnZoneDeactivated += OnZoneDeactivated);

		mainEmissionHandlers = beads
			.Select((b, i) => new EmissionHandler(this, b, poles[i]))
			.ToArray();

		successEmissionHandlers = beads
			.Select(b => new EmissiveMaterialHandler(b, beadAlbedo, beadSuccessEmissiveColor, 1))
			.ToArray();
		
		buttonEmissionHandlers = beads
			.Select((_, i) => new EmissiveMaterialHandler(
				console,
				buttonAlbedo,
				buttonSuccessEmissiveColor,
				((2*i) % 3) + 1) // i exported the button emissive materials in the wrong order :3. this fixes that :P
			)
			.ToArray();
		
		buttonRepromptTimes = buttons.Select(_ => -1f).ToArray();
		solvedPoles = poles.Select(_ => false).ToArray();
	}

	private void OnDisable()
	{
		timezones.ForEach(tz => tz.OnZoneDeactivated -= OnZoneDeactivated);
		buttons.ForEach((b, i) => b.OnPressInteract -= buttonHandlers[i]);
		buttonHandlers.Clear();
	}

	private void ButtonPressed(int i)
	{
		var tz = timezones[i];
		
		if (tz.IsActive)
		{
			tz.Deactivate();
			mainEmissionHandlers[i].Off();
		}
		else
		{
			tz.Activate();
			mainEmissionHandlers[i].On();
		}

		buttonRepromptTimes[i] = Time.time + .2f;
	}

	private void OnZoneDeactivated(TimeZone tz)
	{
		var i = timezones.IndexOfReference(tz);
		
		if (IsPoleSolved(i))
		{
			successEmissionHandlers[i].On();
			buttonEmissionHandlers[i].On();
			solvedPoles[i] = true;

			if (solvedPoles.All(p => p))
			{
				OnPuzzleSolved?.Invoke(this);
			}
		}
		else
		{
			mainEmissionHandlers[i].Off();
			buttonEmissionHandlers[i].Off();
			solvedPoles[i] = false;
		}
	}

	private bool IsPoleSolved(int i)
	{
		var beadPos = beads[i].transform.position;
		var targetPos = targets[i].position;
		var range = .052f;
		return (beadPos - targetPos).sqrMagnitude < range*range;
	}

	private void Update()
	{
		beads.ForEach((_, i) =>
		{
			if (solvedPoles[i] && !IsPoleSolved(i))
			{
				solvedPoles[i] = false;
				successEmissionHandlers[i].Off();
				buttonEmissionHandlers[i].Off();
				
				if (timezones[i].IsActive) mainEmissionHandlers[i].On();
			}

			mainEmissionHandlers[i].Update();
			if (solvedPoles[i] || successEmissionHandlers[i].IsChanging) successEmissionHandlers[i].Update();
			
			buttonEmissionHandlers[i].Update();
		});
		mainEmissionHandlers.ForEach(h => h.Update());
		
		buttonRepromptTimes
			.ForEach((t, i) =>
			{
				if (t < 0 || Time.time < t) return;
				
				buttons[i].ResetInteraction();
				buttons[i].UpdatePromptVisibility();

				buttonRepromptTimes[i] = -1f;
			});
	}

	private class EmissionHandler
	{
		public readonly EmissiveMaterialHandler beadHandler;
		public readonly EmissiveMaterialHandler poleHandler;

		public EmissionHandler(HatchPuzzleA puzzleA, Renderer bead, Renderer pole)
		{
			beadHandler = new EmissiveMaterialHandler(
				bead,
				puzzleA.beadAlbedo,
				puzzleA.beadEmissiveColor,
				1,
				startOn: puzzleA.startLit
			);
			poleHandler = new EmissiveMaterialHandler(
				pole,
				puzzleA.poleAlbedo,
				puzzleA.poleEmissiveColor,
				1,
				startOn: puzzleA.startLit
			);
		}

		public void On()
		{
			beadHandler.On();
			poleHandler.On();
		}

		public void Off()
		{
			beadHandler.Off();
			poleHandler.Off();
		}

		public void Update()
		{
			beadHandler.Update();
			poleHandler.Update();
		}
	}
}