# Kingdom of God

Jeu de stratégie tour par tour et de gestion de royaume qui suit l'histoire
d'Israël dans l'Ancien Testament, sur PC et mobile.

Ce dépôt contient le **squelette du projet Unity** : l'architecture de code
des systèmes de jeu décrits dans le design (ressources, grille hexagonale,
bâtiments, batailles tactiques, miracles, Alliance, mémorisation de
versets, collection d'objets, missions, progression, sauvegarde,
monétisation freemium), avec les données de contenu des **7 Âges de la
campagne déjà remplies** (Patriarches → Exode → Conquête → Juges →
Monarchie Unifiée → Royaumes Divisés → Exil et Retour) et un outil
d'Éditeur qui génère automatiquement les 4 scènes de base entièrement
câblées (voir « Ouvrir le projet » ci-dessous). Il manque encore l'art, le
rendu visuel de la grille hexagonale, et les prefabs de personnages/unités.

Design de référence : [`docs/GDD.md`](docs/GDD.md) ·
[`docs/ArtDirection.md`](docs/ArtDirection.md) ·
[`docs/Economy.md`](docs/Economy.md) ·
[`docs/AudioDesign.md`](docs/AudioDesign.md)

## Prérequis

- Unity **2022.3.50f1 LTS** (voir `ProjectSettings/ProjectVersion.txt`)
- Universal Render Pipeline, TextMeshPro, Input System, Cinemachine, 2D
  Tilemap Extras (déclarés dans `Packages/manifest.json`, installés
  automatiquement à l'ouverture du projet)

## Ouvrir le projet

1. Ouvrir Unity Hub → Add → sélectionner ce dossier.
2. Laisser Unity réimporter les packages (`Packages/manifest.json`) — le
   package Input System étant déjà déclaré, Unity peut proposer une
   boîte de dialogue « activer le nouveau système d'entrée (redémarrage
   requis) » à la première ouverture : accepter, c'est nécessaire pour
   la caméra et les clics sur la grille (voir Interaction/ ci-dessous).
3. Lancer **Kingdom of God → Setup → Create All Scenes** dans le menu
   Unity pour générer et câbler `Bootstrap`, `MainMenu`, `Kingdom` et
   `Battle` d'un coup — voir
   [`Assets/_Project/Scenes/README.md`](Assets/_Project/Scenes/README.md)
   pour le détail de ce que chaque scène contient.
4. Ouvrir `Bootstrap` et lancer Play : ça enchaîne automatiquement sur
   `MainMenu`, où « Nouvelle Partie » ouvre `Kingdom` avec la barre de
   ressources déjà vivante (Foi, Blé, Eau… avec leurs valeurs de départ)
   et une caméra libre (WASD/flèches pour se déplacer, molette pour
   zoomer).

## Structure

