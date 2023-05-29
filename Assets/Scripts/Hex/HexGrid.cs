using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System;
using DG.Tweening;
using Random = System.Random;

public class HexGrid : MonoBehaviour {

	[Header("Base")]
	[SerializeField] private DependencyContainer dependencyContainerSO = default;

	[Header("Settings")]
	[SerializeField] private int cellCountX = 20;
	[SerializeField] private int cellCountZ = 20;

	[Header("Components")]
	[SerializeField] private Transform hexGridContainer = default;
	[SerializeField] private HexCell cellPrefab = default;

	[Header("Actions")]
	public static Action CreateEmptyMapAction = default;
	public static Action HideStateMapAction = default;
	public static Action UpdateBoardersAction = default;

	#region reg/set
	public int CellCountX => cellCountX;
	public int CellCountZ => cellCountZ;
	#endregion

	private HexCell[] cells = default;

	private HexCellPriorityQueue searchFrontier;

	public List<HexCoordinates> currencyPath = new List<HexCoordinates>();

	private void OnEnable()
    {
		CreateEmptyMapAction += CreateEmptyMap;
		HideStateMapAction += HideMap;
		UpdateBoardersAction += UpdateBoarders;
		StateGenerator.StateCreatedAction += ClearEmptyCells;
	}

	private void OnDisable()
    {
		CreateEmptyMapAction -= CreateEmptyMap;
		HideStateMapAction -= HideMap;
		UpdateBoardersAction -= UpdateBoarders;
		StateGenerator.StateCreatedAction -= ClearEmptyCells;
	}

	private void Awake()
	{
		dependencyContainerSO.HexGrid = this;

		DOTween.To(() => dependencyContainerSO.BoarderTextureOffset,
				x => dependencyContainerSO.BoarderTextureOffset = x,
				dependencyContainerSO.BoarderTextureOffset + Vector3.right, 2f)
			.SetLoops(-1)
			.SetEase(Ease.Flash);
	}

	private void CreateEmptyMap()
    {
		CreateMap(cellCountX, cellCountZ);
	}

	private void HideMap() { 
		for (int i = 0; i < cells.Length; i++)
        {
			cells[i].DisableHighlight();
			cells[i].IsEmpty = true;
        }
	}

	private void ClearEmptyCells()
    {
		for(int i = 0; i < cells.Length; i++)
        {
            if (cells[i].IsEmpty && cells[i].IsEnviroment.Equals(false))
            {
				cells[i].SetRenderer(false);
            }
        }
    }

	private bool CreateMap (int x, int z) {
		if (
			x <= 0 || x % HexMetrics.chunkSizeX != 0 ||
			z <= 0 || z % HexMetrics.chunkSizeZ != 0
		) {
			Debug.LogError("Unsupported map size.");
			return false;
		}

		//if (chunks != null) {
		//	for (int i = 0; i < chunks.Length; i++) {
		//		Destroy(chunks[i].gameObject);
		//	}
		//}

		cellCountX = x;
		cellCountZ = z;
		//chunkCountX = cellCountX / HexMetrics.chunkSizeX;
		//chunkCountZ = cellCountZ / HexMetrics.chunkSizeZ;
		CreateCells();
		return true;
	}

	private void CreateCells () {
		if (cells == null)
		{
			cells = new HexCell[cellCountZ * cellCountX];
		}

		for (int z = 0, i = 0; z < cellCountZ; z++) {
			for (int x = 0; x < cellCountX; x++) {
				CreateCell(x, z, i++);
			}
		}
	}

	public HexCell GetCell (Vector3 position) {
		position = transform.InverseTransformPoint(position);
		HexCoordinates coordinates = HexCoordinates.FromPosition(position);
		int index =
			coordinates.X + coordinates.Z * cellCountX + coordinates.Z / 2;
		return cells[index];
	}

	public HexCell GetCell (HexCoordinates coordinates) {
		int z = coordinates.Z;
		if (z < 0 || z >= cellCountZ) {
			return null;
		}
		int x = coordinates.X + z / 2;
		if (x < 0 || x >= cellCountX) {
			return null;
		}
		return cells[x + z * cellCountX];
	}

	public HexCell GetRandomBoardCell(ControlType controlType)
	{
		List<HexCell> boardCells = new List<HexCell>();
		for (int i = 0; i < cells.Length; i++)
		{
			if (cells[i].IsEmpty || cells[i].ObjectInCell != null) continue;
			if (cells[i].IsBoarder && cells[i].ControlType == controlType)
			{
				if (boardCells.Contains(cells[i]) == false)
				{
					boardCells.Add(cells[i]);
				}
			}
		}

		var randomCell = boardCells[UnityEngine.Random.Range(0, boardCells.Count - 1)];
		return randomCell;
	}

