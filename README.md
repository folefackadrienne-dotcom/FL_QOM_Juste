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
2. Laisser Unity réimporter les packages (`Packages/manifest.json`).
3. Lancer **Kingdom of God → Setup → Create All Scenes** dans le menu
   Unity pour générer et câbler `Bootstrap`, `MainMenu`, `Kingdom` et
   `Battle` d'un coup — voir
   [`Assets/_Project/Scenes/README.md`](Assets/_Project/Scenes/README.md)
   pour le détail de ce que chaque scène contient.
4. Ouvrir `Bootstrap` et lancer Play : ça enchaîne automatiquement sur
   `MainMenu`, où « Nouvelle Partie » ouvre `Kingdom` avec la barre de
   ressources déjà vivante (Foi, Blé, Eau… avec leurs valeurs de départ).

## Structure

```
Assets/_Project/
  Scripts/
    Core/          GameManager, cycle des 7 Âges
    Resources/      Blé, Eau, Bois, Or, Foi, Sagesse, Justice
    Grid/           Grille hexagonale (coordonnées axiales, cellules)
    Buildings/      Bâtiments, placement, Temple (niveaux 1-5,
                    TempleSystem.LevelUpgraded)
    Population/     Population & Loyauté (PopulationChanged,
                    LoyaltyLow/LoyaltyCritical)
    Battle/         Batailles tactiques tour par tour, unités, boss
                    (AntagonistData, lié à sa fiche UnitData via le
                    champ optionnel UnitData.antagonist)
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
                    (MissionStarted/MissionCompleted)
    Progression/    Leaders légendaires (LeaderManager : débloqués +
                    leader actif), arbre technologique (3 arbres × 5
                    branches : Économique, Militaire, Spirituel ;
                    TechTree.TechUnlocked)
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
    UI/             HUD, menu de prière, journal des versets
  Editor/         ProjectSceneSetup.cs — génère les 4 scènes de base
                  (menu Kingdom of God > Setup), assembly Editor-only
  ScriptableObjects/  Assets de données créés dans l'Éditeur
                      (Buildings, Units, Miracles, Verses, Artifacts,
                      Missions, Leaders, Antagonists, Techs, Ages,
                      Monetization, Audio) — les 7 Âges sont remplis
                      (Buildings, Verses, Artifacts, Missions, Miracles),
                      plus les 10 leaders, 5 antagonistes majeurs, les 93
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
                      Narrateur/Personnages)
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
   les scènes une fois le rendu visuel branché — le contenu narratif
   (Buildings/Verses/Artifacts/Missions/Miracles/Leaders/Antagonists/
   Techs, y compris les bâtiments génériques Ferme/Scierie/Mine/Marché/
   Grenier/Réservoir/Grand Marché/Atelier de Charpentiers/Fonderie/
   Tribunal) est fait pour les 7 Âges.
3. Brancher `HexGrid`/`BattleGrid` à un rendu visuel (tilemap ou mesh
   hexagonal) dans la scène `Kingdom`.
4. Habiller visuellement le menu de prière et le journal des versets
   (`PrayerMenuUI`/`VerseJournalUI` ont leur logique et leurs panneaux
   générés, mais pas encore de liste d'items/boutons par miracle ou verset).
   Le rituel complet de prière (`MiracleManager.BeginPrayer` /
   `AdvancePrayerTurn` / `AccelerateWithFaith` / `InterruptPrayer`) est déjà
   branché côté `Battle` via `TurnController` ; `PrayerMenuUI` utilise pour
   l'instant `TryCast` (résolution instantanée) faute de boucle de tours
   explicite côté `Kingdom` — à rebrancher sur le rituel complet une fois
   cette boucle ajoutée.
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
   les fichiers son.
