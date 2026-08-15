# Kingdom of God — Système Économique

Référence de conception pour l'économie du jeu. Les 3 arbres technologiques
qu'elle décrit (Économique, Militaire, Spirituel — 5 branches chacun,
93 technologies au total) sont implémentés comme données dans
`Assets/_Project/ScriptableObjects/Techs/` (voir
`Assets/_Project/Scripts/Progression/TechNode.cs`). Ce document couvre le
reste : les ressources, les bâtiments de production, la population, le
commerce, les événements dynamiques, et le lien entre économie et
spiritualité.

L'économie est un pilier du jeu : elle doit être **lisible**,
**stratégique**, et **étroitement liée** à la dimension spirituelle (Foi,
Justice et Alliance).

## 1. Les Ressources Principales

| Ressource | Icône / Couleur | Production principale | Utilisations principales | Particularité |
|---|---|---|---|---|
| **Blé** | Or / Jaune | Fermes, champs | Nourrir la population, recruter, certains miracles | Pénurie = murmures et baisse de loyauté |
| **Eau** | Bleu | Puits, citernes, rivières | Population, agriculture, pureté rituelle | Critique dans le désert et les sièges |
| **Bois** | Marron | Scieries, forêts | Construction, machines de siège, feux | — |
| **Or** | Doré | Mines, commerce, tributs, impôts | Unités avancées, diplomatie, merveilles, Temple | Ressource de prestige |
| **Foi** | Blanc / Lumière dorée | Autels, Temple, prière, versets, obéissance | Miracles, moral, conversion, événements positifs | Ressource la plus importante |
| **Sagesse** | Bleu nuit / Violet | Écoles de prophètes, étude, conseillers | Technologies, meilleures options de dialogue, lois | Débloque des choix stratégiques |
| **Justice** | Pourpre | Jugements justes, application de la Loi, prophètes | Loyauté du peuple, stabilité, bénédictions | Très liée à l'Alliance |

## 2. Production et Bâtiments Économiques

**Bâtiments de base :**
- Ferme / Champ → Blé
- Puits / Citerne → Eau
- Scierie → Bois
- Mine → Or
- Marché → convertit des ressources + génère un peu d'Or
- Grenier → augmente la capacité de stockage de Blé
- Réservoir → augmente le stockage d'Eau

**Bâtiments avancés :**
- Grand Marché / Comptoir commercial : permet le commerce avec les nations étrangères
- Atelier de charpentiers : améliore l'efficacité du Bois
- Fonderie : améliore la production d'Or et permet de meilleures armes
- École de scribes / École de prophètes : produit de la Sagesse
- Tribunal : génère de la Justice (si le joueur rend de bons jugements)

