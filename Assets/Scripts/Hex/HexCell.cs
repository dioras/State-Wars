using System;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections.Generic;
using System.Linq;

public class HexCell : MonoBehaviour {

	[Header("Base")]
	[SerializeField] private DependencyContainer dependencyContainerSO = default;
	[SerializeField] private GroupingStorage groupingStorageSO = default;
	[SerializeField] private StateStorage stateStorageSO = default;
	[SerializeField] private PlayerStorage playerStorageSO = default;

	[SerializeField] private bool isEmpty = false;
	[SerializeField] private bool isEnviroment = default;
	[SerializeField] private bool isBoarder = false;
	[SerializeField] private MeshRenderer meshRenderer = default;
	[SerializeField] private List<Collider> cellColliders = new List<Collider>();
	[SerializeField] private Color defaultColor = default;
	[SerializeField] private CellState cellState = default;
	[SerializeField] private List<Border> borders = new List<Border>();
	private HexCell[] neighbors = new HexCell[6];

	public HexCoordinates coordinates = default;
	private List<HexDirection> hexDirections = new List<HexDirection>();
	
	private ControlType preCellControl = default;
	[SerializeField] private ControlType controlType = default;

	#region get/set
	public bool IsEnviroment
	{
		get => isEnviroment;
		set => isEnviroment = value;
	} 
	public MeshRenderer HexMeshRenderer => meshRenderer;
	public ControlType GetPrControl => preCellControl;
	public bool IsBoarder => isBoarder;
	public TurrelController ObjectInCell { get; set; }
	public ControlType ControlType {
		get
		{
			//UpdateBorders();
			return controlType;
		} 
		set {
			preCellControl = controlType;
			controlType = value;
			//UpdateBorders();
		} 
	}

	public CellState CellState { get => cellState; set {
			cellState = value;
		} 
	}
    public bool IsEmpty { get => isEmpty; set { 
			isEmpty = value;
			//meshRenderer.enabled = !isEmpty;
            if (isEmpty)
            {
                //meshRenderer.transform.DOMoveY(-15f, .5f);
				meshRenderer.transform.position = new Vector3(meshRenderer.transform.position.x,
																-15f,
																meshRenderer.transform.position.z);
			}
            else
            {
				meshRenderer.transform.position = new Vector3(meshRenderer.transform.position.x,
																0f,
																meshRenderer.transform.position.z);
			}
        } 
    }

	public int Elevation {
		get {
			return elevation;
		}
		set {
			if (elevation == value) {
				return;
			}
			elevation = value;
			RefreshPosition();
		}
	}

	public Vector3 Position {
		get {
			return transform.localPosition;
		}
	}

	public int Distance {
		get {
			return distance;
		}
		set {
			distance = value;
		}
	}

	public HexCell PathFrom { get; set; }

	public int SearchHeuristic { get; set; }

	public int SearchPriority {
		get {
			return distance + SearchHeuristic;
		}
	}
	public HexCell[] Neighbors => neighbors;
	#endregion
    public HexCell NextWithSamePriority { get; set; }

	int elevation = int.MinValue;
	int distance = default;
	private static readonly int DottedColor = Shader.PropertyToID("_Color");

	private void Awake()
	{
		hexDirections = Enum.GetValues(typeof(HexDirection)).Cast<HexDirection>().ToList();
		borders.ForEach((board) => board.Boarder.gameObject.SetActive(false));
	}

	private void OnTriggerEnter(Collider other)
    {
		if (other.tag.Equals(Tags.State))
		{
			IsEmpty = false;
		}
		else if (other.tag.Equals(Tags.Base) || other.tag.Equals(Tags.UnionBase))
        {
			if (IsEmpty == false)
			{
				var _base = other.GetComponent<BaseController>();
				if (other.tag.Equals(Tags.UnionBase))
				{
					ControlType = ControlType.Union;
					dependencyContainerSO.CountUnionHex++;
					dependencyContainerSO.UnionBase.AddSpawnCell(this);
					//cellState = CellState.BaseCell;
				}
				else
				{
					ControlType = ControlType.Enemys;
					dependencyContainerSO.CountEnemyHex++;
					dependencyContainerSO.EnemyBase.AddSpawnCell(this);
					//cellState = CellState.BaseCell;
				}
				EnableHighlight(_base.Color, .5f, false);
				//UpdateBorders();
			}
        }
		/*else if (other.tag.Equals(Tags.BlockState))
		{
			IsEmpty = true;
		}*/
    }

