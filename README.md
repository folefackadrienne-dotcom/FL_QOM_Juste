# Kingdom of God

Jeu de stratégie tour par tour et de gestion de royaume qui suit l'histoire
d'Israël dans l'Ancien Testament, sur PC et mobile.

Ce dépôt contient le **squelette du projet Unity** : l'architecture de code
des systèmes de jeu décrits dans le design (ressources, grille hexagonale,
bâtiments, batailles tactiques, miracles, Alliance, mémorisation de
versets, collection d'objets, missions, progression, sauvegarde), sans
contenu jouable complet — pas encore de scènes, d'art, ni de données de jeu
remplies.

Design de référence : [`docs/GDD.md`](docs/GDD.md) ·
[`docs/ArtDirection.md`](docs/ArtDirection.md)

## Prérequis

- Unity **2022.3.50f1 LTS** (voir `ProjectSettings/ProjectVersion.txt`)
- Universal Render Pipeline, TextMeshPro, Input System, Cinemachine, 2D
  Tilemap Extras (déclarés dans `Packages/manifest.json`, installés
  automatiquement à l'ouverture du projet)

## Ouvrir le projet

1. Ouvrir Unity Hub → Add → sélectionner ce dossier.
2. Laisser Unity réimporter les packages (`Packages/manifest.json`).
3. Créer les scènes de base — voir
   [`Assets/_Project/Scenes/README.md`](Assets/_Project/Scenes/README.md).

## Structure

```
Assets/_Project/
  Scripts/
    Core/          GameManager, cycle des 7 Âges
    Resources/      Blé, Eau, Bois, Or, Foi, Sagesse, Justice
    Grid/           Grille hexagonale (coordonnées axiales, cellules)
    Buildings/      Bâtiments, placement, Temple (niveaux 1-5)
    Population/     Population & Loyauté
    Battle/         Batailles tactiques tour par tour
    Miracles/       Miracles conditionnels (Foi, verset requis, choix moral)
    Alliance/       Jauge d'Alliance (0-100) & repentance
    Verses/         Mémorisation de versets (mini-jeu progressif)
    Collectibles/   Artefacts bibliques (Commun → Légendaire)
    Missions/       Définition & suivi des missions
    Progression/    Leaders légendaires, arbre technologique (3 branches)
    SaveSystem/     Sauvegarde locale JSON (+ point d'extension cloud)
    UI/             HUD, menu de prière, journal des versets
  ScriptableObjects/  Assets de données à créer dans l'Éditeur
                      (Buildings, Units, Miracles, Verses, Artifacts,
                      Missions, Leaders, Techs, Ages)
  Scenes/             Vide pour l'instant — voir le README du dossier
  Prefabs/, Art/, Audio/
docs/
  GDD.md              Document de conception consolidé
  ArtDirection.md      Direction artistique détaillée
```

Chaque système de gameplay est un composant indépendant (grid: `HexGrid`,
`ResourceManager`, `AllianceSystem`, etc.), assemblé par `GameManager`
(`Assets/_Project/Scripts/Core/GameManager.cs`) plutôt que retrouvé via
`FindObjectOfType`. Le contenu (bâtiments, unités, miracles, versets,
artefacts, missions, leaders, techs) est data-driven via des
`ScriptableObject` — créables depuis le menu `Create > Kingdom of God > ...`
dans l'Éditeur, sans toucher au code.

## Prochaines étapes suggérées

1. Créer les scènes `Bootstrap`, `Kingdom`, `Battle`, `MainMenu`.
2. Remplir les premiers `ScriptableObject` pour l'Âge 1 (Les Patriarches) :
   bâtiments de base, la mission "L'Appel d'Abraham", les versets et
   artefacts associés (voir `docs/GDD.md`).
3. Brancher `HexGrid`/`BattleGrid` à un rendu visuel (tilemap ou mesh
   hexagonal).
4. Implémenter le rendu du menu de prière et du journal des versets
   (`PrayerMenuUI`, `VerseJournalUI` ne sont que la logique, sans prefab
   UI pour l'instant).
