using System;
using DitzyExtensions.Collection;
using UnityEngine;

namespace Olallieberry.Puzzles;

public class HatchPuzzleA : MonoBehaviour
{
	public delegate void PuzzleSolvedEvent();
	public event PuzzleSolvedEvent OnPuzzleSolved;
	
	[SerializeField] private InteractReceiver[] buttons;

	private void OnEnable()
	{
		buttons.ForEach(b => b.OnPressInteract += ButtonPressed);
	}

	private void OnDisable()
	{
		buttons.ForEach(b => b.OnPressInteract -= ButtonPressed);
	}

	private void ButtonPressed()
	{
		OnPuzzleSolved?.Invoke();
	}
}