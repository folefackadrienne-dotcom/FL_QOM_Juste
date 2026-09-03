# Faith Run

Un jeu de plateforme/course inspiré de Super Mario qui intègre la Bible et le discipolat : le joueur incarne un disciple qui traverse 20 chapitres de l'histoire biblique, recueille des versets sur son chemin, évite les obstacles ("tentations"), et rencontre à la fin de chaque niveau un enseignement de discipolat.

## Jouer

Ouvre `game/faith-run-hub.html` — l'écran d'accueil liste les 20 chapitres et lance le niveau choisi.

```bash
open game/faith-run-hub.html      # macOS
xdg-open game/faith-run-hub.html  # Linux
```

Fichiers HTML autonomes (aucune dépendance, aucun appel réseau), à garder dans le même dossier : le hub et les écrans de fin de niveau se lient entre eux par chemins relatifs.

**Mécaniques communes à tous les niveaux :**
- Déplacement (← → ou A/D) et saut (Espace / ↑) façon plateformer, avec boutons tactiles sur mobile.
- Effets visuels (poussière, particules de collecte, tremblement d'écran) et sons synthétisés en direct (Web Audio), avec bouton muet dans le HUD.
- Vies : 3 cœurs de départ, 5 maximum, regagnés par palier de versets (tous les 2), par repos (14s sans dégât), ou via un cœur caché à trouver.
- 5 versets bibliques par niveau (Louis Segond, 1910, domaine public), un dialogue de discipolat à l'arrivée, un lien vers le chapitre suivant.

## Les 20 chapitres

| # | Fichier | Chapitre | Référence de clôture |
|---|---|---|---|
| 1 | `faith-run-eden.html` | Éden | Jean 15:5 |
| 2 | `faith-run-exodus.html` | L'Exode | Exode 33:14 |
| 3 | `faith-run-canaan.html` | La Terre promise | Josué 1:9 |
| 4 | `faith-run-judges.html` | Les Juges — Gédéon | Juges 7:7 |
| 5 | `faith-run-ruth.html` | Ruth | Ruth 1:16 |
| 6 | `faith-run-goliath.html` | David et Goliath | 1 Samuel 17:47 |
| 7 | `faith-run-david-repentance.html` | Le Roi David | Psaume 51:12 |
| 8 | `faith-run-elie.html` | Élie | 1 Rois 19:12 |
| 9 | `faith-run-daniel-lions.html` | Daniel dans la fosse | Daniel 6:23 |
| 10 | `faith-run-fournaise.html` | La fournaise ardente | Daniel 3:25 |
| 11 | `faith-run-jonas.html` | Jonas et Ninive | Jonas 3:10 |
| 12 | `faith-run-nehemie.html` | Néhémie reconstruit | Néhémie 6:3 |
| 13 | `faith-run-esther.html` | Esther | Esther 4:14 |
| 14 | `faith-run-bon-berger.html` | Le Bon Berger | Psaume 23:4 |
| 15 | `faith-run-nativite.html` | La Nativité | Luc 2:11 |
| 16 | `faith-run-tentation.html` | Le baptême et le désert | Matthieu 4:4 |
| 17 | `faith-run-galilee.html` | Paraboles et miracles | Marc 4:39 |
| 18 | `faith-run-croix.html` | La croix | Jean 19:30 |
| 19 | `faith-run-resurrection.html` | La résurrection | Matthieu 28:6 |
| 20 | `faith-run-pentecote.html` | La Pentecôte | Actes 1:8 |

## Architecture

Les niveaux 1-3 sont entièrement écrits à la main. Les niveaux 4-20 sont **générés** depuis un moteur commun et une table de données par `tools/build_levels.py` — sinon dupliquer ~900 lignes de moteur (physique, particules, audio, cœurs, HUD) 17 fois n'aurait pas été gérable. Chaque niveau généré varie par ses données : palette, versets, thème d'obstacle/décor (parmi 8 motifs réutilisables : pousse, feu, structure, eau, montagne, étoile, créature, croix), et texte des dialogues — le contenu biblique reste rédigé individuellement pour chaque chapitre.

```bash
python3 tools/build_levels.py   # régénère game/faith-run-<slug>.html pour les niveaux 4-20
python3 tools/build_hub.py      # régénère game/faith-run-hub.html à partir de la liste des niveaux
```

Pour ajouter un chapitre : ajouter une entrée à `LEVELS` dans `tools/build_levels.py` (palette, verses, motif, dialogue...), relancer les deux scripts.

## Prochaines étapes possibles

- Système de progression (sauvegarde locale du niveau atteint / versets collectés).
- Rendre le déblocage des chapitres réel sur le hub (actuellement tous ouverts).
- Niveaux au-delà de la Pentecôte (les voyages missionnaires de Paul, l'Apocalypse...).