```
Assets/_Project/
  Scripts/
    Core/          GameManager, cycle des 7 Âges, KingdomTurnManager —
                    le tour qui manquait à Kingdom : BuildingManager.
                    ProcessTurnProduction, PopulationSystem.Grow et la
                    conséquence d'une pénurie sur la Loyauté existaient
                    déjà en code mais rien ne les appelait ; EndTurn()
                    (bouton "Fin de Tour" du HUD) les enchaîne désormais
                    à chaque tour
    Resources/      Blé, Eau, Bois, Or, Foi, Sagesse, Justice
    Grid/           Grille hexagonale (coordonnées axiales, cellules),
                    HexCoordinates.FromWorldPosition (inverse de
                    ToWorldPosition, pour le clic-pour-sélectionner) —
                    HexGridRenderer affiche enfin la grille (tuiles hexagonales
                    plates générées en code, sans art : une couleur par
                    HexCell.TerrainType puisée dans UIThemeData, la teinte
                    « Plain » suivant l'Âge en cours via
                    UIThemeData.GetAgeAccent, plus une tuile de survol qui
                    suit la souris) au lieu de rien du tout
    Interaction/    Caméra RTS libre (HexCameraController : WASD/flèches
                    + molette) et clic-pour-jouer sur la grille —
                    KingdomInputController (pose de bâtiment via
                    BuildingManager.TryPlace, selectedBuilding piloté par
                    BuildingPaletteUI plutôt que réglé à la main) et
                    BattleInputController (sélection/déplacement/attaque
                    d'unité via BattleManager)
    Buildings/      Bâtiments, placement — BuildingManager.TryPlace
                    instancie BuildingData.prefab à la position de la
                    cellule dès qu'un prefab existe —, Temple (niveaux
                    1-5, un TempleLevelData.prefab par niveau,
                    TempleSystem.LevelUpgraded ; TempleSystem.levels
                    est désormais peuplé — CanUpgrade/TryUpgrade
                    existaient déjà mais la liste était vide, donc
                    inatteignables). BuildingData.populationCapacityBonus
                    (nouveau, même mécanisme que storageCapacityBonus)
                    donne enfin un effet aux 5 bâtiments d'Habitat.
                    ProcessTurnProduction multiplie désormais chaque
                    production par PopulationSystem.ProductionMultiplier
    Population/     Population & Loyauté (PopulationChanged,
                    LoyaltyLow/LoyaltyCritical) — ProductionMultiplier
                    (formule à paliers sur les seuils existants),
                    WheatUpkeep/WaterUpkeep, ComputeGrowth et Capacity
                    sont nouveaux ; ModifyLoyalty n'était jusqu'ici
                    jamais appelé qu'en négatif nulle part dans le code
                    (une pénurie ne remontait donc jamais) — corrigé par
                    la récompense "bien nourri" et par
                    KingdomTurnManager.ApplyGovernanceLoyalty (+1 tant
                    que Justice et Foi dépassent 0,5 par habitant,
                    cumulable, jamais négatif)
    Battle/         Batailles tactiques tour par tour, unités, boss
                    (AntagonistData, lié à sa fiche UnitData via le
                    champ optionnel UnitData.antagonist) —
                    BattleManager.SpawnUnit/TryMove/OnUnitDied
                    instancient, déplacent et détruisent
                    UnitData.prefab dès qu'un prefab existe ;
                    CheckBattleEnd (ex-CheckVictory) déclare aussi
                    Défaite désormais, pas seulement Victoire ;
                    BattleManager.Configure(VictoryCondition) permet de
                    remplacer à l'exécution la condition de victoire
                    réglée à l'édition (voir Missions/MissionBattleSetup)
    Miracles/       Miracles conditionnels (5 catégories, coût en Foi, verset/
                    objet/Alliance requis, jauge de prière 1-4 tours
                    interruptible, limite à 1 usage unique et coût
                    d'Alliance pour les plus puissants)
    Alliance/       Jauge d'Alliance (0-100), repentance & multiplicateur
                    de puissance des miracles
    Verses/         Mémorisation de versets (mini-jeu progressif,
                    VerseUnlocked/VerseMemorized)
    Collectibles/   Artefacts bibliques (Commun → Légendaire),
                    CollectionManager.ArtifactCollected/AgeCollectionCompleted
    Missions/       Définition & suivi des missions
                    (MissionStarted/MissionCompleted/MissionFailed) —
                    MissionManager.StartMission charge désormais la scène
                    Battle partagée pour une mission de type Battle (8 des
                    35 missions), et MissionBattleSetup (câblé dans la
                    scène Battle) reprend l'ActiveMission côté Battle pour
                    configurer BattleManager.victoryCondition et faire
                    apparaître MissionData.playerUnits/enemyUnits (avec un
                    escadron générique de repli si une mission n'a pas
                    encore de composition dédiée), puis rapporte
                    Victoire/Défaite à CompleteActiveMission/
                    FailActiveMission quand la bataille se termine. Les
                    27 missions des 5 autres types se résolvent
                    directement dans Kingdom sans changer de scène, via
                    MissionResolutionUI (UI/) et 4 nouvelles méthodes de
                    résolution : TryResolveConstruction (dépense
                    MissionData.constructionCost via
                    ResourceManager.TrySpend), TryResolveSurvival
                    (vérifie que MissionData.survivalRequirement est
                    actuellement en réserve via ResourceManager.
                    CanAfford, sans rien dépenser — la mission reste
                    active tant que ce n'est pas le cas), ResolveMoralChoice
                    (applique le delta d'Alliance de MissionData.optionA/
                    optionB choisi via AllianceSystem.Modify, récompenses
                    de base dans tous les cas — l'enjeu est spirituel) et
                    ResolveDiplomacy (accorde le rewardOverride de
                    l'option choisie à la place des récompenses de base —
                    l'enjeu est pratique) ; ResolveSandbox complète sans
                    condition (1 seule mission de ce type). Les 26
                    missions concernées (hors la seule Sandbox) ont leurs
                    valeurs réelles renseignées dans leurs .asset — coûts/
                    seuils de ressources et libellés/deltas/récompenses
                    d'options plausibles au vu du récit de chaque
                    mission, non playtestés/équilibrés
    Progression/    Leaders légendaires (LeaderManager : débloqués +
                    leader actif), arbre technologique (3 arbres × 5
                    branches : Économique, Militaire, Spirituel ;
                    TechTree.TechUnlocked) — CanUnlock/TryUnlock sont du
                    code réel mais TechTree.allNodes était une liste
                    vide, rendant tout prérequis infranchissable quel
                    que soit le coût ; peuplée désormais par
                    ProjectSceneSetup.SetTechNodeList. Coûts des 93
                    TechNode rechiffrés (voir docs/Economy.md
                    « Coûts ») après simulation : la formule d'origine
                    laissait 10 technologies définitivement hors de
                    portée faute de Sagesse (seuls 3 des 39 bâtiments
                    en produisent, aucun avant l'Âge 4)
    SaveSystem/     Sauvegarde locale JSON (+ point d'extension cloud),
                    SaveManager.Saved/Loaded
    Monetization/   Entitlements (gratuit/Édition Complète), catalogue de
                    produits, seam IAP (stub Éditeur en attendant le vrai
                    store), EntitlementManager.ProductPurchased/TierChanged
    Audio/          Direction sonore : thèmes musicaux par contexte
                    (MusicThemeData), leitmotifs récurrents
                    (LeitmotifData), ambiances (AmbientSoundscapeData),
                    effets ponctuels d'Interface/Construction/Bataille/
                    Miracle/Foi & Alliance/Progression/Economy/Narrative/
                    Meta/Collectibles (SfxCueData) et le mixage
                    dynamique (AudioManager) — crossfade par scène,
                    bascule en Crise sous Alliance basse, sourdine
                    pendant la prière d'un miracle, SFX de
                    clic/validation/erreur/fermeture de menu, de
                    pose/amélioration de bâtiment, de combat (dont
                    entrée/défaite de boss), de rituel, de variation
                    des jauges de Foi/Alliance/Population, de
                    déblocage tech/leader, de progression
                    mission/verset, de sauvegarde/achat et de
                    collection d'artefact ; voix du Narrateur/des
                    Personnages et lecture des versets (VoiceLineData,
                    narrationClip* sur VerseData) avec sélection de
                    langue FR/EN/HE
    UI/             HUD, menu de prière, journal des versets —
                    RefreshList() instancie un MiracleListItemUI/
                    VerseListItemUI par entrée dès qu'un listItemPrefab
                    est assigné ; UIThemeData applique en couleurs plates
                    la palette de docs/ArtDirection.md (boutons, panneaux,
                    libellés) et WorldMoodUI teinte l'écran de Kingdom
                    selon AllianceSystem.StandingChanged ;
                    BattleHUDController — stats de l'unité sélectionnée
                    (BattleInputController.SelectionChanged), bouton Fin
                    de Tour, liste de miracles castables générée en code
                    (pas besoin de prefab, juste des boutons UITheme) via
                    BattleManager.TryCastMiracle, panneau Victoire/Défaite
                    sur BattleManager.BattleEnded (jusque-là sans aucun
                    auditeur) ; BuildingPaletteUI — un bouton par
                    BuildingData débloqué (AgeManager.IsUnlocked), généré
                    en code comme les boutons de miracle, sélection ->
                    KingdomInputController.SelectBuilding, avec un
                    libellé et un bouton Annuler toujours visibles hors
                    du panneau ; MissionListUI — liste les 35 missions
                    débloquées et non terminées, un bouton par mission
                    appelant MissionManager.StartMission (charge la
                    scène Battle pour une mission de combat, ouvre
                    MissionResolutionUI pour les 5 autres types) ;
                    MissionResolutionUI — panneau qui reconstruit ses
                    boutons d'action selon MissionType : coût en
                    ressources + bouton Contribuer (Construction),
                    seuil de ressources + bouton Vérifier (Survival,
                    vérifié sans être dépensé), deux boutons d'option
                    nommés (MoralChoice → AllianceSystem.Modify,
                    Diplomacy → jeu de récompenses différent par choix),
                    bouton Terminer inconditionnel (Sandbox) ; barre
                    d'outils à 5 boutons (Prière /
                    Versets / Prophétie / Bâtiments / Missions) qui
                    n'ouvrait jusque-là aucune UI malgré des méthodes
                    HUDController déjà prêtes pour les 4 premiers ;
                    TempleUI — petit widget permanent (jamais fermé,
                    contrairement aux autres panneaux) affichant le
                    niveau du Temple et un bouton Améliorer actif dès
                    que TempleSystem.CanUpgrade() l'autorise ; bouton
                    "Fin de Tour" + libellé de tour appelant
                    HUDController.EndTurn -> KingdomTurnManager.EndTurn,
                    seule façon de faire avancer un tour dans Kingdom
  Editor/         ProjectSceneSetup.cs — génère les 4 scènes de base
                  (menu Kingdom of God > Setup) ; VoiceNarrationImporter.cs
                  — importe en masse des enregistrements de narration
                  nommés "audio <Référence>" (ex. "audio Genese 12,2-3.mp3")
                  et les assigne au bon VerseData.narrationClip* en
                  faisant correspondre la référence extraite du nom de
                  fichier (accents/virgule-deux-points ignorés) (menu
                  Kingdom of God > Setup > Import Voice Narrations) ;
                  SfxImporter.cs — importe des effets sonores nommés
                  "sound effects - <description>" (ex. "sound effects -
                  male cry.mp3") et suggère un clip pour chacun des 44
                  SfxCueData par recoupement de mots-clés avec sa
                  description (un même fichier peut être suggéré et
                  assigné à plusieurs SFX) (menu Kingdom of God > Setup >
                  Import Sound Effects) ; MusicVideoImporter.cs — extrait
                  la piste audio de vidéos nommées "musique <Thème>"
                  (ex. "musique Menu Principal.mp4") via ffmpeg (requis
                  sur le poste) et l'assigne au MusicThemeData/
                  LeitmotifData/AmbientSoundscapeData dont le nom
                  correspond exactement (menu Kingdom of God > Setup >
                  Import Music Videos), assembly Editor-only
  ScriptableObjects/  Assets de données créés dans l'Éditeur
                      (Buildings, Units, Miracles, Verses, Artifacts,
                      Missions, Leaders, Antagonists, Techs, Ages,
                      Monetization, Audio) — les 7 Âges sont remplis
                      (Buildings, Verses, Artifacts, Missions, Miracles),
                      plus les 10 leaders, 5 antagonistes majeurs — chacun
                      avec sa fiche de statistiques de boss dans Units/
                      (Unit_Boss*, UnitData.antagonist renseigné) —, les 93
                      nœuds des 3 arbres technologiques (Techs/) et la
                      direction sonore complète (Audio/ : 8 thèmes
                      musicaux, 6 leitmotifs, 6 ambiances, 44 SFX
                      d'Interface/Construction/Bataille/Miracle/Foi &
                      Alliance/Progression/Economy/Narrative/Meta/
                      Collectibles (dont Entrée en Crise, Faveur
                      Élevée, Repentance, déblocages de tech/leader,
                      variation de Population, Temple amélioré,
                      progression mission/verset, fermeture de menu,
                      entrée/défaite de boss, sauvegarde/achat et
                      artefact précieux trouvé), 3 lignes de voix
                      Narrateur/Personnages), et UI/UITheme.asset — la
                      palette de couleurs de docs/ArtDirection.md
                      traduite en valeurs concrètes (UIThemeData)
  Scenes/             Générées par Kingdom of God > Setup > Create All
                      Scenes — voir le README du dossier
  Prefabs/, Art/, Audio/
docs/
  GDD.md              Document de conception consolidé
  ArtDirection.md      Direction artistique détaillée
  Economy.md           Système économique : ressources, bâtiments,
                        population, commerce, arbres technologiques
  AudioDesign.md        Direction sonore : musique par contexte,
                        leitmotifs, ambiances, SFX, voix, mixage
```

