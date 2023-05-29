public enum LevelResult
{
	Win,
	Lose
}

public enum LevelType
{
	normal,
	bonus
}

public enum ADSType
{
	interstitial,
	rewarded
}

public enum PlacementType
{
	none,
	int_on_level_complite,
	rwd_x2_money,
	rwd_x5_money,
	rwd_add_money,
	rwd_continue_after_broke,
	rwd_get_new_hero,
	rwd_get_new_skin,
	rwd_bonus_level,
	rwd_player_upgrade,
	rwd_add_3_keys,
	rwd_skip_level
}

public enum ADSResult
{
	success,
	not_available,
	too_early,
	cancel,
	watched,
	clicked,
	start
}

public enum RateUsReason
{
	new_player,
	after_pause_retry
}

public enum RateUsResultType
{
	Close = 0,
	NoRate = 1,
	Rate = 5
}

public enum UIPanelType
{
	None,
	LevelResult,
	Promo,
	PU_RateUs,
	Store,
	Main,
	Game
}

public enum ParticleType
{
	Coin,
	Explosion,
	PlacedBase,
	UnitDestroy,
	BaseDestroted,
	CellPaint,
	MargePuff
}

public enum StateType
{
	AZ,
	CA,
	CO,
	ID,
	KS,
	MT,
	NE,
	NM,
	NV,
	OK,
	OR,
	TX,
	UT,
	WA,
	WY
}

public enum GroupingType
{
	Union,
	EnemyGroup_01,
	EnemyGroup_02,
	EnemyGroup_03
}

public enum BaseType
{
	UnionBase,
	EnemyBase
}

public enum UnitType { 
	Easy,
	Hard,
	Tank,
	Nulk,
	Vinom,
	NulkBuster,
	None
}

public enum UnitState
{
	Wait,
	Move,
	Painting,
	Looking,
	Happy,
	Battle,
	BaseAttack
}

public enum CellState
{
	None,
	ReservedUnion,
	ReservedEnemy,
	BaseCell
}

public enum ControlType
{
	Neutral,
	Union,
	Enemys
}

public enum FightType
{
	ShortRange,
	LongRange
}

public enum EnviromentType
{
	Land,
	Water,
	Forest,
	Desert,
	City
}
