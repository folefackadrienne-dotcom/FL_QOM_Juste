# Kingdom of God — Game Design Document

Consolidated from the original design notes. This is the reference the code in
`Assets/_Project/Scripts` implements against.

## 1. High Concept

**Kingdom of God** est un jeu de stratégie tour par tour et de gestion de
royaume qui suit l'histoire d'Israël dans l'Ancien Testament. Le joueur
incarne un dirigeant (juge, roi ou prophète selon l'époque) et doit faire
prospérer le peuple tout en maintenant l'Alliance avec Dieu.

**Piliers de design :**
- Fun stratégique avant tout
- Éducation biblique intégrée naturellement
- Conséquences morales réelles
- Miracles puissants mais conditionnels
- Accessible sur PC et mobile

Le jeu mêle stratégie de construction/gestion, batailles tactiques, miracles
et interventions divines, mémorisation de versets, collection d'objets
bibliques, et découverte progressive de l'histoire sainte. Le joueur peut
échouer, se repentir et être restauré — c'est la mécanique centrale.

## 2. Les 7 Âges de la campagne

| # | Âge | Période | Thème | Difficulté |
|---|-----|---------|-------|------------|
| 1 | Les Patriarches | Abraham → Joseph | La promesse et la foi | Facile |
| 2 | L'Exode et le Désert | Moïse | Libération et Alliance | Moyenne |
| 3 | La Conquête | Josué | Prise de possession du pays | Moyenne+ |
| 4 | Les Juges | Débora, Gédéon, Samson... | Cycle péché-oppression-repentance-délivrance | Variable |
| 5 | La Monarchie Unifiée | Saül → David → Salomon | Le roi selon le cœur de Dieu | Élevée |
| 6 | Les Royaumes Divisés | Roboam → Chute de Samarie/Jérusalem | L'idolâtrie et les prophètes | Élevée |
| 7 | L'Exil et le Retour | Babylone → Esdras/Néhémie | Jugement, espérance, restauration | Très élevée |

Each age introduces new mechanics, buildings, units, miracles, verses and
artifacts, while following the Old Testament chronology. Sample missions per
age are listed in `docs/Missions.md`-equivalent detail inside this file's
source material; see the original notes for the full mission list (5 per
age) and per-age object/verse tables.

## 3. Gameplay Loop

1. Gestion de territoire (villes, ressources, temple, armée)
2. Événements bibliques (sécheresse, invasion, prophète, idolâtrie…)
3. Choix stratégiques + choix moraux
4. Batailles ou résolutions
5. Récompenses (ressources, objets, versets, bénédictions)

### Resources (7)

| Ressource | Utilité | Obtention | Particularité |
|---|---|---|---|
| Blé (Wheat) | Nourriture, population | Fermes, commerce, bénédictions | Pénurie = murmures |
| Eau (Water) | Population, agriculture, pureté | Puits, rivières, miracles | Critique dans le désert |
| Bois (Wood) | Construction, sièges | Forêts, commerce | - |
| Or (Gold) | Unités avancées, diplomatie, temple | Mines, tributs, commerce | - |
| Foi (Faith) | Miracles, moral, conversion | Prière, obéissance, prophètes, versets | Ressource centrale |
| Sagesse (Wisdom) | Technologies, décisions | Étude de la Torah, conseils de sages | Débloque des options |
| Justice | Loyauté du peuple, jugement | Jugements justes, application de la Loi | Influence les événements |

### Buildings

Habitat, Production, Militaire, Spirituel, Spéciaux (Arche d'Alliance,
Palais royal, Muraille). Placement sur grille hexagonale, avec bonus de
position et prérequis de Foi/Justice pour certains bâtiments. Le Temple a 5
niveaux, chacun augmentant le plafond de Foi et débloquant des miracles.

### Battles

Tour par tour sur grille hexagonale (inspiré d'Advance Wars / Fire Emblem
simplifié). Unités : Fantassin, Archer, Char, Cavalerie, Prêtre/Lévite
(soutien), Prophète (rare). Unités spéciales déblocables (Guerriers de
David, Hommes de Gédéon, Chérubins...). 1 miracle utilisable par bataille.
Conditions de victoire variables : anéantir l'ennemi, survivre X tours,
protéger un personnage, capturer un point.

### Miracles

Conditionnels : niveau de Foi minimum, parfois un verset mémorisé
spécifique, parfois un objet possédé, parfois un choix moral préalable.
Exemples : Mer Rouge, Chute de Jéricho, Soleil arrêté, Feu du Carmel, Manne,
Guérison.

### Verses (memorization mini-game)

Chaque mission majeure débloque 1 à 3 versets. Mini-jeu progressif :
lecture → compléter les trous → remettre dans l'ordre → quiz de contexte.
Récompense : bonus permanent + accès Bibliothèque de la Torah (Mode
Méditation).

### Collectibles

Artefacts majeurs, objets secondaires, reliques. Rareté : Commun, Rare,
Épique, Légendaire. ~18-20 légendaires, ~25-30 épiques au total. Chaque
objet a une fiche : texte biblique exact, contexte historique, commentaire
éducatif, effet de jeu, illustration.

### Alliance & Moralité

Jauge d'Alliance (0-100) : augmente par obéissance, justice, prière,
mémorisation de versets ; diminue par idolâtrie, injustice, orgueil,
alliances interdites. Haute Alliance → miracles plus puissants, événements
positifs. Basse Alliance → malédictions, invasions, rébellions. Le joueur
peut toujours se repentir (restauration avec coût).

### Progression & Metagame

Arbre de technologies en 3 branches (Militaire, Spirituelle, Civile).
Leaders légendaires déblocables (David, Salomon, Josué, Débora...) avec
talents uniques. Mode Libre (sandbox) après certains âges. New Game+ avec
défis (Ironman, Mode Prophète).

## 4. Monetization

Free-to-play + Premium soft: version gratuite = 2-3 premiers âges;
achat unique "Édition Complète"; cosmétiques et confort uniquement (pas de
pay-to-win); Battle Pass saisonnier léger. Pas de loot boxes agressives, pas
de timers punitifs.

## 5. Platform & Tech

- Moteur : Unity (PC + mobile)
- PC : souris + clavier, caméra libre, tooltips riches
- Mobile : tactile (glisser-déposer + menus contextuels), UI simplifiée
- Sauvegarde cloud (cross-play), mode hors-ligne complet

## 6. Target Audience

Chrétiens et juifs intéressés par l'Ancien Testament (12 ans+), familles,
curieux d'histoire biblique aimant la stratégie.

## 7. Where this lives in code

See `Assets/_Project/Scripts` — one folder per system (`Core`, `Resources`,
`Grid`, `Buildings`, `Population`, `Battle`, `Miracles`, `Alliance`,
`Verses`, `Collectibles`, `Missions`, `Progression`, `SaveSystem`, `UI`).
Design-time content (buildings, units, miracles, verses, artifacts,
missions, leaders, tech nodes) is authored as ScriptableObject assets under
`Assets/_Project/ScriptableObjects`.