Chaque système de gameplay est un composant indépendant (grid: `HexGrid`,
`ResourceManager`, `AllianceSystem`, etc.), assemblé par `GameManager`
(`Assets/_Project/Scripts/Core/GameManager.cs`) plutôt que retrouvé via
`FindObjectOfType`. Le contenu (bâtiments, unités, miracles, versets,
artefacts, missions, leaders, techs) est data-driven via des
`ScriptableObject` — créables depuis le menu `Create > Kingdom of God > ...`
dans l'Éditeur, sans toucher au code.

## Prochaines étapes suggérées

1. Ouvrir le projet dans Unity et lancer **Kingdom of God → Setup →
   Create All Scenes** (voir ci-dessus) — les scènes ne sont pas encore
   validées dans un vrai Éditeur Unity, donc à vérifier/ajuster à la
   première ouverture.
2. Peupler `Assets/_Project/ScriptableObjects/Leaders`/`Techs` etc. dans
   les scènes — le contenu narratif (Buildings/Verses/Artifacts/Missions/
   Miracles/Leaders/Antagonists/Techs, y compris les bâtiments génériques
   Ferme/Scierie/Mine/Marché/Grenier/Réservoir/Grand Marché/Atelier de
   Charpentiers/Fonderie/Tribunal) est fait pour les 7 Âges.
