# Faith Run

Un jeu de plateforme/course inspiré de Super Mario qui intègre la Bible et le discipolat : le joueur incarne un disciple qui traverse des mondes bibliques (Éden, l'Exode, la Terre promise, les Évangiles, les Actes...), recueille des versets sur son chemin, évite les obstacles ("tentations"), et rencontre à la fin de chaque niveau un enseignement de discipolat.

## Prototypes jouables

- `game/faith-run-eden.html` — Niveau 1 : Éden.
- `game/faith-run-exodus.html` — Niveau 2 : l'Exode.

Fichiers HTML autonomes (aucune dépendance à installer, aucun appel réseau). Pour les lancer :

```bash
open game/faith-run-eden.html      # macOS
xdg-open game/faith-run-eden.html  # Linux
```

Ou double-cliquer le fichier pour l'ouvrir dans un navigateur.

**Mécaniques communes aux deux niveaux :**
- Déplacement (← → ou A/D) et saut (Espace / ↑) façon plateformer, avec boutons tactiles sur mobile.
- Effets visuels (poussière, particules de collecte, tremblement d'écran) et sons synthétisés en direct (Web Audio, aucun fichier externe), avec bouton muet dans la barre HUD.
- Textes bibliques : version Louis Segond (1910, domaine public).

**Niveau 1 — Éden :**
- 5 versets à collecter (Genèse 1–2, Jean 15), affichés en toast à la collecte.
- Obstacles : ronces et un serpent patrouilleur — 3 cœurs de vie.
- Rivière à franchir par plateformes (nénuphars/pierres).
- Arrivée : dialogue avec l'Arbre de Vie (Jean 15:5) reliant Éden au discipolat en Christ.

**Niveau 2 — l'Exode :**
- 5 versets à collecter (Exode 3, 13, 14, 20), affichés en toast à la collecte.
- Obstacles : impacts de grêle et un char de Pharaon patrouilleur — 3 cœurs de vie.
- Le Nil à franchir par plateformes, puis traversée de la mer Rouge (murs d'eau animés).
- Arrivée : dialogue avec la Colonne de Feu (Exode 33:14) reliant la délivrance d'Égypte à la confiance au jour le jour.

## Prochaines étapes possibles

- Niveau 3 : la Terre promise, puis les Évangiles et les Actes.
- Écran d'accueil / hub reliant les niveaux entre eux.
- Système de progression entre niveaux (sauvegarde locale du niveau atteint / versets collectés).
