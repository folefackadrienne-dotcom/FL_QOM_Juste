using KingdomOfGod.Alliance;
using KingdomOfGod.Buildings;
using KingdomOfGod.Collectibles;
using KingdomOfGod.Grid;
using KingdomOfGod.Miracles;
using KingdomOfGod.Missions;
using KingdomOfGod.Monetization;
using KingdomOfGod.Population;
using KingdomOfGod.Resources;
using KingdomOfGod.SaveSystem;
using KingdomOfGod.Verses;
using UnityEngine;

namespace KingdomOfGod.Core
{
    /// <summary>
    /// Root composition point for a kingdom-management scene. Wires together the manager
    /// components living on the same GameObject (or assigned in the Inspector) so gameplay
    /// systems can find each other through one place instead of FindObjectOfType calls.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Managers")]
        [SerializeField] private AgeManager ageManager;
        [SerializeField] private ResourceManager resourceManager;
        [SerializeField] private HexGrid hexGrid;
        [SerializeField] private BuildingManager buildingManager;
        [SerializeField] private PopulationSystem populationSystem;
        [SerializeField] private AllianceSystem allianceSystem;
        [SerializeField] private MiracleManager miracleManager;
        [SerializeField] private VerseManager verseManager;
        [SerializeField] private CollectionManager collectionManager;
        [SerializeField] private MissionManager missionManager;
        [SerializeField] private SaveManager saveManager;
        [SerializeField] private EntitlementManager entitlementManager;

        public AgeManager Ages => ageManager;
        public ResourceManager Resources => resourceManager;
        public HexGrid Grid => hexGrid;
        public BuildingManager Buildings => buildingManager;
        public PopulationSystem Population => populationSystem;
        public AllianceSystem Alliance => allianceSystem;
        public MiracleManager Miracles => miracleManager;
        public VerseManager Verses => verseManager;
        public CollectionManager Collection => collectionManager;
        public MissionManager Missions => missionManager;
        public SaveManager Save => saveManager;
        public EntitlementManager Entitlements => entitlementManager;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
}
