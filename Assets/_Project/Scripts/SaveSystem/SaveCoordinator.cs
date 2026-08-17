using System;
using System.Collections.Generic;
using KingdomOfGod.Core;
using KingdomOfGod.Resources;
using UnityEngine;

namespace KingdomOfGod.SaveSystem
{
    /// <summary>
    /// The missing link between SaveManager (which only reads/writes the JSON file) and the
    /// gameplay systems themselves: SaveManager.SaveLocal/LoadLocal were never called by anything,
    /// and MainMenuController.OnContinue discarded the SaveData that LoadLocal returned. Capture()
    /// gathers every system's current state into a SaveData snapshot; Apply() reapplies one loaded
    /// from disk, in an order that keeps each side-effect-bearing "do the thing" method
    /// (AdvanceStep, Collect, Complete, TryUnlock, TryPlace...) out of the loop — those already
    /// granted their resource bonuses/spent their costs once, and SaveData.resourceValues already
    /// captures the final totals, so each system instead gets a RestoreFromSave that sets its state
    /// directly. Entitlements restore first (AgeManager's unlock gate reads the tier), then
    /// ages/temple/buildings (which raise resource caps and population capacity), then the resource
    /// stock itself (so it isn't clamped against not-yet-restored caps), then everything else.
    /// </summary>
    public class SaveCoordinator : MonoBehaviour
    {
        [SerializeField] private GameManager gameManager;

        private void Awake()
        {
            if (gameManager == null) gameManager = GameManager.Instance;
        }

        /// <summary>Captures the current game state and writes it to the local save file.</summary>
        public void SaveGame()
        {
            if (gameManager == null || gameManager.Save == null) return;
            gameManager.Save.SaveLocal(Capture());
        }

        public SaveData Capture()
        {
            var data = new SaveData
            {
                currentAge = (int)gameManager.Ages.CurrentAge,
                entitlementTier = gameManager.Entitlements.Tier,
                allianceValue = gameManager.Alliance.Value,
                templeLevel = gameManager.Temple.CurrentLevel,
                population = gameManager.Population.Population,
                loyalty = gameManager.Population.Loyalty
            };

            foreach (Age age in Enum.GetValues(typeof(Age)))
            {
                if (gameManager.Ages.IsUnlocked(age)) data.unlockedAges.Add((int)age);
            }

            data.ownedProductIds.AddRange(gameManager.Entitlements.OwnedProductIds);

            foreach (ResourceType type in Enum.GetValues(typeof(ResourceType)))
            {
                data.resourceTypes.Add(type.ToString());
                data.resourceValues.Add(gameManager.Resources.Get(type));
            }

            foreach (var verse in gameManager.Verses.MemorizedVerses)
            {
                data.memorizedVerseIds.Add(verse.reference);
            }

            foreach (var artifact in gameManager.Collection.Owned)
            {
                data.ownedArtifactIds.Add(artifact.displayName);
            }

            foreach (var mission in gameManager.Missions.CompletedMissions)
            {
                data.completedMissionIds.Add(mission.missionId);
            }

            foreach (var node in gameManager.Tech.UnlockedNodes)
            {
                data.unlockedTechIds.Add(node.techId);
            }

            foreach (var building in gameManager.Buildings.Buildings)
            {
                data.placedBuildings.Add(new PlacedBuildingSave
                {
                    buildingName = building.Data.displayName,
                    q = building.Position.q,
                    r = building.Position.r,
                    level = building.Level
                });
            }

            foreach (var leader in gameManager.Leaders.UnlockedLeaders)
            {
                data.unlockedLeaderIds.Add(leader.displayName);
            }
            if (gameManager.Leaders.ActiveLeader != null)
            {
                data.activeLeaderId = gameManager.Leaders.ActiveLeader.displayName;
            }

            foreach (var miracle in gameManager.Miracles.UsedOnceMiracles)
            {
                data.usedOnceMiracleIds.Add(miracle.displayName);
            }

            foreach (var prophecy in gameManager.Prophecies.UnlockedProphecies)
            {
                data.unlockedProphecyIds.Add(prophecy.displayName);
            }

            return data;
        }

        /// <summary>Reapplies a loaded SaveData to every system. See the class summary for why order matters here.</summary>
        public void Apply(SaveData data)
        {
            gameManager.Entitlements.RestoreFromSave(data.entitlementTier, data.ownedProductIds);

            var unlockedAges = new List<Age>();
            foreach (var age in data.unlockedAges) unlockedAges.Add((Age)age);
            gameManager.Ages.RestoreFromSave(unlockedAges, (Age)data.currentAge);

            gameManager.Temple.RestoreFromSave(data.templeLevel);

            var placedBuildings = new List<(string buildingName, int q, int r, int level)>();
            foreach (var b in data.placedBuildings) placedBuildings.Add((b.buildingName, b.q, b.r, b.level));
            gameManager.Buildings.RestoreFromSave(placedBuildings);

            var stock = new List<ResourceAmount>();
            for (int i = 0; i < data.resourceTypes.Count && i < data.resourceValues.Count; i++)
            {
                if (Enum.TryParse(data.resourceTypes[i], out ResourceType type))
                {
                    stock.Add(new ResourceAmount { type = type, amount = data.resourceValues[i] });
                }
            }
            gameManager.Resources.RestoreStock(stock);

            gameManager.Population.RestoreFromSave(data.population, data.loyalty);
            gameManager.Alliance.RestoreFromSave(data.allianceValue);
            gameManager.Verses.RestoreFromSave(data.memorizedVerseIds);
            gameManager.Collection.RestoreFromSave(data.ownedArtifactIds);
            gameManager.Missions.RestoreFromSave(data.completedMissionIds);
            gameManager.Tech.RestoreFromSave(data.unlockedTechIds);
            gameManager.Leaders.RestoreFromSave(data.unlockedLeaderIds, data.activeLeaderId);
            gameManager.Miracles.RestoreFromSave(data.usedOnceMiracleIds);
            gameManager.Prophecies.RestoreFromSave(data.unlockedProphecyIds);
        }
    }
}