3. `HexGridRenderer` affiche désormais `HexGrid`/`BattleGrid` (tuiles
   hexagonales plates générées en code dans `Kingdom` et `Battle`,
   colorées par `HexCell.TerrainType` et teintées par l'Âge en cours, plus
   une tuile de survol qui suit la souris) — reste à faire générer une
   vraie variété de terrain (`HexGrid.GenerateHexagonalMap` ne pose que
   `TerrainType.Plain` pour l'instant, aucun générateur de relief
   n'existe). La palette de sélection de bâtiment (`BuildingPaletteUI`) et
   l'UI de combat (`BattleHUDController` : stats d'unité, Fin de Tour,
   liste de miracles, Victoire/Défaite) existent toutes les deux, il ne
   leur manque que des icônes une fois l'art produit.
4. Habiller visuellement le menu de prière et le journal des versets
   (`PrayerMenuUI`/`VerseJournalUI` ont désormais leur boucle
   d'instanciation de liste — `RefreshList()` peuple un
   `MiracleListItemUI`/`VerseListItemUI` par miracle castable/verset
   mémorisé — mais elle reste un no-op tant qu'aucun prefab d'item n'est
   assigné à `listItemPrefab`, faute d'art produit).
   Le rituel complet de prière (`MiracleManager.BeginPrayer` /
   `AdvancePrayerTurn` / `AccelerateWithFaith` / `InterruptPrayer`) est déjà
   branché côté `Battle` via `TurnController` ; `PrayerMenuUI` utilise pour
   l'instant `TryCast` (résolution instantanée). `Kingdom` a désormais sa
   propre boucle de tours (`KingdomTurnManager`, voir `Core/` ci-dessus) —
   la précondition qui manquait pour rebrancher `PrayerMenuUI` sur le
   rituel complet existe maintenant, mais le rebranchement lui-même (barre
   de progression, annulation, `AdvancePrayerTurn` appelé depuis
   `KingdomTurnManager.EndTurn` quand une prière est en cours) reste à
   faire — pas tenté ici, changement de comportement UI à part entière.
5. Créer des prefabs d'unités référençant les 6 `UnitData` de base
   (`Assets/_Project/ScriptableObjects/Units`) pour peupler la scène
   `Battle`.
6. Remplacer `EditorIAPService` par une vraie intégration store (Unity IAP
   ou équivalent) une fois les fiches produit créées dans App Store
   Connect / Play Console — voir `Assets/_Project/Scripts/Monetization`.
7. Composer/enregistrer les pistes audio décrites dans
   `docs/AudioDesign.md` et les assigner aux champs `clip` (encore vides)
   des `MusicThemeData`/`LeitmotifData`/`AmbientSoundscapeData`/`SfxCueData`/
   `VoiceLineData` dans `Assets/_Project/ScriptableObjects/Audio/`, et aux
   champs `narrationClipFrench`/`English`/`Hebrew` des 34 `VerseData`
   (`Assets/_Project/ScriptableObjects/Verses/`) — `AudioManager` pilote
   déjà le crossfade, le mixage dynamique, le déclenchement contextuel
   (y compris les 44 SFX d'Interface, Construction, Bataille, Miracle,
   Foi & Alliance, Progression, Economy, Narrative, Meta et
   Collectibles) et la lecture de voix multilingue, il ne manque que
   les fichiers son. Pour les narrations de versets nommées
   "audio <Référence>" (ex. "audio Genese 12,2-3.mp3"), voir
   **Kingdom of God → Setup → Import Voice Narrations**
   (`VoiceNarrationImporter.cs`) qui les importe, les associe
   automatiquement au bon `VerseData` par correspondance de référence
   (accents/virgule-deux-points ignorés) et les renomme — à vérifier
   avant de valider, les fichiers non reconnus restent assignables à
   la main. Pour les effets sonores nommés "sound effects -
   <description>" (ex. "sound effects - male cry.mp3"), voir
   **Kingdom of God → Setup → Import Sound Effects**
   (`SfxImporter.cs`) qui suggère un clip par `SfxCueData` en
   recoupant les mots-clés de sa description — un même fichier peut
   être assigné à plusieurs SFX (ex. un cri de guerre servant aussi
   d'entrée en scène de boss) ; toujours vérifier/écouter avant
   d'assigner, l'appariement reste une heuristique. Pour les musiques
   enregistrées en vidéo et nommées "musique <Thème>" (ex. "musique
   Menu Principal.mp4"), voir **Kingdom of God → Setup → Import Music
   Videos** (`MusicVideoImporter.cs`, nécessite ffmpeg installé sur le
   poste) qui en extrait la piste audio et l'associe au
   `MusicThemeData`/`LeitmotifData`/`AmbientSoundscapeData` dont le nom
   correspond exactement.
8. Remplacer les couleurs plates de `UIThemeData`/`ProjectSceneSetup` par
   de vraies textures/sprites 9-slice une fois l'art produit (bordures
   dorées, fond parchemin texturé, icônes symboliques du blé/eau/flamme/
   rouleau de Torah décrites dans `docs/ArtDirection.md`) — la palette de
   couleurs elle-même (`Assets/_Project/ScriptableObjects/UI/UITheme.asset`)
   n'a pas besoin d'attendre l'art et est déjà appliquée.
9. Les 35 missions se résolvent désormais toutes de bout en bout depuis
   `MissionListUI` : les 8 de type `Battle` via
   `MissionManager.StartMission` → scène `Battle` → `MissionBattleSetup`
   (configure `BattleManager.victoryCondition`, fait apparaître
   `MissionData.playerUnits`/`enemyUnits` avec un escadron générique de
   3 `Unit_Fantassin` de repli tant qu'une mission n'a pas sa propre
   composition, rapporte Victoire/Défaite à `MissionManager` au retour) ;
   les 27 des 5 autres types via `MissionResolutionUI`, ouvert
   directement dans `Kingdom` sans changer de scène — `Construction`
   dépense `MissionData.constructionCost`, `Survival` vérifie
   `survivalRequirement` sans le dépenser, `MoralChoice` applique le
   delta d'Alliance de l'option choisie (`AllianceSystem.Modify`),
   `Diplomacy` accorde le jeu de récompenses de l'option choisie à la
   place des récompenses de base, `Sandbox` (1 mission) complète sans
   condition. Reste ouvert : composer un vrai `playerUnits`/`enemyUnits`
   par mission de combat (actuellement tous vides, donc tout le monde
   combat avec l'escadron de repli) ; les valeurs de coût/seuil/option
   des 26 missions non-Sandbox sont plausibles au vu du récit de chaque
   mission mais pas playtestées/équilibrées ; et
   `VictoryConditionType.ProtectUnit` reste incomplet — son commentaire
   dit "vérifié séparément via `UnitInstance.Died`" mais rien
   n'implémente cette vérification (`UnitInstance` n'a pas d'identifiant
   à faire correspondre à `protectedUnitId`) ; aucune des 35 missions
   actuelles ne l'utilise, donc ce n'est pas bloquant aujourd'hui.
10. L'économie de `Kingdom` tourne enfin : `KingdomTurnManager` (nouveau,
    bouton "Fin de Tour" du HUD) appelle chaque tour
    `BuildingManager.ProcessTurnProduction`, l'entretien de la population
    (Blé/Eau, `PopulationSystem.WheatUpkeep`/`WaterUpkeep`) et la
    croissance (`PopulationSystem.ComputeGrowth`, plafonnée par
    `Capacity`) — ces méthodes existaient déjà mais rien ne les appelait,
    donc les 39 `BuildingData` déjà chiffrés ne produisaient jamais rien
    en pratique. Un vrai bug corrigé au passage :
    `PopulationSystem.ModifyLoyalty` n'était jusque-là jamais appelé
    qu'en négatif nulle part dans le code, donc toute pénurie devenait un
    cliquet à sens unique vers 0 % de Loyauté pour toujours ; une
    récompense "bien nourri" (+2 Loyauté/tour) comble ce manque.
    `KingdomTurnManager.ApplyGovernanceLoyalty` chiffre également « la
    Justice et la Foi sont les meilleurs moyens de maintenir une haute
    loyauté » (`docs/Economy.md` §3) : +1 Loyauté/tour supplémentaire,
    cumulable avec la récompense "bien nourri", tant que les réserves de
    Justice *et* de Foi dépassent chacune 0,5 par habitant — volontairement
    sans pénalité miroir (les bâtiments de Justice n'existent qu'à partir
    de l'Âge 2, donc pénaliser son absence punirait un manque que le
    système d'Âges rend inévitable en tout début de partie). Le Temple est
    également débranché de son vide : `TempleSystem.levels` (niveaux 2 à
    5) est peuplé, et `TempleUI` (widget permanent du HUD) donne enfin un
    moyen d'appeler `TryUpgrade`. Toutes les valeurs nouvelles (taux de
    consommation, croissance, capacité de logement, coûts du Temple, seuil
    de gouvernance) ont été validées par une simulation Python de 420
    tours contre les 39 bâtiments déjà chiffrés (voir la note de chiffrage
    dans `docs/Economy.md` §3) plutôt que devinées au hasard — mais jamais
    testées en jeu réel, aucun Éditeur Unity n'étant disponible dans cet
    environnement. `TempleLevelData.miraclesUnlocked` reste vide faute de
    champ de niveau de Temple sur `MiracleData` pour établir la
    correspondance.
11. Les coûts des 93 `TechNode` (`docs/Economy.md` « Coûts ») ont été
    inclus dans la même simulation Python et rechiffrés : la formule
    d'origine (`10 + (palier - 1) × 8` Sagesse) laissait 10 technologies
    définitivement inatteignables quelle que soit la stratégie de jeu,
    parce que seuls 3 des 39 `BuildingData` produisent de la Sagesse et
    aucun avant l'Âge 4 — la Sagesse reste bloquée à sa valeur de départ
    pendant 4 Âges sur 7, la recherche ne progressant que par à-coups
    via les récompenses ponctuelles de missions. Nouvelle formule (`8 +
    (palier - 1) × 6`, alignée sur celle déjà utilisée pour la Foi) :
    les 93 sont désormais atteignables en fin de simulation. Un vrai bug
    de câblage corrigé au passage : `TechTree.allNodes` était une liste
    vide, donc `CanUnlock`/`TryUnlock` (du code réel et correct)
    renvoyaient toujours `false` pour toute technologie ayant un
    prérequis — peuplée désormais par
    `ProjectSceneSetup.SetTechNodeList`. Reste ouvert : aucune UI ne
    permet encore de parcourir/lancer une recherche parmi les 93 nœuds
    (3 arbres × 5 branches) — contrairement au Temple ou aux missions,
    ce serait un vrai morceau d'UI à construire, hors du périmètre de ce
    chiffrage.
