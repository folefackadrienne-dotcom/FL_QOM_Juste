# Faith Run

Un jeu de plateforme/course inspiré de Super Mario qui intègre la Bible et le discipolat : le joueur incarne un disciple qui traverse des mondes bibliques (Éden, l'Exode, la Terre promise, les Évangiles, les Actes...), recueille des versets sur son chemin, évite les obstacles ("tentations"), et rencontre à la fin de chaque niveau un enseignement de discipolat.

## Prototypes jouables

- `game/faith-run-hub.html` — écran d'accueil : choix du chapitre, relie tous les niveaux entre eux.
- `game/faith-run-eden.html` — Niveau 1 : Éden.
- `game/faith-run-exodus.html` — Niveau 2 : l'Exode.
- `game/faith-run-canaan.html` — Niveau 3 : la Terre promise.

Fichiers HTML autonomes (aucune dépendance à installer, aucun appel réseau), à garder dans le même dossier : le hub et les écrans de fin de niveau se lient entre eux par chemins relatifs. Pour lancer le jeu :

```bash
open game/faith-run-hub.html      # macOS
xdg-open game/faith-run-hub.html  # Linux
```

Ou double-cliquer sur `faith-run-hub.html` pour l'ouvrir dans un navigateur, puis choisir un chapitre. Chaque niveau propose aussi un lien « ← Tous les niveaux » dans l'en-tête et, une fois terminé, un lien direct vers le chapitre suivant.

**Mécaniques communes aux trois niveaux :**
- Déplacement (← → ou A/D) et saut (Espace / ↑) façon plateformer, avec boutons tactiles sur mobile.
- Effets visuels (poussière, particules de collecte, tremblement d'écran) et sons synthétisés en direct (Web Audio, aucun fichier externe), avec bouton muet dans la barre HUD.
- Vies : 3 cœurs de départ, 5 maximum. Trois façons d'en regagner : un cœur bonus tous les 2 versets collectés, un cœur qui se restaure après 14 secondes sans dégât, et un cœur caché à trouver en explorant chaque niveau.
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

**Niveau 3 — la Terre promise :**
- 5 versets à collecter (Josué, Nombres 13), affichés en toast à la collecte.
- Obstacles : lances et un géant patrouilleur — 3 cœurs de vie.
- Le Jourdain à franchir par plateformes, puis approche des murailles de Jéricho, qui s'effondrent (tremblement d'écran, gravats) à l'arrivée.
- Arrivée : dialogue au pied de l'Arche et des murailles tombées (Josué 1:9) sur le courage face aux géants de la vie.

## Prochaines étapes possibles

- Niveau 4 : les Évangiles, puis les Actes.
- Système de progression entre niveaux (sauvegarde locale du niveau atteint / versets collectés), et déblocage réel des chapitres 4-5 sur le hub.