	private void CreateCell (int x, int z, int i) {
		Vector3 position;
		position.x = (x + z * 0.5f - z / 2) * (HexMetrics.innerRadius * 2f);
		position.y = 0f;
		position.z = z * (HexMetrics.outerRadius * 1.5f);

		HexCell cell = null;
		if (cells[i] == null)
        {
			cell = cells[i] = Instantiate<HexCell>(cellPrefab, hexGridContainer);
		}
		else
        {
			cell = cells[i];
		}
		cell.IsEmpty = true;
		cell.IsEnviroment = false;
		cell.ControlType = ControlType.Neutral;
		cell.CellState = CellState.None;
		cell.SetRenderer(true);
		cell.transform.localPosition = position;
		cell.coordinates = HexCoordinates.FromOffsetCoordinates(x, z);
		cell.DisableHighlight();

        if (x > 0)
        {
            cell.SetNeighbor(HexDirection.W, cells[i - 1]);
        }
        if (z > 0)
        {
            if ((z & 1) == 0)
            {
                cell.SetNeighbor(HexDirection.SE, cells[i - cellCountX]);
                if (x > 0)
                {
                    cell.SetNeighbor(HexDirection.SW, cells[i - cellCountX - 1]);
                }
            }
            else
            {
                cell.SetNeighbor(HexDirection.SW, cells[i - cellCountX]);
                if (x < cellCountX - 1)
                {
                    cell.SetNeighbor(HexDirection.SE, cells[i - cellCountX + 1]);
                }
            }
        }

		cell.Elevation = 0;
	}

