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
                    à chaque tour, et fait aussi avancer une prière en
                    cours (MiracleManager.AdvancePrayerTurn) — sauf si
                    PopulationSystem.LoyaltyCritical (bande de rébellion)
                    l'a déjà interrompue ce tour-ci, l'équivalent côté
                    Kingdom d'une attaque ennemie perturbant le rituel.
                    AgeManager.AdvanceToNextAge était une méthode réelle
                    et appelable que rien n'appelait — la campagne
                    n'avait aucune notion de "cet Âge est terminé, on
                    passe au suivant". AgeManager s'abonne désormais à
                    MissionManager.MissionCompleted et appelle
                    AdvanceToNextAge dès que les 5 missions de l'Âge
                    courant sont toutes terminées (MissionManager.
                    AreAllMissionsComplete) ; le verrou monétisation
                    (EntitlementManager, gratuit limité aux 2-3 premiers
                    Âges) s'applique enfin pour de vrai puisque UnlockAge
                    est désormais réellement appelé pour des Âges au-delà
                    du premier. Nouveau AgeNarrationController : joue une
                    courte réplique du Narrateur à chaque nouvel Âge
                    (AgeManager.AgeUnlocked) et un épilogue à la fin de la
                    campagne (AgeManager.CampaignCompleted, nouveau
                    événement) — 8 VoiceLineData créés dans
                    Assets/_Project/ScriptableObjects/Audio/Voice/
                    (Voice_Age1Intro…Age7Intro, Voice_CampaignEpilogue),
                    textes validés avec l'utilisateur, clips audio non
                    encore fournis
    Resources/      Blé, Eau, Bois, Or, Foi, Sagesse, Justice
    Grid/           Grille hexagonale (coordonnées axiales, cellules),
                    HexCoordinates.FromWorldPosition (inverse de
                    ToWorldPosition, pour le clic-pour-sélectionner) —
                    HexGridRenderer affiche enfin la grille (tuiles hexagonales
                    générées en code : une couleur par HexCell.TerrainType
                    puisée dans UIThemeData par défaut (la teinte « Plain »
                    suivant l'Âge en cours via UIThemeData.GetAgeAccent), ou
                    une vraie texture quand TerrainTileSet en fournit une
                    pour ce TerrainType (6/8 aujourd'hui — Plain/Mountain/
                    Forest restent en couleur, faute d'illustration reçue),
                    plus une tuile de survol qui suit la souris) au lieu de
                    rien du tout. TerrainGenerator (nouveau) donne enfin
                    autre chose que du Plain à afficher : bruit de valeur
                    fait main (élévation/humidité/rivière/côte + hachage
                    de ruines), seedé (HexGrid.seed) donc reproductible,
                    calibré par simulation Python plutôt que deviné, avec
                    des garde-fous sur la colonne de spawn Battle et le
                    centre de la carte (seul point de capture codé en dur)
                    pour que Mountain (le seul TerrainType impassable) n'y
                    apparaisse jamais
    Interaction/    Caméra RTS libre (HexCameraController : WASD/flèches
                    + molette) et clic-pour-jouer sur la grille —
                    KingdomInputController (pose de bâtiment via
                    BuildingManager.TryPlace, selectedBuilding piloté par
                    BuildingPaletteUI plutôt que réglé à la main) et
                    BattleInputController (sélection/déplacement/attaque
                    d'unité via BattleManager)
    Buildings/      Bâtiments, placement — BuildingManager.TryPlace
                    instancie BuildingData.prefab à la position de la
                    cellule dès qu'un prefab existe, sinon (les 39
                    BuildingData actuels n'en ont aucun) génère un
                    placeholder en code : une primitive Unity
                    (Cube/Cylindre/Sphère) mise à l'échelle de la
                    tuile, forme/hauteur/couleur selon BuildingCategory
                    via UITheme (cubes pâles bas pour Habitat,
                    cylindres ochre pour Production, cubes sombres
                    hauts pour Militaire, cylindres dorés hauts pour
                    Spirituel, sphère bleue pour Spécial), plus un
                    libellé TextMeshPro flottant qui fait toujours face
                    à la caméra (BillboardLabel, nouveau). Remplacer
                    l'art se fera juste en assignant BuildingData.prefab,
                    sans toucher au code. Temple (niveaux
                    1-5, un TempleLevelData.prefab par niveau,
                    TempleSystem.LevelUpgraded ; TempleSystem.levels
                    est désormais peuplé — CanUpgrade/TryUpgrade
                    existaient déjà mais la liste était vide, donc
                    inatteignables) — TempleSystem lui-même n'avait
                    aucune position ni aucun rendu : c'est un
                    GameObject non-visuel sur le GameManager persistant,
                    sans HexCoordinates ni référence à HexGrid, et
                    TempleLevelData.prefab n'était jamais instancié
                    nulle part. Nouveau TempleVisualController (fixe au
                    centre de la carte (0,0), la seule cellule déjà
                    garantie jamais Montagne par TerrainGenerator —
                    c'est l'objectif CapturePoint codé en dur de
                    Mission_Age5_03_DavidRoiAHebronPuisJerusalem, réutilisé
                    ici comme "capitale") : un placeholder doré qui
                    grandit avec le niveau (corps ochre→or, "capstone"
                    cube pivoté en divineLight, docs/ArtDirection.md
                    "imposant, lumineux, couvert d'or"), reconstruit à
                    chaque TempleSystem.LevelUpgraded. Nouveau
                    HexCell.IsReserved marque cette cellule pour que
                    BuildingManager ne puisse jamais y laisser construire
                    un bâtiment par-dessus. BuildingData.populationCapacityBonus
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
                    UnitData.prefab dès qu'un prefab existe, sinon (les
                    11 UnitData actuels n'en ont aucun) génèrent un
                    placeholder en code : une primitive (forme selon
                    UnitClass — cube/cylindre/capsule/sphère) colorée par
                    Allegiance (bleu Joueur, rouge Ennemi, olive Allié),
                    agrandie et assombrie pour un boss
                    (UnitData.antagonist renseigné), avec un libellé
                    TextMeshPro suivant la caméra (BillboardLabel,
                    déplacé de Buildings/ vers Core/ car désormais
                    partagé avec BuildingManager) — même discipline
                    UITheme-au-lieu-d'art que les bâtiments ;
                    CheckBattleEnd (ex-CheckVictory) déclare aussi
                    Défaite désormais, pas seulement Victoire ;
                    BattleManager.Configure(VictoryCondition) permet de
                    remplacer à l'exécution la condition de victoire
                    réglée à l'édition (voir Missions/MissionBattleSetup).
                    Les 8 missions de type Battle ont désormais un vrai
                    roster (`MissionData.playerUnits`/`enemyUnits`,
                    plus l'escadron générique de repli) plutôt que des
                    listes vides : La Mer Rouge (Unit_BossPharaon + 2
                    Unit_Char contre 2 Unit_Fantassin), La Chute de
                    Jéricho (3 Unit_Fantassin + 1 Unit_Archer contre une
                    garnison de 2 Unit_Archer + 1 Unit_Fantassin), La
                    Bataille de Gabaon (armée à 5 contre la coalition
                    des cinq rois amoréens, 5 unités en face), Débora et
                    Barac (Unit_Prophete inclus côté joueur, contre
                    3 Unit_Char représentant les chars de Sisera),
                    Gédéon et les 300 (2 Unit_Fantassin seulement côté
                    joueur — la mécanique « moins d'unités » de la
                    mission — contre 5 unités madianites), David et
                    Goliath (Unit_Archer + 2 Unit_Fantassin en soutien
                    contre Unit_BossGoliath seul, en duel), David roi à
                    Hébron puis Jérusalem (armée royale à 4 contre une
                    garnison jébusite, condition CapturePoint), et
                    Ézéchias et Sennachérib (Unit_PretreLevite inclus
                    pour le soin, contre Unit_BossSennacherib + 2
                    Unit_Char + 1 Unit_Archer). Composés à la main à
                    partir du récit et des statistiques déjà existantes
                    des 6 `UnitData` de base et des 3 fiches de boss
                    narrativement liées (Pharaon, Goliath, Sennachérib) ;
                    pas playtestés en combat réel. A nécessité de créer
                    des `.meta` avec GUID fixe pour les 9 `UnitData`
                    désormais référencées par nom depuis des `.asset` de
                    mission — elles n'en avaient encore aucun, rien ne
                    les ayant référencées par GUID jusqu'ici. Ce "pour le
                    soin" restait cependant purement narratif :
                    UnitData.canHeal/healAmount et UnitInstance.Heal
                    étaient des membres réels et fonctionnels sans aucun
                    appelant — Unit_PretreLevite était placé dans le
                    roster de cette mission mais ne pouvait rien soigner
                    du tout. Nouveau BattleManager.TryHeal (réutilise
                    attackRange comme portée de soin plutôt que d'ajouter
                    un champ dédié pour cette seule unité) ; cliquer sur
                    un allié avec un soigneur sélectionné soigne
                    désormais au lieu de resélectionner
                    (BattleInputController), et BattleHUDController
                    affiche "Soin X" dans la fiche d'unité quand
                    UnitData.canHeal est vrai
    Miracles/       Miracles conditionnels (5 catégories, coût en Foi, verset/
                    objet/Alliance requis, jauge de prière 1-4 tours
                    interruptible, limite à 1 usage unique et coût
                    d'Alliance pour les plus puissants). MiracleVfxController
                    (nouveau) — PrayerStarted/MiracleCast/PrayerCancelled
                    étaient déjà câblés côté son (AudioManager) mais sans
                    rien à l'écran ; un unique ParticleSystem généré en
                    code (matériau Default-Particle intégré à Unity,
                    aucune texture à produire) fait une lueur dorée
                    continue pendant la prière et une salve à la
                    résolution, ancré à un point fixe au-dessus du centre
                    de la carte (la prière n'est liée à aucune unité/
                    bâtiment précis, dans Kingdom comme dans Battle).
                    MiracleManager.Unlock était une méthode réelle et
                    appelable que rien n'appelait jamais — tout le rituel
                    de prière fonctionnait déjà mais aucun miracle ne
                    pouvait être castable. Nouveau champ MiracleData.age
                    (rempli sur les 24 assets d'après leur chronologie
                    biblique) + abonnement à AgeManager.AgeUnlocked (même
                    schéma que Verses/Collectibles/Leaders) ; au passage,
                    usedOnceMiracles (qui empêche de recaster un miracle à
                    usage unique) n'était jamais sauvegardé — corrigé via
                    SaveData.usedOnceMiracleIds/RestoreFromSave
    Alliance/       Jauge d'Alliance (0-100), repentance & multiplicateur
                    de puissance des miracles. AllianceSystem.TryRepent
                    était réelle et appelable ("le joueur peut toujours se
                    repentir", dit son propre commentaire) mais sans aucun
                    appelant — nulle part où relever une Alliance tombée
                    Basse autrement que passivement via des récompenses de
                    mission. Nouveau widget permanent RepentanceUI (UI/,
                    à côté du TempleWidget) affiche la valeur/le palier et
                    un bouton "Se Repentir" ; nouvelle méthode
                    AllianceSystem.CanRepent() pilote son état actif/
                    inactif comme TempleSystem.CanUpgrade()
    Verses/         Mémorisation de versets (mini-jeu progressif,
                    VerseUnlocked/VerseMemorized). Unlock/AdvanceStep
                    étaient réels et appelables mais sans aucun appelant :
                    VerseManager s'abonne désormais à AgeManager.
                    AgeUnlocked (déverrouille chaque VerseData dont l'Âge
                    vient de s'ouvrir) et le nouveau bouton "Avancer" du
                    journal (UI/) pilote AdvanceStep — les 4 étapes du GDD
                    (lecture/trous/ordre/quiz) sont représentées par un
                    clic chacune plutôt que 4 mini-jeux distincts, faute de
                    contenu de puzzle rédigé (mots à masquer, questions de
                    quiz) pour les 34 versets
    Prophecy/       (nouveau) Journal Prophétique — HUDController.
                    ToggleProphecyJournal ouvrait un panneau que
                    ProjectSceneSetup ne construisait qu'avec un fond
                    parchemin, sans aucune ProphecyData/ProphecyManager
                    pour l'alimenter. Nouveau ProphecyData (référence,
                    texte de la prophétie, texte d'accomplissement) + 7
                    prophéties bibliques réelles, une par Âge, dont
                    l'accomplissement est vérifiable dans le texte biblique
                    lui-même (Genèse 12/15 → Deutéronome 18 → Josué 6/1 Rois
                    16 → 1 Samuel 2 → 2 Samuel 7 → 1 Rois 21/2 Rois 9 →
                    Jérémie 25 & 29/Esdras 1) ; ProphecyManager s'abonne à
                    AgeManager.AgeUnlocked, même schéma que Verses/
                    Collectibles/Leaders/Miracles
    Collectibles/   Artefacts bibliques (Commun → Légendaire),
                    CollectionManager.ArtifactCollected/AgeCollectionCompleted
                    — Collect était une méthode réelle et appelable que
                    rien n'appelait jamais ; CollectionManager s'abonne
                    désormais lui-même à AgeManager.AgeUnlocked et
                    collecte chaque ArtifactData dont l'Âge vient de se
                    débloquer (aucune nouvelle donnée à saisir : le champ
                    ArtifactData.age existait déjà), et un nouveau panneau
                    Collection (UI/) affiche la fiche de chaque artefact
                    une fois découvert
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
                    mission, non playtestés/équilibrés. Nouveau
                    MissionManager.AreAllMissionsComplete(Age) — vérifie
                    si les 5 missions d'un Âge sont toutes terminées ;
                    c'est le critère qui manquait à
                    AgeManager.AdvanceToNextAge (voir Core/ ci-dessous)
    Progression/    Leaders légendaires (LeaderManager : débloqués +
                    leader actif) — Unlock/SetActiveLeader étaient des
                    méthodes réelles et appelables que rien n'appelait
                    jamais ; nouveau champ LeaderData.unlockMission
                    (renseigné pour les 6 des 10 leaders dont
                    unlockCondition cite une mission précise — David,
                    Débora, Élie, Gédéon, Salomon, Samson — nécessitant
                    un premier .meta pour ces 6 assets Mission, qui n'en
                    avaient aucun) et LeaderManager s'abonne à
                    AgeManager.AgeUnlocked/MissionManager.MissionCompleted
                    pour débloquer chaque leader au bon déclencheur (les 4
                    autres — Abraham, Moïse, Josué, Néhémie — dès que leur
                    Âge se débloque) ; un nouvel écran Leader (UI/) liste
                    les 10, affiche la fiche narrative de ceux débloqués
                    (« ??? (Verrouillé) » + unlockCondition sinon) et
                    permet d'Activer le leader courant, arbre
                    technologique (3 arbres × 5
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
                    en produisent, aucun avant l'Âge 4). Mais même
                    peuplé, allNodes n'était accessible d'aucune UI :
                    TryUnlock restait réel et appelable sans un seul
                    appelant, les 93 TechNode restant à jamais
                    inatteignables en jeu quel que soit leur coût. Nouvel
                    écran TechScreenUI (UI/), à onglets par catégorie
                    (Économique/Militaire/Spirituel — 93 nœuds d'un coup
                    sans onglets aurait été une liste ingérable, cf.
                    MissionListUI ci-dessous pour la même limite « pas de
                    ScrollRect dans ce projet »), qui pilote TryUnlock et
                    affiche coût/prérequis (résolus en noms lisibles
                    depuis leurs techId) pour le nœud sélectionné
    SaveSystem/     Sauvegarde locale JSON (+ point d'extension cloud),
                    SaveManager.Saved/Loaded — SaveManager ne fait que
                    lire/écrire le fichier ; SaveCoordinator.Capture/Apply
                    est le chaînon qui manquait : il rassemble l'état de
                    tous les systèmes (âges, ressources, Temple,
                    Population, Alliance, versets mémorisés, artefacts
                    possédés, missions/tech débloqués, leaders débloqués/
                    actif, bâtiments posés — nouveaux champs SaveData.
                    placedBuildings/unlockedLeaderIds/activeLeaderId,
                    jamais persistés jusqu'ici) et le réapplique via des
                    méthodes RestoreFromSave « silencieuses » qui
                    court-circuitent les bonus/coûts déjà appliqués une
                    première fois (AdvanceStep, Collect, Complete,
                    TryUnlock, TryPlace) pour ne pas les accorder deux
                    fois. Sauvegarde automatique en fin de tour + bouton
                    « Sauvegarder » dans la barre d'outils du Royaume ;
                    MainMenuController.OnContinue applique désormais
                    réellement la sauvegarde chargée au lieu de jeter le
                    résultat de LoadLocal
    Monetization/   Entitlements (gratuit/Édition Complète), catalogue de
                    produits, seam IAP (stub Éditeur en attendant le vrai
                    store), EntitlementManager.ProductPurchased/TierChanged
                    — mais PurchaseFullEdition/RestorePurchases étaient
                    réelles et appelables sans qu'aucun écran Boutique
                    n'existe nulle part : un joueur gratuit ne pouvait
                    donc *jamais* dépasser freeAgeLimit (Âge 2), et
                    AgeManager.AdvanceToNextAge échouait silencieusement
                    son verrou de contenu pour toujours au-delà — la
                    campagne ne pouvait jamais atteindre CampaignCompleted.
                    Nouveau StoreUI (UI/), accessible depuis la barre
                    d'outils du Royaume, affiche le palier courant et
                    appelle les deux méthodes ; EditorIAPService fait
                    réussir tout "achat" instantanément (stub Éditeur). Un
                    second blocage restait caché derrière celui-ci : un
                    joueur qui termine la dernière mission de son Âge
                    *avant* d'acheter restait quand même bloqué pour
                    toujours, achat ou pas — plus aucune mission à
                    terminer pour redéclencher la vérification, et rien
                    n'écoutait TierChanged pour retenter le déblocage.
                    Nouvel événement IContentGate.GateChanged (implémenté
                    par EntitlementManager, levé avec TierChanged) auquel
                    AgeManager s'abonne désormais lui-même pour retenter
                    AdvanceToNextAge dès que le palier change — sans que
                    AgeManager ait besoin de savoir pourquoi le verrou
                    s'est levé
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
                    PrayerMenuUI a deux vues (sélection/rituel) commutées
                    sur MiracleManager.IsPraying : la liste des miracles
                    castables est générée en code (un bouton UITheme par
                    MiracleData, comme BuildingPaletteUI, plus besoin de
                    MiracleListItemUI/prefab, classe supprimée) et
                    ConfirmCast appelle désormais BeginPrayer (jauge
                    multi-tours) au lieu de TryCast (résolution
                    instantanée) ; la vue rituel affiche la progression
                    et un bouton Accélérer (AccelerateWithFaith) et
                    Abandonner (InterruptPrayer) ; VerseJournalUI a été
                    reconstruit sur CreateCodexPanel (même liste+portrait
                    +détail que LeaderScreenPanel/AntagonistCodexPanel/
                    CollectionPanel, portrait masqué car VerseData n'a pas
                    de sprite) : lignes générées en code (plus de
                    listItemPrefab, VerseListItemUI supprimée) et un bouton
                    "Avancer" qui pilote VerseManager.AdvanceStep, puis
                    "Écouter" une fois le verset mémorisé ; UIThemeData
                    applique en couleurs plates
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
                    en code comme les boutons de miracle, affiche
                    désormais BuildingData.icon quand une icône est
                    assignée (14 des 39 bâtiments ont une image importée —
                    Autel de Pierres, Marché, Tabernacle, Tente Familiale
                    depuis un précédent round, plus Campement de Caravane,
                    Mine, Citerne de Siège, Scierie, Autel d'Ébal, Autel du
                    Sinaï, Camp de Guilgal, Champs de Canaan, Forteresse et
                    Ferme cette fois ; les 25 autres restent en couleur
                    plate, toujours utilisables),
                    sélection -> KingdomInputController.SelectBuilding,
                    avec un libellé et un bouton Annuler toujours visibles
                    hors du panneau ; MissionListUI — liste les 35 missions
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
                    seule façon de faire avancer un tour dans Kingdom ;
                    barre d'outils secondaire à 3 boutons (Leaders /
                    Antagonistes / Collection, au-dessus de la première —
                    6 boutons pleine largeur (240px) dépassaient déjà la
                    zone de canevas de référence) ouvrant 3 nouveaux
                    panneaux liste + fiche partageant un même gabarit
                    (CreateCodexPanel) : LeaderScreenUI — un bouton par
                    LeaderData (LeaderManager.AllLeaders), fiche narrative
                    si débloqué sinon "??? (Verrouillé)" + unlockCondition,
                    bouton Activer -> LeaderManager.SetActiveLeader ;
                    AntagonistCodexUI — codex en lecture seule des boss
                    majeurs, révélés par Âge (AgeManager.IsUnlocked),
                    fiche narrative + mécanique d'affrontement +
                    condition de victoire ; CollectionUI — fiche de
                    chaque ArtifactData une fois possédé (référence
                    biblique, contexte historique, commentaire éducatif,
                    effet), couleur de la rareté (Commun→Légendaire)
                    reprise de la palette UITheme. Corrigé au passage :
                    PrayerMenuUI/VerseJournalUI/BuildingPaletteUI/
                    MissionListUI n'avaient pas de VerticalLayoutGroup
                    sur leur ListContainer — chaque bouton généré en code
                    atterrissait à la même position (0,0), superposé aux
                    autres plutôt qu'empilé ; ConfigureListLayout
                    l'ajoute désormais partout (déjà présent seulement sur
                    BattleHUDController.miracleListContainer). Trois
                    écrans de plus, joignables depuis une nouvelle
                    troisième rangée d'outils (Technologies / Boutique) ou
                    en widget permanent (Repentance) : TechScreenUI —
                    voir Progression/ ci-dessus ; StoreUI — voir
                    Monetization/ ci-dessus ; RepentanceUI — voir
                    Alliance/ ci-dessus. ProphecyJournalUI — reconstruit
                    sur CreateCodexPanel comme VerseJournalUI (portrait
                    masqué, pas de sprite sur ProphecyData), une entrée
                    par Âge affichant la prophétie et son accomplissement
                    une fois débloquée ; voir Prophecy/ ci-dessus pour le
                    contenu et ProphecyManager
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
                  Import Music Videos) ; ImageImporter.cs — importe des
                  illustrations nommées "image <Nom>" (ex. "image
                  Moïse.png") comme Sprites et suggère une assignation
                  pour chaque fiche ayant un champ portrait/icône
                  (LeaderData, AntagonistData, UnitData, BuildingData,
                  MiracleData, ArtifactData, ProductData — 7 types, 135
                  fiches au total) par correspondance de nom avec
                  displayName (accents ignorés, sous-chaîne par mot
                  entier acceptée dans les deux sens) ; comme SfxImporter,
                  volontairement plusieurs-à-plusieurs : une même image
                  peut être suggérée et assignée à plusieurs fiches dont
                  le nom correspond (menu Kingdom of God > Setup > Import
                  Images), assembly Editor-only
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
   hexagonales générées en code dans `Kingdom` et `Battle`, colorées par
   `HexCell.TerrainType` et teintées par l'Âge en cours, ou texturées via
   `TerrainTileSet` pour 6 des 8 types, plus une tuile de survol qui suit
   la souris). `HexGrid.GenerateHexagonalMap` ne posait jusqu'ici que
   `TerrainType.Plain` (aucun générateur de relief n'existait), donc les
   textures de `Hill`/`Desert`/etc. n'avaient nulle part où s'afficher —
   `TerrainGenerator` (nouveau) comble ça : 4 canaux de bruit de valeur
   (élévation/humidité/rivière/côte, une grille de valeurs aléatoires
   interpolée bilinéairement, pas `Mathf.PerlinNoise`, pour que les seuils
   ci-dessous soient calibrés par simulation Python plutôt que devinés) et
   un hachage par cellule pour des ruines éparses, classent chaque
   cellule ; seeded (`HexGrid.seed`, 12345 par défaut) donc reproductible
   d'une session à l'autre. Deux garde-fous : la colonne `q == ±rayon`
   (là où `MissionBattleSetup.SpawnEdge` place toujours les unités de
   Battle) et le centre `(0, 0)` (le seul point de capture codé en dur,
   `Mission_Age5_03_DavidRoiAHebronPuisJerusalem`) ne deviennent jamais
   `Mountain`, le seul `TerrainType` que `HexCell.IsPassable` bloque —
   sans ça une carte malchanceuse aurait pu rendre une mission
   littéralement infranchissable ou faire apparaître une unité sur une
   case qu'elle ne peut plus jamais quitter. La palette de sélection de bâtiment (`BuildingPaletteUI`) et
   l'UI de combat (`BattleHUDController` : stats d'unité, Fin de Tour,
   liste de miracles, Victoire/Défaite) existent toutes les deux, il ne
   leur manque que des icônes une fois l'art produit. `BuildingManager.
   TryPlace` pose désormais quelque chose de visible même sans art : les 39
   `BuildingData` n'ont aucun `prefab` assigné, donc `SpawnVisual` génère un
   placeholder en code (primitive Unity mise à l'échelle de la tuile,
   forme/hauteur/couleur selon `BuildingCategory` via `UITheme`, libellé
   `TextMeshPro` qui suit toujours la caméra via `BillboardLabel`) — même
   discipline UITheme-au-lieu-d'art que `HexGridRenderer`/`BuildingPaletteUI`.
   Remplacer par le vrai art se fera juste en assignant `BuildingData.prefab`,
   sans toucher au code.
4. ~~Habiller visuellement le journal des versets~~ Fait : `VerseJournalUI`
   ne dépend plus d'un `listItemPrefab` (jamais assigné faute d'art produit,
   donc `RefreshList()` restait un no-op permanent) — reconstruite sur
   `CreateCodexPanel` avec des lignes générées en code, comme
   `BuildingPaletteUI`/`PrayerMenuUI`. Plus profond que prévu : `VerseManager.
   Unlock`/`AdvanceStep` étaient eux-mêmes des méthodes réelles sans aucun
   appelant, donc aucun verset n'atteignait jamais ce panneau. `VerseManager`
   s'abonne désormais à `AgeManager.AgeUnlocked` (déverrouille chaque
   `VerseData` dont l'Âge vient de s'ouvrir) et un bouton "Avancer" pilote
   `AdvanceStep`. Limite assumée : le mini-jeu de mémorisation en 4 étapes du
   GDD (lecture → trous → ordre → quiz de contexte) est représenté par un
   clic "Avancer" par étape plutôt que 4 écrans de puzzle distincts, faute de
   contenu de puzzle rédigé (quels mots masquer, quelles questions de quiz)
   pour les 34 versets — à construire séparément si souhaité. `PrayerMenuUI`,
   elle, n'a plus besoin de prefab : sa liste de
   miracles castables est générée en code (comme `BuildingPaletteUI`).
   Le rituel complet de prière (`MiracleManager.BeginPrayer` /
   `AdvancePrayerTurn` / `AccelerateWithFaith` / `InterruptPrayer`) était déjà
   branché côté `Battle` via `TurnController` ; `Kingdom` avait sa propre
   boucle de tours (`KingdomTurnManager`) mais `PrayerMenuUI` utilisait
   encore `TryCast` (résolution instantanée) faute de vue pour piloter un
   rituel qui dure plusieurs tours. C'est fait : `PrayerMenuUI` a
   maintenant une vue sélection (liste + `ConfirmCast` → `BeginPrayer`) et
   une vue rituel (progression, `Accélérer` → `AccelerateWithFaith`,
   `Abandonner` → `InterruptPrayer`), commutées sur
   `MiracleManager.IsPraying`. `KingdomTurnManager.EndTurn` appelle
   `AdvancePrayerTurn` chaque tour quand une prière est en cours, et
   `PopulationSystem.LoyaltyCritical` (bande de rébellion) l'interrompt —
   l'équivalent côté Kingdom de « une attaque ennemie peut faire régresser
   la jauge » côté Battle, puisqu'il n'y a pas d'ennemi sur la carte du
   Royaume. Un `PrayerStatusLabel` persistant sur `HUDController` (et non
   sur `PrayerMenuUI`, dont le GameObject se désactive avec le panneau, ce
   qui couperait ses abonnements aux événements) garde la progression
   visible même panneau fermé, sur le même principe que `turnLabel`.
5. Créer de vrais prefabs d'unités (les 11 `UnitData` — 6 de base + 5 boss
   — dans `Assets/_Project/ScriptableObjects/Units`) une fois l'art
   produit ; en attendant, `Battle` n'est plus vide : `BattleManager.
   SpawnVisual` retombe sur un placeholder en code (primitive colorée par
   Allegiance/UnitClass, voir `Battle/` ci-dessus) tant qu'aucun
   `UnitData.prefab` n'est assigné.
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
   les fichiers son. 42 des 44 `SfxCueData` et les 6 `AmbientSoundscapeData`
   ont reçu trois lots de fichiers envoyés directement dans la
   conversation plutôt qu'importés via `SfxImporter.cs` (aucun Éditeur
   Unity disponible ici pour le lancer) — assignation reproduite par
   script au même format, mais par **correspondance de nom de fichier
   uniquement** : contrairement aux images, l'agent ne peut pas écouter
   un fichier audio, donc chaque appariement (ex. "monster_roar.wav" →
   Antagonistes - Entrée en Scène du Boss, "buzz.flac" → Interface -
   Erreur / Action Impossible) doit être **vérifié à l'oreille** une fois
   le projet ouvert dans un vrai Éditeur avant de considérer que c'est
   bon — d'autant que plusieurs clips sont volontairement réutilisés sur
   plusieurs `SfxCueData` à la fois (ex. `SFX_DissonantPianoStinger` sert
   à la fois Miracle - Interruption, Foi en Baisse et Alliance en Baisse ;
   `SFX_PingNotification` sert Interface - Validation Positive,
   Monétisation - Achat Réussi et Missions - Mission Commencée), donc une
   correction à l'oreille peut concerner plusieurs fiches à la fois. Ne
   restent réellement vides que Progression - Leader Actif et
   Progression - Technologie Économique Débloquée (aucun clip reçu
   jusqu'ici ne convient : le premier veut un flourish de cuivres, le
   second un chime d'oud/terreux, et aucun fichier envoyé n'a cette
   couleur). Pour les narrations de versets nommées
   "audio <Référence>" (ex. "audio Genese 12,2-3.mp3"), voir
   **Kingdom of God → Setup → Import Voice Narrations**
   (`VoiceNarrationImporter.cs`) qui les importe, les associe
   automatiquement au bon `VerseData` par correspondance de référence
   (accents/virgule-deux-points ignorés) et les renomme — à vérifier
   avant de valider, les fichiers non reconnus restent assignables à
   la main. **34 des 34** `VerseData.narrationClipFrench` ont reçu un
   fichier envoyé directement dans la conversation (même limite que les
   SFX : appariement par référence dans le nom de fichier uniquement,
   pas d'écoute possible, donc à vérifier à l'oreille) — reproduit par
   script au même format que `VoiceNarrationImporter` plutôt que lancé
   depuis l'Éditeur : Genèse 12:2-3/15:6/22:12/50:20, Exode 3:14/14:14/
   20:2-3/33:14, Nombres 6:24-26, Josué 1:9/6:20/24:15, Juges 2:16/6:12/
   16:28, 1 Samuel 16:7 (fichier initialement nommé "audio_1_Samuel_77",
   confirmé par l'utilisateur comme 16:7 après vérification — aucun
   verset "1 Samuel 7:7" n'existe dans le jeu), 1 Samuel 17:45,
   2 Samuel 7:16, 1 Rois 3:9/8:27/18:21/19:12, 2 Rois 6:16/19:35,
   Psaume 23:1, Amos 5:24, Ésaïe 40:31/41:10, Jérémie 29:11,
   Daniel 3:17-18/6:23, Esdras 7:10, Néhémie 4:17/8:10 — couverture
   française complète. `narrationClipEnglish`/`Hebrew` sur les 34
   restent entièrement vides (aucun fichier reçu pour ces langues
   jusqu'ici). Pour les effets sonores nommés "sound effects -
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
   n'a pas besoin d'attendre l'art et est déjà appliquée. Pour les 135
   portraits/icônes de fiches de contenu réparties sur 7 types
   (`LeaderData.portrait`, `AntagonistData.portrait`, `UnitData.icon`,
   `BuildingData.icon`, `MiracleData.icon`, `ArtifactData.icon`,
   `ProductData.icon`), voir **Kingdom of God → Setup → Import Images**
   (`ImageImporter.cs`) : dépose des fichiers "image <Nom>.png/.jpg" dans
   un dossier et lance l'import. 16 premières images reçues et assignées à
   la main (pas d'Éditeur Unity ici pour lancer l'outil lui-même, donc
   import/`.meta`/assignation reproduits par script en suivant exactement
   le même format que produirait `ImageImporter.cs`) —
   `Assets/_Project/Art/Sprites/` : 6 portraits de Leaders (Abraham,
   Moïse, Josué, Débora, David, Gédéon) et 4 icônes de Bâtiments (Autel de
   Pierres, Tente Familiale, Tabernacle, Marché). Les 6 dernières
   (`Terrain_Desert/Plaine/Ruines/Cote/Colline/Marais.jpg`) n'allaient
   dans aucun des 7 types ci-dessus — ce sont des tuiles de terrain
   hexagonal, pas des portraits/icônes de fiche — donc `HexGridRenderer`
   a été étendu plutôt que de les laisser inutilisées : un nouveau
   `TerrainTileSet` (`Assets/_Project/ScriptableObjects/Grid/
   TerrainTileSet.asset`, un `TerrainType` → `Texture2D`) fournit une
   texture par tuile quand une existe, avec repli sur la couleur plate
   UITheme d'origine sinon (donc `Plain`/`Mountain`/`Forest`, non
   couverts par les 6 images reçues, restent en couleur). Le maillage de
   tuile (`AppendHex`) porte désormais des UV (chaque sommet = sa
   position sur le cercle unité, remappée en [0,1] — l'éventail hexagonal
   n'échantillonne jamais les coins du carré UV, donc le padding
   noir/blanc autour de chaque illustration n'est simplement jamais
   visité), et `CreateTexturedMaterial` bascule le shader plat existant
   sur `_BaseMap`/`_MainTex` au lieu d'une couleur unie. Un deuxième lot de
   10 icônes de Bâtiments a été reçu et assigné de la même façon (guid fixe
   + `.meta` reproduits à la main) : Campement de Caravane, Mine, Citerne
   de Siège, Scierie, Autel d'Ébal, Autel du Sinaï, Camp de Guilgal, Champs
   de Canaan, Forteresse et Ferme — portant le total à 14 des 39 bâtiments
   illustrés. Les 5 portraits d'`AntagonistData` (Pharaon, Goliath,
   Jézabel, Sennachérib, Sanballat et Tobija) ont ensuite été reçus et
   assignés de la même façon — `AntagonistCodexUI` affichait déjà
   `AntagonistData.portrait` génériquement, donc aucun changement de code
   n'était nécessaire, seulement l'import. Un troisième lot a suivi : 11
   icônes de Bâtiments supplémentaires (Aire à Battre le Blé, Autel de
   l'Éternel-Paix, Camp des Trois Cents, Refuge des Collines, Atelier de
   Charpentiers, École des Scribes, Fonderie, Palais Royal, Tribunal,
   Réservoir, École des Prophètes — total 25 des 39 bâtiments illustrés)
   et 5 icônes d'`ArtifactData` (Bâton d'Abraham, Anneau de Pharaon,
   Arche d'Alliance, Armure de Saül, Butin d'Aï) — même procédé, aucun
   changement de code requis pour ni l'un ni l'autre puisque
   `BuildingPaletteUI`/`CollectionUI` lisaient déjà leurs champs `icon`
   génériquement. Un quatrième lot a suivi, entièrement des icônes
   d'`ArtifactData` (16 : Couteau du Sacrifice, Épée de Goliath, Éphod de
   Gédéon, Ustensiles du Temple, Urne de la Manne, Trône de Salomon,
   Truelle de Néhémie, Torches et cruches de Gédéon, Tente de la
   Promesse, Tables de la Loi (nouvelles), Tables de la Loi (brisées),
   Serpent d'airain (détruit), Sceau royal d'Ézéchias, Rouleaux de la Loi
   (Esdras), Plans du Temple, Pierre de Gilgal — total 21 des 45
   artefacts illustrés), même procédé, aucun changement de code requis.
   Un cinquième lot de 16 images a suivi : 14 nouvelles icônes
   d'`ArtifactData` (Bâton d'Élisée, Char de feu d'Élie, Cheveux de
   Samson, Manteau d'Élie, Harpe de David, Lettres de Sennachérib,
   Mâchoire d'âne de Samson, Corne de bélier de Jéricho, Marteau et pieu
   de Jaël, Muraille reconstruite, Frondes de David, Fournaise de
   Babylone, Fosse aux lions, Huile de la veuve — total 35 des 45
   artefacts illustrés), mais aussi 2 vrais doublons repérés et exclus
   avant import (mêmes dimensions, ré-encodage différent seulement) :
   une image de la Tente de la Promesse déjà assignée au round
   précédent, et une image du Couteau du Sacrifice déjà assignée deux
   lots plus tôt — confirmant que certaines images étaient effectivement
   renvoyées une seconde fois. Un sixième lot de 7 images a suivi : 6
   nouvelles icônes d'`ArtifactData` (Arche d'Alliance (Jérusalem),
   Couronne de David, Couteau de silex (circoncision), Décret de Cyrus,
   Manteau de Joseph, Menorah du Tabernacle — total 41 des 45 artefacts
   illustrés), et à nouveau 1 doublon détecté par hash MD5 identique
   (donc pas un simple ré-encodage cette fois, mais le même fichier
   binaire) : une seconde image du Bâton d'Élisée, déjà assignée au
   round précédent. Sur demande explicite, cette image du bâton a
   finalement été réutilisée telle quelle comme icône du Bâton de Moïse
   (`Artifact_BatonDeMoise.asset`, même fichier PNG que
   `Image_BatonDElisee.png`, nouveau guid dédié) — total 42 des 45
   artefacts illustrés. Sur une demande similaire, l'illustration déjà
   utilisée pour `Building_Tabernacle.asset` (vue d'ensemble du camp
   avec le voile d'entrée du parvis) a été réutilisée comme icône du
   Voile du Tabernacle (`Artifact_VoileDuTabernacle.asset`, même fichier
   `Image_Tabernacle.jpg`, nouveau guid dédié) — total 43 des 45
   artefacts illustrés. Un dernier envoi de 2 images (« Nehemiah's
   Sword » et un autel de pierre sur une montagne éclairée d'un rayon de
   lumière) a fourni les 2 icônes manquantes — Épée de Néhémie et Autel
   du Carmel — portant le total à **45 des 45 artefacts illustrés**,
   catégorie désormais complète. Un lot de 16 rendus isométriques
   « diorama » de bâtiments a suivi, envoyés en réponse à une demande de
   modèles 3D — mais ce sont des images 2D (mêmes formats PNG/JPG que
   les icônes précédentes), pas des `GameObject`/maillages Unity, donc
   `BuildingData.prefab` (qui attend un vrai modèle 3D instanciable)
   reste vide pour les 39 bâtiments. En revanche 6 de ces 16 images
   correspondaient à des bâtiments sans icône : Camp des Exilés,
   Campement des Tribus, Chantier de la Muraille, Grenier, Citerne du
   Désert, Marché Royal — portant le total à 20 des 39 icônes de
   bâtiments. Les 10 autres images (Autel de Pierres, Autel de
   l'Éternel-Paix, Campement de Caravane, Citerne de Siège, Ferme,
   Fonderie, Camp de Guilgal, Champs de Canaan, et deux rendus sans
   correspondance exacte avec un bâtiment existant) montraient des
   bâtiments déjà pourvus d'une icône — elles n'ont pas été
   réimportées.
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
   condition. `MissionData.playerUnits`/`enemyUnits` sont désormais
   composés pour les 8 missions de combat plutôt que systématiquement
   vides — voir `Battle/` ci-dessus pour le détail des 8 rosters —
   donc l'escadron générique de repli (3 `Unit_Fantassin`) ne sert plus
   qu'à une future mission de combat qui n'aurait pas encore la sienne.
   Les valeurs de
   coût/seuil/option des 26 missions non-Sandbox sont plausibles au vu
   du récit de chaque
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