**Bâtiment ultime — le Temple (niveaux 1 à 5)** : consomme beaucoup d'Or,
de Bois et de main-d'œuvre ; produit énormément de Foi ; débloque des
miracles plus puissants ; attire des pèlerins (bonus de population et
d'Or). Voir `Assets/_Project/Scripts/Buildings/TempleSystem.cs` —
`TempleSystem.levels` (niveaux 2 à 5, le niveau 1 étant le départ gratuit)
est désormais peuplé par `ProjectSceneSetup.SetTempleLevels` : coûts
croissants en Bois/Or dès le niveau 2, plus Justice/Sagesse à partir du
niveau 3, et le niveau 5 demande aussi de la Foi elle-même (atteindre le
plafond de Foi coûte une partie de la Foi qu'il va libérer). `CanUpgrade`/
`TryUpgrade` existaient déjà mais `levels` était une liste vide, donc
inatteignables quoi qu'il arrive ; `TempleUI` (widget permanent du HUD
`Kingdom`, pas un panneau à ouvrir) affiche le niveau actuel et un bouton
« Améliorer » actif dès que l'achat est possible. Le lien vers
`miraclesUnlocked` par niveau n'est pas fait — `MiracleData` n'a aucun
champ de niveau de Temple à faire correspondre, et le construire
demanderait de choisir quels miracles vont à quel niveau parmi tous ceux
déjà créés ; laissé pour un futur passage de conception dédié plutôt que
deviné ici.

*Note : ces bâtiments de base sont génériques (utilisables dès l'Âge 1) et
distincts des bâtiments thématiques déjà créés par âge dans
`Assets/_Project/ScriptableObjects/Buildings/` (ex. Aire à Battre le Blé,
Citerne de Siège). Créés comme `BuildingData` : Ferme, Scierie, Mine,
Marché, Grenier, Réservoir, Grand Marché (Comptoir commercial), Atelier de
Charpentiers, Fonderie, Tribunal. Puits et École de scribes/prophètes ne
sont pas dupliqués : `Building_Puits` (Âge 1) et `Building_EcoleDesScribes`/
`Building_EcoleDesProphetes` (Âges 5-6) remplissent déjà ce rôle. Grenier
et Réservoir utilisent le nouveau champ `storageCapacityBonus` de
`BuildingData` (appliqué une fois à la pose via `BuildingManager.TryPlace`),
le seul mécanisme d'augmentation de capacité de stockage qui existait
jusqu'ici étant celui, dédié, du Temple (`TempleSystem`).*

## 3. Population, Loyauté et Économie

La Population est à la fois une ressource et un indicateur.
- Chaque habitant consomme du Blé et de l'Eau.
- Une population élevée augmente la production globale, mais aussi les
  risques de murmures si les besoins ne sont pas satisfaits.
- La Loyauté (0-100 %) influence directement la productivité :
  - Haute loyauté → +20 à +40 % de production
  - Basse loyauté → grèves, sabotage, baisse de production, risque de révolte
- La Justice et la Foi sont les meilleurs moyens de maintenir une haute
  loyauté sur le long terme.

Voir `Assets/_Project/Scripts/Population/PopulationSystem.cs`. Ce système
existait déjà (jauge de Loyauté, seuils de murmures/rébellion), mais rien ne
l'actionnait : `BuildingManager.ProcessTurnProduction`, `PopulationSystem.
Grow` et la conséquence d'une pénurie sur la Loyauté étaient de vraies
méthodes sans aucun appelant nulle part — `Kingdom` n'avait aucune notion de
tour. `KingdomTurnManager` (nouveau, `Assets/_Project/Scripts/Core/`) est ce
tour manquant, déclenché par le bouton « Fin de Tour » du HUD :
1. `BuildingManager.ProcessTurnProduction` applique la production de chaque
   bâtiment posé, multipliée par `PopulationSystem.ProductionMultiplier` —
   une formule à paliers directement sur les seuils déjà existants
   (`rebellionThreshold`/`murmurThreshold`/`productionBonusThreshold`) :
   ×0.5 en dessous du seuil de rébellion, ×0.7 en dessous du seuil de
   murmures, ×1.2 à ×1.4 (linéaire) au-dessus du seuil de bonus, ×1 sinon —
   les « +20 à +40 % »/« baisse de production » ci-dessus, chiffrés.
2. La population consomme `Population × 0.05` Blé et autant d'Eau (voir note
   de chiffrage ci-dessous). Nourrie : la Loyauté remonte légèrement (+2) et
   la population croît de 2 % par tour, plafonnée par sa capacité de
   logement (`PopulationSystem.Capacity`, base 100, augmentée une fois à la
   pose par `BuildingData.populationCapacityBonus` sur les 5 bâtiments
   d'Habitat — Tente Familiale, Campement des Tribus, Camp de Guilgal,
   Refuge des Collines, Camp des Exilés — qui ne servaient jusqu'ici à
   rien). En pénurie : Loyauté −5, rien n'est dépensé.
3. « La Justice et la Foi sont les meilleurs moyens de maintenir une haute
   loyauté » (ci-dessus), désormais chiffré : si les réserves de Justice
   *et* de Foi dépassent chacune 0,5 par habitant, la Loyauté remonte
   encore de +1 — un second bonus indépendant du premier, qui se cumule
   avec lui. Volontairement asymétrique : aucune pénalité miroir n'existe
   pour un royaume qui néglige Justice/Foi. Les bâtiments produisant de la
   Justice (Autel d'Ébal, Tribunal…) ne se débloquent qu'à partir de l'Âge
   2 — une pénalité symétrique aurait donc puni tous les joueurs pour un
   manque que le système d'Âges lui-même rend inévitable en tout début de
   partie, pas un vrai choix. La pénurie de Blé/Eau (point 2) couvre déjà
   la négligence pure ; ce bonus ne fait que s'y ajouter quand il
   s'applique, jamais s'y soustraire.

**Un bug réel corrigé au passage** : avant ce round, `PopulationSystem.
ModifyLoyalty` n'était jamais appelé qu'en négatif (nulle part dans le code
ne l'appelait en positif) — la moindre pénurie devenait donc un cliquet à
sens unique vers 0 % de Loyauté pour toujours, sans jamais pouvoir remonter.
Une simulation (voir note de chiffrage) l'a révélé avant que ça n'atterrisse
en jeu : la nouvelle récompense « bien nourri » (+2 Loyauté/tour) est le
correctif.

**Note de chiffrage** : 0,05 Blé/Eau par habitant et par tour a été choisi
(plutôt qu'un chiffre rond comme 0,1) après simulation en Python de 420
tours contre les 39 `BuildingData` déjà chiffrés — il n'existe que 3
bâtiments producteurs de Blé et 3 d'Eau sur les 7 Âges (~17/tour chacun au
complet), donc une population qui grossit jusqu'à la capacité de logement
maximale (base 100 + les 5 bonus d'Habitat = 260) a besoin d'un taux par
habitant assez bas pour rester nourrissable par ce nombre fixe de bâtiments.
0,1 provoquait une pénurie permanente dès que la population dépassait ~170 ;
0,05 laisse une marge saine (~13 Blé/Eau nécessaires à capacité pleine
contre ~17 produits). Simulation non jouée en vrai dans l'Éditeur (aucun
Éditeur Unity disponible dans cet environnement), donc à confirmer/ajuster
en playtest réel.

## 4. Commerce et Diplomatie Économique

Le joueur peut commercer avec les nations voisines (Philistins, Égypte,
Tyr, Sidon, Assyrie, Babylone…) et les caravanes de passage.

**Mécaniques de commerce :**
- Routes commerciales (doivent être protégées)
- Traités commerciaux (bonus permanents, mais parfois dangereux moralement)
- Tributs (reçus ou payés)

**Attention :** certaines alliances commerciales (surtout avec des nations
idolâtres) peuvent faire baisser la jauge d'Alliance.

## 5. Événements Économiques Dynamiques

L'économie n'est jamais complètement stable.

**Positifs :** année de pluie abondante (+Blé), découverte d'une veine
d'or, caravane généreuse, bénédiction de la manne (Âge 2).

**Négatifs :** sécheresse, sauterelles, invasion de bandits sur les routes
commerciales, grève ou murmures du peuple, malédiction liée à l'idolâtrie
(production fortement réduite).

## 6. Spécificités Économiques par Âge

| Âge | Particularité économique principale | Défi principal |
|---|---|---|
| 1 – Patriarches | Économie pastorale et nomade | Gérer les déplacements et les puits |
| 2 – Exode | Pénurie extrême + Manne | Survie dans le désert |
| 3 – Conquête | Butin de guerre + répartition des terres | Gérer le butin sans tomber dans la convoitise |
| 4 – Juges | Économie instable (cycles d'oppression) | Reconstruire après chaque oppression |
| 5 – Monarchie | Apogée économique (surtout sous Salomon) | Gérer l'abondance sans orgueil |
| 6 – Division | Deux économies séparées + tributs étrangers | Résister aux pressions assyriennes/babyloniennes |
| 7 – Exil/Retour | Économie de reconstruction sous contrainte | Construire avec très peu de moyens |

## 7. Lien entre Économie et Spiritualité

L'un des aspects les plus importants du jeu :
- Une économie florissante **sans Justice ni Foi** finit par générer
  orgueil, oppression des pauvres et idolâtrie.
- À l'inverse, un peuple fidèle peut recevoir des bénédictions
  économiques inattendues (manne, cailles, pluie, etc.).
- Le joueur peut choisir de prélever des impôts lourds (efficace à court
  terme) ou d'appliquer des principes plus justes (meilleure loyauté à
  long terme).
- L'année sabbatique et le jubilé existent comme mécaniques avancées
  (lâcher prise économique en échange de bénédictions) — voir les
  technologies `eco_agri_annee_sabbatique` et `eco_adm_jubile`.

## 8. Stockage et Limites

- Chaque ressource a une capacité de stockage limitée (augmentée par des
  bâtiments).
- Dépasser la capacité = gaspillage.
- En cas de siège, les stocks deviennent vitaux.

## Les 3 arbres technologiques

Chaque technologie a un nom biblique ou historique, un effet concret, un
âge conseillé, et est reliée à la précédente de sa branche par
`prerequisiteIds` (référence par identifiant texte, pas par objet — voir
la note dans `TechNode.cs` sur pourquoi). Certaines ont en plus une
condition qualitative (`additionalRequirement`, ex. « Justice élevée »,
« Haute Alliance ») non encore branchée sur le code, gardée comme donnée
de conception.

### Arbre Économique (31 technologies, préfixe `eco_`)
1. **Agriculture & Alimentation** (`eco_agri`) — Irrigation primitive → Charrue de bois → Greniers renforcés → Assolement → Vignes et oliviers → Année sabbatique
2. **Eau & Survie** (`eco_eau`) — Puits profonds → Citernes → Canaux d'irrigation → Aqueducs → Gestion de la sécheresse
3. **Artisanat & Industrie** (`eco_art`) — Travail du bois avancé → Métallurgie du bronze → Forges → Métallurgie du fer → Artisanat de luxe → Chantiers navals
4. **Commerce & Richesse** (`eco_com`) — Routes caravanières → Marchés organisés → Traités commerciaux → Système de poids et mesures → Monnaie royale → Ports et commerce maritime (nécessite aussi Chantiers navals) → Tributs organisés
5. **Administration & Économie Sacrée** (`eco_adm`) — Recensement → Scribes royaux → Lois économiques justes → Administration du Temple → Dîme organisée → Jubilé ; Reconstruction sous contrainte (débloquée automatiquement à l'Âge 7)

### Arbre Militaire (30 technologies, préfixe `mil_`)
1. **Infanterie & Recrutement** (`mil_inf`) — Organisation tribale → Entraînement de base → Boucliers renforcés → Unités d'élite → Conscription organisée → Garde royale
2. **Tir & Support** (`mil_tir`) — Arcs simples → Arcs composites → Frondeurs entraînés → Tir de précision → Archers de soutien
3. **Mobilité — Cavalerie & Chars** (`mil_mob`) — Domestication des chevaux → Chars de combat → Cavalerie lourde → Tactiques de harcèlement → Chars renforcés
4. **Fortifications & Siège** (`mil_for`) — Palissades → Murailles de pierre → Tours de guet → Portes fortifiées → Machines de siège → Ingénierie de siège avancée → Contre-siège
5. **Doctrine & Guerre Sacrée** (`mil_doc`) — Cri de guerre → Formation disciplinée → Guerre sainte (Anathème) → Prière avant le combat → Chapitres militaires → Confiance en l'Éternel → Reste fidèle

### Arbre Spirituel (32 technologies, préfixe `spi_`)
1. **Foi & Vie Spirituelle** (`spi_foi`) — Autels de pierre → Prière quotidienne → Jeûne → Louange et cantiques → Méditation de la Torah → Cœur affermi → Reste fidèle
2. **Miracles & Puissance Divine** (`spi_mir`) — Signes et prodiges → Intercession → Miracles de délivrance → Miracles de jugement → Double portion → Présence manifeste
3. **Prophètes & Révélation** (`spi_pro`) — Écoute de la voix → École de prophètes → Discernement → Prophétie de jugement → Prophétie de consolation → Esprit de révélation
4. **Alliance & Sainteté** (`spi_all`) — Circoncision → Purification → Application de la Loi → Crainte de l'Éternel → Cœur non partagé → Alliance renouvelée
5. **Temple & Culte** (`spi_tem`) — Tabernacle → Organisation lévitique → Sacrifices réguliers → Plans du Temple → Temple de Salomon → Culte purifié ; Second Temple (débloqué à l'Âge 7)

### Mécaniques transversales

- La Sagesse est la ressource principale pour rechercher des technologies,
  dans les trois arbres.
- Les technologies spirituelles, et la branche Doctrine & Guerre Sacrée de
  l'arbre militaire, coûtent aussi de la Foi.
- Plusieurs technologies ont des versions améliorées (ou, à l'inverse, des
  effets négatifs) selon le niveau de la jauge d'Alliance — non encore
  implémenté en code.

### Coûts

Les coûts de recherche (`cost`) n'étaient pas chiffrés dans le document
source ; ils suivent une formule simple et transparente plutôt que des
valeurs inventées au cas par cas, où le palier est la position de la
technologie dans sa branche (1 pour la première, 2 pour la suivante,
etc.) : `8 + (palier - 1) × 6` Sagesse pour toutes, plus `8 + (palier -
1) × 6` Foi en plus pour les technologies spirituelles et celles de la
branche Doctrine & Guerre Sacrée (coût cumulé, donc, pour ces deux
groupes de branches — deux ressources à réunir, pas une seule).

**Chiffrage validé, et corrigé une fois** : la formule Sagesse d'origine
(`10 + (palier - 1) × 8`, plus chère) a été simulée en Python sur 420
tours contre l'économie réelle — bâtiments, missions et Temple, tous
committés aux rounds précédents — et laissait 10 des 93 technologies
définitivement hors de portée, quelle que soit la stratégie de jeu
testée (gloutonne, orientée branche, avec ou sans priorité sur le
Temple). Cause : seuls 3 des 39 `BuildingData` produisent de la Sagesse
(École des Scribes, École des Prophètes, Palais Royal), et aucun avant
l'Âge 4 — la Sagesse reste donc bloquée à sa valeur de départ (5) tout
au long des Âges 0 à 3, la recherche ne progressant que par à-coups
grâce aux récompenses ponctuelles de missions (≈ 80 Sagesse cumulées sur
ces 4 Âges, toutes missions confondues). Un rythme de recherche lent en
tout début de partie colle au thème de la Sagesse comme ressource rare
(GDD : « débloque des choix stratégiques »), mais 10 technologies
jamais atteignables même en fin de partie, elles, ne collent à rien —
c'est un vrai défaut de calibrage, pas un choix de rythme. La formule
allégée (alignée sur la forme déjà utilisée pour la Foi) a été revalidée
sur la même simulation : les 93 sont désormais atteignables, la
dernière autour du tour 408/420 — un arbre qui se termine tout juste en
fin de partie pour un joueur qui s'y consacre, plutôt que jamais.

`TechTree.allNodes` était par ailleurs une liste vide — `CanUnlock`/
`TryUnlock` sont du code réel et correct (ils résolvent bien
`prerequisiteIds` contre `allNodes`), mais avec la liste vide,
`allNodes.Find(prereqId)` renvoyait toujours `null`, donc `CanUnlock`
renvoyait `false` pour toute technologie ayant au moins un prérequis —
la quasi-totalité — indépendamment de son coût. `ProjectSceneSetup.
SetTechNodeList` la peuple désormais (même mécanisme que
`SetBuildingList`/`SetMissionList`). Aucune UI de recherche n'existe
encore pour autant — contrairement au Temple (`TempleUI`) ou aux
missions (`MissionListUI`), parcourir/lancer une recherche parmi 93
nœuds répartis en 3 arbres × 5 branches est un vrai morceau d'UI à part
entière, hors du périmètre de ce chiffrage des coûts ; `CanUnlock`/
`TryUnlock` sont maintenant utilisables en code, mais rien ne les
appelle encore côté joueur.

### Exemples de synergies (du document source)

- **David** : Frondeurs entraînés + Confiance en l'Éternel + Garde royale
- **Josué** : Guerre sainte + Murailles de pierre + Prière avant le combat
- **Néhémie** : Contre-siège + Reste fidèle + Formation disciplinée
- **Moïse** : Signes et prodiges + Intercession + Tabernacle
- **David** (spirituel) : Louange et cantiques + Cœur affermi + Crainte de l'Éternel
- **Salomon** : Plans du Temple + Temple de Salomon + Culte purifié
- **Élie** : Miracles de jugement + Prophétie de jugement + Double portion
- **Néhémie** (spirituel) : Alliance renouvelée + Reste fidèle + Second Temple
- Combo économique fort sous Salomon : Vignes et oliviers + Artisanat de
  luxe + Ports et commerce maritime + Administration du Temple — puissant,
  mais risqué si la Justice et la Foi ne suivent pas.
