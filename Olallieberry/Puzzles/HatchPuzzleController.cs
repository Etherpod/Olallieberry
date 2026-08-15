using System;
using DitzyExtensions.Collection;
using UnityEngine;

namespace Olallieberry.Puzzles;

public class HatchPuzzleController : MonoBehaviour
{
	[SerializeField] private PuzzleIndicator[] puzzleIndicators;
	[SerializeField] private HatchPuzzleA puzzleA;
	[SerializeField] private AbstractDoor hatchDoor;

	private void OnEnable()
	{
		puzzleA.OnPuzzleSolved += OnPuzzleSolved;
	}

	private void OnDisable()
	{
		puzzleA.OnPuzzleSolved -= OnPuzzleSolved;
	}

	private void OnPuzzleSolved()
	{
		puzzleIndicators?.ForEach(i => i.Activate());

		if (hatchDoor is null) return;

		if (hatchDoor.IsOpen())
			hatchDoor.Close();
		else
			hatchDoor.Open();
	}
}