# Faith Run

Un jeu de plateforme/course inspiré de Super Mario qui intègre la Bible et le discipolat : le joueur incarne un disciple qui traverse des mondes bibliques (Éden, l'Exode, la Terre promise, les Évangiles, les Actes...), recueille des versets sur son chemin, évite les obstacles ("tentations"), et rencontre à la fin de chaque niveau un enseignement de discipolat.

## Prototype jouable

`game/faith-run-eden.html` — Niveau 1 : Éden.

Fichier HTML autonome (aucune dépendance à installer). Pour le lancer :

```bash
open game/faith-run-eden.html      # macOS
xdg-open game/faith-run-eden.html  # Linux
```

Ou double-cliquer le fichier pour l'ouvrir dans un navigateur.

**Mécaniques du niveau 1 :**
- Déplacement (← → ou A/D) et saut (Espace / ↑) façon plateformer.
- 5 versets à collecter (Genèse 1–2, Jean 15), affichés en toast à la collecte.
- Obstacles : ronces (thorns) et un serpent patrouilleur — 3 cœurs de vie.
- Rivière à franchir par plateformes (nénuphars/pierres).
- Arrivée : dialogue avec l'Arbre de Vie (Jean 15:5) reliant Éden au discipolat en Christ, puis écran de fin de niveau.
- Textes bibliques : version Louis Segond (1910, domaine public).

## Prochaines étapes possibles

- Niveau 2 : l'Exode (mécanique de "plaies"/obstacles à esquiver, franchissement de la mer Rouge).
- Système de progression entre niveaux (sauvegarde locale du niveau atteint / versets collectés).
- Mode mobile tactile plus abouti (boutons déjà présents, à affiner).
