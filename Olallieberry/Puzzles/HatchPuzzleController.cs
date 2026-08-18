using System;
using DitzyExtensions.Collection;
using UnityEngine;

namespace Olallieberry.Puzzles;

public class HatchPuzzleController : MonoBehaviour
{
	[SerializeField] private PuzzleIndicator[] puzzleIndicators;
	[SerializeField] private HatchPuzzleA puzzleA;
	[SerializeField] private AbstractDoorController hatchDoor;

	private void OnEnable()
	{
		puzzleA.OnPuzzleSolved += OnPuzzleSolved;
	}

	private void OnDisable()
	{
		puzzleA.OnPuzzleSolved -= OnPuzzleSolved;
	}

	private void OnPuzzleSolved(HatchPuzzleA puzzle)
	{
		puzzleIndicators?.ForEach(i => i.Activate());

		if (hatchDoor is null) return;

		if (hatchDoor.IsOpen)
			hatchDoor.Close();
		else
			hatchDoor.Open();
	}
}