	public List<HexCoordinates> Search(HexCell fromCell, HexCell toCell, ControlType enemyControl = ControlType.Neutral) {
		currencyPath.Clear();
		currencyPath = new List<HexCoordinates>();
		if (toCell == null)
        {
			return currencyPath;
        }
		currencyPath.Add(toCell.coordinates);

		/*if (enemyControl != ControlType.Neutral && IsNeighborEnemys(fromCell, enemyControl))
		{
			currencyPath.Clear();
			return currencyPath;
		}*/

		int _maxIter = 0;

		if (searchFrontier == null) {
			searchFrontier = new HexCellPriorityQueue();
		}
		else {
			searchFrontier.Clear();
		}

		for (int i = 0; i < cells.Length; i++) {
			cells[i].Distance = int.MaxValue;
		}
		fromCell.Distance = 0;
		searchFrontier.Enqueue(fromCell);
		while (searchFrontier.Count > 0) {
			HexCell current = searchFrontier.Dequeue();

			if (current == toCell) {
				current = current.PathFrom;
				while (current != fromCell) 
				{
					if (current != null)
					{
						if (currencyPath.Contains(current.coordinates) == false)
						{
							currencyPath.Add(current.coordinates);
						}
						current = current.PathFrom;
					}
					else
                    {
						currencyPath.Clear();
						return currencyPath;
					}
					_maxIter++;
					if (_maxIter > 500)
					{
						currencyPath.Clear();
						return currencyPath;
					}

					/*if (toCell.ControlType == (enemyControl != ControlType.Union ? ControlType.Union : ControlType.Enemys))
                    {
						currencyPath.Clear();
						return currencyPath;
                    }*/
				}
				break;
			}

			int distance = current.Distance;
			bool _finded = false;
			for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++) {
				HexCell neighbor = current.GetNeighbor(d);
				if (neighbor == null) {
					continue;
				}
				else if (neighbor.IsEmpty) {
					continue;
				}
				/*else if (enemyControl != ControlType.Neutral)
				{
					if (neighbor != toCell && (neighbor.ControlType == enemyControl || neighbor.ControlType == ControlType.Neutral))
					{
						continue;
					}
				}
				else if (neighbor.CellState != CellState.None)
				{
					continue;
				}*/

                if (neighbor.Distance == int.MaxValue) {
					neighbor.Distance = distance;
					neighbor.PathFrom = current;
					neighbor.SearchHeuristic =
						neighbor.coordinates.DistanceTo(toCell.coordinates);
					searchFrontier.Enqueue(neighbor);
					_finded = true;
				}
				else if (distance < neighbor.Distance) {
					int oldPriority = neighbor.SearchPriority;
					neighbor.Distance = distance;
					neighbor.PathFrom = current;
					searchFrontier.Change(neighbor, oldPriority);

					_finded = true;
				}
			}

			//if (_finded == false)
            //{
			//	currencyPath.Clear();
			//	return currencyPath;
			//}
		}
		return currencyPath;
	}

	public List<HexCoordinates> HardSearch(HexCell fromCell, HexCell toCell)
	{
		currencyPath.Clear();
		currencyPath = new List<HexCoordinates>();
		if (toCell == null)
		{
			return currencyPath;
		}
		currencyPath.Add(toCell.coordinates);

		int _maxIter = 0;

		if (searchFrontier == null)
		{
			searchFrontier = new HexCellPriorityQueue();
		}
		else
		{
			searchFrontier.Clear();
		}

		for (int i = 0; i < cells.Length; i++)
		{
			cells[i].Distance = int.MaxValue;
		}
		fromCell.Distance = 0;
		searchFrontier.Enqueue(fromCell);
		while (searchFrontier.Count > 0)
		{
			HexCell current = searchFrontier.Dequeue();

			if (current == toCell)
			{
				current = current.PathFrom;
				while (current != fromCell)
				{
					if (current != null)
					{
						if (currencyPath.Contains(current.coordinates) == false)
						{
							currencyPath.Add(current.coordinates);
						}
						current = current.PathFrom;
					}
					else
					{
						currencyPath.Clear();
						return currencyPath;
					}
					_maxIter++;
					if (_maxIter > 300)
					{
						print("OPS :) it`s very bad D:");
						currencyPath.Clear();
						return currencyPath;
					}
				}
				break;
			}

			int distance = current.Distance;
			bool _finded = false;
			for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
			{
				HexCell neighbor = current.GetNeighbor(d);
				if (neighbor == null)
				{
					continue;
				}
				else if (neighbor.IsEmpty)
				{
					continue;
				}

				if (neighbor.Distance == int.MaxValue)
				{
					neighbor.Distance = distance;
					neighbor.PathFrom = current;
					neighbor.SearchHeuristic =
						neighbor.coordinates.DistanceTo(toCell.coordinates);
					searchFrontier.Enqueue(neighbor);
					_finded = true;
				}
				else if (distance < neighbor.Distance)
				{
					int oldPriority = neighbor.SearchPriority;
					neighbor.Distance = distance;
					neighbor.PathFrom = current;
					searchFrontier.Change(neighbor, oldPriority);

					_finded = true;
				}
			}

			if (_finded == false)
			{
				currencyPath.Clear();
				return currencyPath;
			}
		}
		return currencyPath;
	}

	public int GetAvialableHex()
    {
		int _result = 0;
		for (int i = 0; i < cells.Length; i++)
        {
			if (cells[i].IsEmpty == false /*&& cells[i].CellState != CellState.BaseCell*/)
            {
				_result++;
            }
        }
		return _result;
    }

	private bool IsNeighborEnemys(HexCell fromCell, ControlType enemyControl)
    {
		for (int i = 0; i < fromCell.Neighbors.Length; i++)
		{
			if (fromCell.Neighbors[i] != null)
			{
				if (fromCell.Neighbors[i].ControlType != enemyControl)
				{
					return false;
				}
			}
		}
		return true;
	}

	public HexCell FindNearNeutralHex(Vector3 fromCoordinate, ControlType unionCell, CellState reservUnion, HexCell currencyCell)
    {
		float _nearDistance = -1f;
		HexCell _nearCell = null;
		for (int i = 0; i < cells.Length; i++)
        {
			if (cells[i].IsEmpty) continue;
			//if (cells[i] == currencyCell) continue;
			//if (unionCell == ControlType.Enemys && cells[i].CellState == reservUnion) continue;

			if (cells[i].ControlType != unionCell && cells[i].CellState != reservUnion)
			{
				float _distance = Vector3.Distance(fromCoordinate, cells[i].transform.position);
				if (_distance < _nearDistance || _nearDistance < 0f)
				{
					_nearDistance = _distance;
					_nearCell = cells[i];
				}
			}
        }

		return _nearCell; 
    }

	public HexCell FindNearUnionHex(Vector3 fromCoordinate, ControlType unionCell)
    {
		float _nearDistance = -1f;
		HexCell _nearCell = cells[0];
		for (int i = 0; i < cells.Length; i++)
		{
			if (cells[i].IsEmpty || cells[i].CellState == CellState.BaseCell) continue;

			if (cells[i].ControlType == unionCell)
			{
				float _distance = Vector3.Distance(fromCoordinate, cells[i].transform.position);
				if (_distance < _nearDistance || _nearDistance < 0f)
				{
					_nearDistance = _distance;
					_nearCell = cells[i];
				}
			}
		}
		return _nearCell;
	}

	public HexCell FindCurrencyCell(Vector3 fromPosition)
    {
		float _nearDistance = -1f;
		HexCell _nearCell = cells[0];
		for (int i = 0; i < cells.Length; i++)
		{
			if (cells[i].IsEmpty) continue;
			float _distance = Vector3.Distance(fromPosition, cells[i].transform.position);
			if (_distance < _nearDistance || _nearDistance < 0f)
			{
				_nearDistance = _distance;
				_nearCell = cells[i];
			}
		}
		return _nearCell;
	}

	public void SetAllHexCellColor(Color _color, ControlType controlType)
    {
		for (int i= 0; i < cells.Length; i++)
		{
			if (cells[i].IsEmpty) continue;
			cells[i].ControlType = controlType;
			if (cells[i].IsEnviroment.Equals(false))
			{
				cells[i].EnableHighlight(_color, .2f, false, false);
			}
			cells[i].DisableBoards();
        }
    }

	private void UpdateBoarders()
	{
		for (int i= 0; i < cells.Length; i++)
		{
			if (cells[i].IsEmpty == false && cells[i].ControlType != ControlType.Neutral)
			{
				cells[i].UpdateBorders();
			}
		}
	}
}