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
[`docs/Economy.md`](docs/Economy.md)

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
    Buildings/      Bâtiments, placement, Temple (niveaux 1-5)
    Population/     Population & Loyauté
    Battle/         Batailles tactiques tour par tour, unités, boss
                    (AntagonistData)
    Miracles/       Miracles conditionnels (Foi, verset requis, choix moral)
    Alliance/       Jauge d'Alliance (0-100) & repentance
    Verses/         Mémorisation de versets (mini-jeu progressif)
    Collectibles/   Artefacts bibliques (Commun → Légendaire)
    Missions/       Définition & suivi des missions
    Progression/    Leaders légendaires, arbre technologique (3 arbres ×
                    5 branches : Économique, Militaire, Spirituel)
    SaveSystem/     Sauvegarde locale JSON (+ point d'extension cloud)
    Monetization/   Entitlements (gratuit/Édition Complète), catalogue de
                    produits, seam IAP (stub Éditeur en attendant le vrai
                    store)
    UI/             HUD, menu de prière, journal des versets
  Editor/         ProjectSceneSetup.cs — génère les 4 scènes de base
                  (menu Kingdom of God > Setup), assembly Editor-only
  ScriptableObjects/  Assets de données créés dans l'Éditeur
                      (Buildings, Units, Miracles, Verses, Artifacts,
                      Missions, Leaders, Antagonists, Techs, Ages,
                      Monetization) — les 7 Âges sont remplis (Buildings,
                      Verses, Artifacts, Missions, Miracles), plus les 6
                      leaders, 5 antagonistes majeurs et les 93 nœuds des
                      3 arbres technologiques (Techs/)
  Scenes/             Générées par Kingdom of God > Setup > Create All
                      Scenes — voir le README du dossier
  Prefabs/, Art/, Audio/
docs/
  GDD.md              Document de conception consolidé
  ArtDirection.md      Direction artistique détaillée
  Economy.md           Système économique : ressources, bâtiments,
                        population, commerce, arbres technologiques
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
2. Créer les `BuildingData` génériques décrits dans `docs/Economy.md`
   (Ferme, Puits, Scierie, Mine, Marché, Grenier, Réservoir…) — dernier
   contenu narratif notable encore manquant ; le reste (Buildings/Verses/
   Artifacts/Missions/Miracles/Leaders/Antagonists/Techs) est fait pour
   les 7 Âges.
3. Brancher `HexGrid`/`BattleGrid` à un rendu visuel (tilemap ou mesh
   hexagonal) dans la scène `Kingdom`.
4. Habiller visuellement le menu de prière et le journal des versets
   (`PrayerMenuUI`/`VerseJournalUI` ont leur logique et leurs panneaux
   générés, mais pas encore de liste d'items/boutons par miracle ou verset).
5. Créer des prefabs d'unités référençant les 6 `UnitData` de base
   (`Assets/_Project/ScriptableObjects/Units`) pour peupler la scène
   `Battle`.
6. Remplacer `EditorIAPService` par une vraie intégration store (Unity IAP
   ou équivalent) une fois les fiches produit créées dans App Store
   Connect / Play Console — voir `Assets/_Project/Scripts/Monetization`.
