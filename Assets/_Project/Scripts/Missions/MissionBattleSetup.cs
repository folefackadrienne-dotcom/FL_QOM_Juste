using System.Collections.Generic;
using System.Linq;
using KingdomOfGod.Battle;
using KingdomOfGod.Core;
using UnityEngine;

namespace KingdomOfGod.Missions
{
    /// <summary>
    /// Bridges MissionManager and a Battle scene: reads GameManager.Instance.Missions.ActiveMission
    /// (set by MissionManager.StartMission just before this scene loaded) and, if one exists,
    /// configures BattleManager's victory condition and spawns MissionData.playerUnits/enemyUnits
    /// along the grid's west/east edge — falling back to a generic squad of defaultPlayerUnit/
    /// defaultEnemyUnit when a mission hasn't authored its own roster, so an empty roster doesn't
    /// silently auto-win (AnnihilateEnemy with no enemies) or become unwinnable (CapturePoint with
    /// no player units to stand on the point). Reports the outcome back to MissionManager
    /// (CompleteActiveMission/FailActiveMission) once BattleManager.BattleEnded fires — nothing did
    /// either of these before: BattleManager.victoryCondition was edit-time-only, and the outcome
    /// panel's return button just loaded Kingdom without telling MissionManager anything happened.
    /// If ActiveMission is null (the Battle scene opened directly, e.g. to playtest it on its own),
    /// this is a no-op and BattleManager keeps whatever victoryCondition/units were set up by hand.
    /// </summary>
    public class MissionBattleSetup : MonoBehaviour
    {
        [SerializeField] private BattleManager battleManager;
        [SerializeField] private BattleGrid battleGrid;
        [SerializeField] private MissionManager missionManager;

        [SerializeField] private UnitData defaultPlayerUnit;
        [SerializeField] private UnitData defaultEnemyUnit;
        [SerializeField] private int defaultSquadSize = 3;

        private void Awake()
        {
            // missionManager lives on the persistent Bootstrap GameManager, in a different scene
            // from this bridge — Inspector references can't cross scenes, so fall back to the
            // running singleton when this field was left unassigned.
            if (missionManager == null && GameManager.Instance != null)
            {
                missionManager = GameManager.Instance.Missions;
            }
        }

        private void Start()
        {
            var mission = missionManager != null ? missionManager.ActiveMission : null;
            if (mission == null) return;

            battleManager.Configure(mission.victoryCondition);
            SpawnEdge(mission.playerUnits, defaultPlayerUnit, Allegiance.Player, west: true);
            SpawnEdge(mission.enemyUnits, defaultEnemyUnit, Allegiance.Enemy, west: false);

            battleManager.BattleEnded += OnBattleEnded;
        }

        private void OnDestroy()
        {
            if (battleManager != null) battleManager.BattleEnded -= OnBattleEnded;
        }

        private void OnBattleEnded(BattleOutcome outcome)
        {
            if (missionManager == null) return;

            if (outcome == BattleOutcome.Victory) missionManager.CompleteActiveMission();
            else if (outcome == BattleOutcome.Defeat) missionManager.FailActiveMission();
        }

        /// <summary>Places units in a single column along the grid's west (west: true) or east edge, one per available row — a generic deterministic layout since no mission hand-places a formation yet.</summary>
        private void SpawnEdge(List<UnitData> roster, UnitData fallback, Allegiance allegiance, bool west)
        {
            if (battleGrid == null || battleGrid.Grid == null) return;

            var units = roster != null && roster.Count > 0 ? roster : BuildFallbackRoster(fallback);
            if (units.Count == 0) return;

            var coordinates = battleGrid.Grid.Cells.Keys;
            int edgeQ = west ? coordinates.Min(c => c.q) : coordinates.Max(c => c.q);
            var column = coordinates.Where(c => c.q == edgeQ).OrderBy(c => c.r).ToList();

            for (int i = 0; i < units.Count && i < column.Count; i++)
            {
                if (units[i] == null) continue;
                battleManager.SpawnUnit(units[i], allegiance, column[i]);
            }
        }

        private List<UnitData> BuildFallbackRoster(UnitData fallback)
        {
            var list = new List<UnitData>();
            if (fallback == null) return list;

            for (int i = 0; i < defaultSquadSize; i++)
            {
                list.Add(fallback);
            }
            return list;
        }
    }
}
