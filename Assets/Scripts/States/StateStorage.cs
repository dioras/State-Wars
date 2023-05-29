using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/StateStorage", fileName = "StateStorage")]
public class StateStorage : ScriptableObject
{
    [SerializeField] private string saveString = "_countrySave";
    [SerializeField] private Country country = default;

    [Header("State settings")]
    [SerializeField] private List<StateSettings> stateSettings = new List<StateSettings>();

    public Country Country => country;

    public StateSettings GetStateSettings(StateType stateType)
    {
        return stateSettings.Find((_state) => _state.StateType == stateType);
    }

    public StateType GetNextState()
    {
        var _nextState = country.States.Find((_state) => _state.Grouping != GroupingType.Union);
        if (_nextState == null)
        {
            var _unionStates = country.States.FindAll((_state) => _state.Grouping == GroupingType.Union);
            _nextState = _unionStates[UnityEngine.Random.Range(0, _unionStates.Count - 1)];
            _nextState.Grouping = country.GetRandomGrouping();
        }
        return _nextState.StateType;
    }

    public void Save()
    {
        PlayerPrefs.SetString(saveString, JsonUtility.ToJson(country));
    }

    public void Load()
    {
        var countryString = PlayerPrefs.GetString(saveString, "");
        if (PlayerPrefs.HasKey(saveString) && countryString != "")
        {
            country = JsonUtility.FromJson<Country>(countryString);
        }
        else
        {
            country = new Country();
        }

        PlayerMapController.ShowPlayerMapAction?.Invoke(true);
    }
}

[Serializable]
public class Country
{
    [SerializeField] private List<State> states = new List<State>();

    public List<State> States => states;

    public Country()
    {
        ConfigStates();
    }

    public GroupingType GetRandomGrouping()
    {
        var _grouping = Enum.GetValues(typeof(GroupingType)).Cast<GroupingType>().ToList();
        _grouping.Remove(GroupingType.Union);
        return _grouping[UnityEngine.Random.Range(0, _grouping.Count)];
    }

    public State GetState(StateType stateType)
    {
        return states.Find((_state) => _state.StateType == stateType);
    }

    public int GetCountStateControlled(GroupingType groupingType)
    {
        return states.FindAll((_state) => _state.Grouping == groupingType).Count;
    }

    public void ConfigStates()
    {
        states = new List<State>();
        states.Add(new State(StateType.WA, GroupingType.EnemyGroup_01));
        states.Add(new State(StateType.OR, GroupingType.EnemyGroup_01));
        states.Add(new State(StateType.ID, GroupingType.EnemyGroup_02));
        states.Add(new State(StateType.MT, GroupingType.EnemyGroup_03));
        states.Add(new State(StateType.NE, GroupingType.EnemyGroup_03));
        states.Add(new State(StateType.WY, GroupingType.EnemyGroup_02));
        states.Add(new State(StateType.UT, GroupingType.EnemyGroup_02));
        states.Add(new State(StateType.AZ, GroupingType.EnemyGroup_03));
        states.Add(new State(StateType.CA, GroupingType.EnemyGroup_01));
        states.Add(new State(StateType.NV, GroupingType.EnemyGroup_01));
        states.Add(new State(StateType.CO, GroupingType.EnemyGroup_02));
        states.Add(new State(StateType.KS, GroupingType.EnemyGroup_02));
        states.Add(new State(StateType.NM, GroupingType.EnemyGroup_03));
        states.Add(new State(StateType.OK, GroupingType.EnemyGroup_01));
        states.Add(new State(StateType.TX, GroupingType.EnemyGroup_02));
    }
}

[Serializable]
public class State
{
    [SerializeField] private StateType stateType = default;
    [SerializeField] private GroupingType grouping = default;

    public StateType StateType => stateType;
    public GroupingType Grouping { get => grouping; set => grouping = value; }

    public State(StateType stateType, GroupingType groupingType)
    {
        this.stateType = stateType;
        this.grouping = groupingType;
    }
}

[Serializable]
public class StateSettings
{
    [SerializeField] private StateType stateType = default;
    [SerializeField] private StateLevelObject statePrefab = default;
    [SerializeField] private GameObject stateSight = default;
    [SerializeField] private Vector3 statePosition = Vector3.zero;
    [SerializeField] private Vector3 stateScale = Vector3.one;
    [SerializeField] private Vector3 eulerRotation = Vector3.zero;

    public StateLevelObject StatePrefab => statePrefab;
    public GameObject StateSight => stateSight;
    public Vector3 StatePosition => statePosition;
    public Vector3 ScaleScale => stateScale;
    public Vector3 EulerRotation => eulerRotation;
    public StateType StateType => stateType;
}