    public void UpdateBorders()
    {
	    DisableBoards();
	    if (controlType != ControlType.Neutral)
	    {
		    foreach (var dir in hexDirections)
		    {
			    var neighbor = GetNeighbor(dir);
			    if (neighbor != null && neighbor.IsEmpty == false && neighbor.controlType != controlType || (neighbor != null && ObjectInCell != null && neighbor.ObjectInCell != ObjectInCell))
			    {
				    var border = borders.Find((border) => border.HexDirection == dir);

				    Color color = default;
				    if (ObjectInCell == null)
				    {
					    color = controlType == ControlType.Union
						    ? groupingStorageSO.GetGrouping(stateStorageSO.Country
							    .GetState(playerStorageSO.ConcretePlayer.CurrecyStateType).Grouping).Color
						    : groupingStorageSO.GetGrouping(GroupingType.Union).Color;
				    }
				    else
				    {
					    color = groupingStorageSO.GetGrouping(ObjectInCell.GroupingType).ColorStickmans;
				    }

				    border.Boarder.gameObject.SetActive(true);
				    
				    border.Boarder.material.SetColor(DottedColor, color);
				    isBoarder = true;

				    //border.Boarder.material.DOKill();
				    //border.Boarder.material.DOOffset(border.Boarder.material.GetTextureOffset("_MainTex") + Vector2.right, "_MainTex", 2f).SetLoops(-1).SetEase(Ease.Flash);
			    }
		    }
	    }
    }

    private void Update()
    {
	    borders.ForEach((_border) =>
	    {
		    if (_border.Boarder.gameObject.activeSelf)
		    {
			    _border.Boarder.material.SetTextureOffset("_MainTex", dependencyContainerSO.BoarderTextureOffset);
		    }
	    });
    }


    public void DisableBoards()
    {
	    borders.ForEach((border) => border.Boarder.gameObject.SetActive(false));
	    isBoarder = false;
    }
    
	public void SetRenderer(bool _enable)
    {
		meshRenderer.enabled = _enable;
		cellColliders.ForEach((_collider) => _collider.enabled = _enable);
	}

    public HexCell GetNeighbor (HexDirection direction) {
		return neighbors[(int)direction];
	}

	public void SetNeighbor (HexDirection direction, HexCell cell) {
		neighbors[(int)direction] = cell;
		cell.neighbors[(int)direction.Opposite()] = this;
	}

	void RefreshPosition () {
		Vector3 position = transform.localPosition;
		position.y = elevation * HexMetrics.elevationStep;
		position.y += 0f;
		transform.localPosition = position;
	}

	public void DisableHighlight () {
		meshRenderer.materials[0].DOKill();
		meshRenderer.materials[2].DOKill();
		meshRenderer.materials[2].DOColor(defaultColor, .5f);
		meshRenderer.materials[0].DOColor(defaultColor, .5f);
	}

	public void EnableHighlight (Color color, float paintDuration, bool sphereEffect = true, bool updateBoards = true) {
		meshRenderer.materials[2].DOKill();
		meshRenderer.materials[0].DOKill();
		meshRenderer.materials[2].DOColor(color, paintDuration);
		meshRenderer.materials[0].DOColor(color, paintDuration);

		if (sphereEffect)
        {
			ParticleController.PlayParticleWitColorAction?.Invoke(transform.position, ParticleType.CellPaint, color);
        }

		if (updateBoards)
		{
			HexGrid.UpdateBoardersAction?.Invoke();
		}
	}

	public void SetCellControl(ControlType controlType, Color cellColor)
    {
		//EnableHighlight(cellColor);
		if (this.controlType == ControlType.Neutral)
        {
			if (controlType == ControlType.Union)
            {
				dependencyContainerSO.CountUnionHex++;
            }
			else
            {
				dependencyContainerSO.CountEnemyHex++;
			}
        }
		else 
        {
			if (this.controlType == ControlType.Union && controlType != this.controlType)
			{
				dependencyContainerSO.CountEnemyHex++;
				dependencyContainerSO.CountUnionHex--;
			}
			else if (this.controlType != ControlType.Union && controlType != this.controlType)
			{
				dependencyContainerSO.CountUnionHex++;
				dependencyContainerSO.CountEnemyHex--;
			}
		}
		this.ControlType = controlType;
		this.cellState = CellState.None;
    }
}

[Serializable]
public class Border
{
	[SerializeField] private HexDirection hexDirection = default;
	[SerializeField] private MeshRenderer boarderTransform = default;

	public HexDirection HexDirection => hexDirection;
	public MeshRenderer Boarder => boarderTransform;
}