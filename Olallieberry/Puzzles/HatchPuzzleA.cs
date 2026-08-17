using System;
using System.Collections.Generic;
using System.Linq;
using DitzyExtensions.Collection;
using Olallieberry.Utils;
using UnityEngine;

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

	[SerializeField] [ColorUsage(false, true)]
	private Color beadEmissiveColor;

	[SerializeField] [ColorUsage(false, true)]
	private Color poleEmissiveColor;

	[SerializeField] private Renderer[] beads;
	[SerializeField] private Renderer[] poles;
	[SerializeField] private InteractReceiver[] buttons;
	[Header("Debug")] [SerializeField] private bool startLit = false;

	private readonly Dictionary<int, SingleInteractionVolume.PressInteractEvent> buttonHandlers = new();

	private EmissionHandler[] emissionHandlers = [];

	private void OnEnable()
	{
		buttons.ForEach((b, i) =>
		{
			buttonHandlers[i] = () => ButtonPressed(i);
			b.OnPressInteract += buttonHandlers[i];
		});

		emissionHandlers = beads
			.Select((b, i) => new EmissionHandler(this, b, poles[i]))
			.ToArray();
	}

	private void OnDisable()
	{
		buttons.ForEach((b, i) => b.OnPressInteract -= buttonHandlers[i]);
		buttonHandlers.Clear();
	}

	private void ButtonPressed(int index)
	{
		OnPuzzleSolved?.Invoke(this);

		emissionHandlers[index].On();
	}

	private void Update()
	{
		emissionHandlers.ForEach(h => h.Update());
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