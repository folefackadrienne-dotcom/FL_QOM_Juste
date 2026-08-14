# Kingdom of God – Direction Sonore (Audio Design)

Identité sonore complète et cohérente du jeu. Elle renforce le ton
**épique, sacré, humain et immersif** tout en restant accessible.

Implémentation : `Assets/_Project/Scripts/Audio/` (`MusicThemeData`,
`LeitmotifData`, `AmbientSoundscapeData`, `AudioManager`) — voir
[« Implémentation »](#implémentation) en fin de document.

---

## 1. Identité sonore globale

Le style sonore se situe entre :
- La gravité des bandes originales de films bibliques (*The Bible*, *Exodus*, *The Passion*)
- L'épique orchestral de jeux comme *Civilization VI* et *Total War*
- Des influences du Proche-Orient ancien (instruments traditionnels)

**Mots-clés :**
Sacré • Épique • Organique • Solennel • Espoir • Tension spirituelle

Le son doit faire sentir la présence de Dieu sans jamais être lourd ou
moralisateur. La musique et les effets soutiennent l'émotion plus qu'ils
ne la forcent.

---

## 2. Musique

**Instrumentation principale :**
- Orchestre symphonique (cordes, cuivres, percussions)
- Instruments anciens et moyen-orientaux : oud, ney (flûte), duduk, lyre, harpe, shofar, tambours sur cadre, cymbales
- Chœurs (hommes et femmes) utilisés avec parcimonie, souvent en hébreu ou en vocalises

**Structure musicale par contexte :**

| Situation              | Style musical                                      | Exemples d'instruments / ambiance                  |
|------------------------|----------------------------------------------------|----------------------------------------------------|
| **Menu principal**     | Thème majestueux et accueillant                    | Cordes + harpe + chœur distant                     |
| **Exploration / Paix** | Mélodies contemplatives et chaleureuses            | Lyre, flûte, cordes douces                         |
| **Construction**       | Rythme régulier et positif                         | Percussions légères + oud                          |
| **Bataille**           | Épique et tendu                                    | Cuivres, percussions puissantes, cordes agressives |
| **Miracle**            | Montée progressive puis explosion lumineuse        | Chœur + cordes ascendantes + shofar                |
| **Crise / Idolâtrie**  | Sombre, dissonant, oppressant                      | Duduk grave, percussions sourdes, cordes stridentes|
| **Repentance / Restauration** | Passage de l'ombre à la lumière               | Retour progressif des instruments nobles           |
| **Temple / Moments sacrés** | Solennel et élevé                               | Chœur a cappella + orgue léger ou harpe            |

Ces 8 situations sont les 8 `MusicThemeData` créés dans
`Assets/_Project/ScriptableObjects/Audio/Music/`.

**Thèmes récurrents (Leitmotifs) :**
- **Thème de l'Alliance** : mélodie douce et noble qui revient chaque fois que le joueur reste fidèle
- **Thème de la Promesse** (Abraham) : simple et espérant
- **Thème de la Libération** (Exode) : puissant et ascendant
- **Thème de David** : héroïque et lyrique (avec harpe)
- **Thème du Jugement** : lourd et inévitable
- **Thème de l'Espérance** (Retour d'exil) : lumineux et résilient

Chaque âge a sa propre couleur musicale tout en gardant une cohérence
globale. Ces 6 thèmes sont les 6 `LeitmotifData` créés dans
`Assets/_Project/ScriptableObjects/Audio/Leitmotifs/`, chacun rattaché à
son `MusicThemeData` principal via `primaryLeitmotif` quand pertinent.

---

## 3. Ambiances (Ambient Soundscape)

Les sons d'environnement sont très importants pour l'immersion :

- **Désert** : vent, sable qui glisse, insectes lointains, chameaux
- **Campement** : feux de camp, conversations murmurées, animaux, enfants
- **Ville / Jérusalem** : pas sur la pierre, marchés, prières lointaines, oiseaux
- **Temple** : silence presque sacré, léger écho, flamme des lampes, chants très lointains
- **Bataille** : cris, métal, chevaux, vent de guerre
- **Nuit** : grillons, vent doux, feu qui crépite

Le volume des ambiances baisse intelligemment pendant les dialogues
importants ou les moments de miracle. Ces 6 environnements sont les 6
`AmbientSoundscapeData` créés dans
`Assets/_Project/ScriptableObjects/Audio/Ambient/`.

---

## 4. Effets sonores (SFX)

**Interface :**
- Sons doux et nobles (clic sur parchemin, ouverture de menu comme un rouleau qui se déroule)
- Validation positive : petite cloche ou note de lyre
- Erreur / action impossible : son sourd et court

Ces 4 signaux sont les `SfxCueData` de catégorie `Interface` dans
`Assets/_Project/ScriptableObjects/Audio/Sfx/`, déclenchés par
`AudioManager.PlaySfx` depuis `PrayerMenuUI`/`VerseJournalUI`
(ouverture de menu, sélection, validation, erreur) et
`MainMenuController` (clic).

**Construction :**
- Pierres qui s'assemblent, bois taillé, outils
- Quand un bâtiment important est terminé : petite fanfare douce + chœur très léger

Ces 2 signaux sont les `SfxCueData` de catégorie `Construction` ;
`AudioManager` s'abonne à `BuildingManager.BuildingPlaced` et choisit
automatiquement la fanfare (bâtiments `Spiritual`/`Special`) ou le son
de pose ordinaire selon la catégorie du bâtiment placé.

**Batailles :**
- Impacts de métal réalistes mais pas excessivement violents
- Cris de guerre hébreux stylisés
- Mort des unités : son rapide et digne (pas de son gore)

Ces 3 signaux sont les `SfxCueData` de catégorie `Battle`, déclenchés
directement par `BattleManager` : `SpawnUnit` (cri de guerre à l'entrée
en combat), `TryAttack` (impact à chaque coup résolu) et `OnUnitDied`
(mort d'une unité).

**Miracles (très importants) :**
Chaque miracle a une signature sonore unique et mémorable :
- Mer Rouge : rugissement d'eau + vent puissant + note de shofar
- Feu du Carmel : silence puis explosion de flammes + chœur
- Soleil arrêté : étirement du temps + note suspendue
- Colonne de nuée/feu : souffle profond et lumineux

Ces 4 signatures sont posées dans
`MiracleData.audioSignatureDescription` sur les 4 miracles concernés ;
le champ existe (vide) sur les 20 autres, prêt à être rempli au fur et à
mesure des compositions.

Au-delà de ces signatures propres à chaque miracle, 4 `SfxCueData` de
catégorie `Miracle` couvrent les moments structurels communs à tout
rituel de prière, déclenchés par `AudioManager` sur les événements de
`MiracleManager` : Début de Prière (`PrayerStarted`), Interruption
(`PrayerInterrupted`, sans couper la prière en cours), Annulation
(`PrayerCancelled`) et Déclenchement (`MiracleCast` — l'« explosion
lumineuse » générique pour les miracles sans signature propre).

**Foi & Alliance :**
- Quand la jauge de Foi augmente : note claire et chaude
- Quand elle baisse : dissonance légère et inconfortable

Le même traitement est appliqué à la jauge d'Alliance, nommée dans le
titre de la section : 4 `SfxCueData` de catégorie `FaithAlliance`
(Foi en Hausse/Baisse, Alliance en Hausse/Baisse). `AudioManager`
s'abonne directement à `ResourceManager.ResourceChanged` (filtré sur
`ResourceType.Faith`) et à `AllianceSystem.ValueChanged`, en gardant la
dernière valeur connue de chaque jauge pour détecter le sens de la
variation et jouer le bon signal.

Au-delà de ces variations continues, 3 `SfxCueData` supplémentaires
couvrent les moments Alliance/Repentance qui méritent leur propre
signal plutôt que la simple note de hausse/baisse (GDD sections 3
« Alliance & Moralité » et 2, ligne Repentance / Restauration) :
- **Entrée en Crise** — joué une seule fois quand `AllianceSystem.Standing`
  franchit le seuil bas (devient `Low`), en plus du crossfade déjà en
  place vers le contexte musical Crise.
- **Faveur Élevée** — joué une seule fois quand `Standing` franchit le
  seuil haut (devient `High`).
- **Repentance / Restauration** — « passage de l'ombre à la lumière »,
  joué spécifiquement quand le joueur se repent
  (`AllianceSystem.TryRepent`), en plus de — et non à la place de — la
  note de hausse générique : `AllianceSystem` expose un événement dédié
  `Repented`, distinct de `ValueChanged`, pour que ce moment délibéré
  reste identifiable même si d'autres gains d'Alliance surviennent
  ailleurs pour la même quantité.

**Progression & Leaders :**

Catégorie non nommée dans le document d'origine, mais une extension
naturelle de la section « Progression & Metagame » du GDD (arbre
technologique, leaders légendaires déblocables). 5 `SfxCueData` de
catégorie `Progression` :
- Un chime par arbre technologique (Économique/Militaire/Spirituel),
  chacun coloré comme le domaine qu'il représente — même principe que
  la variation par catégorie déjà utilisée en Construction.
- Leader Débloqué (fanfare à l'arrivée d'un nouveau leader légendaire)
  et Leader Actif (flourish plus discret quand un leader déjà débloqué
  est mis en commandement).

Ces événements n'avaient pas encore de point d'ancrage dans le code :
`TechTree.TryUnlock` ne déclenchait aucun événement, et aucun manager
ne suivait les leaders débloqués/actifs. `TechTree` expose maintenant
`TechUnlocked`, et un nouveau `LeaderManager`
(`Assets/_Project/Scripts/Progression/LeaderManager.cs`, sur le même
modèle que `MissionManager`) expose `LeaderUnlocked`/`LeaderActivated`.

**Économie & Bâtiments :**

Extension de la section « Ressources & Économie » du GDD (Population &
Loyauté, Temple). 4 `SfxCueData` de catégorie `Economy` — Population en
Hausse/Baisse (`PopulationSystem.PopulationChanged`, même traitement
hausse/baisse que Foi & Alliance) et Murmures du Peuple / Rébellion
Imminente (`LoyaltyLow`/`LoyaltyCritical` — « Pénurie = murmures et
baisse de loyauté ») — plus 1 `SfxCueData` de catégorie `Construction`,
Temple Amélioré, distinct de la fanfare de bâtiment important puisqu'il
s'agit d'un Temple existant qui monte de niveau
(`TempleSystem.LevelUpgraded`), pas d'une nouvelle pose.

`PopulationSystem.Grow` ne déclenchait aucun événement de population
(seule la Loyauté en avait) ; il expose maintenant `PopulationChanged`.
Plus notable : `TempleSystem` — script complet avec ses propres coûts,
niveaux et miracles débloqués par niveau — n'était jamais instancié
nulle part dans `ProjectSceneSetup`, ni exposé par `GameManager`. Les
deux ont été corrigés en même temps que l'ajout des SFX, sur le même
modèle que les autres managers (`GameManager.Temple`, câblage dans
`CreateBootstrapScene`).

**Missions & Versets :**

Extension des sections « Missions » et « Verses (memorization mini-
game) » du GDD. 4 `SfxCueData` de catégorie `Narrative` : Mission
Commencée/Accomplie (`MissionManager.MissionStarted`/`MissionCompleted`)
et Verset Débloqué/Mémorisé (`VerseManager.VerseUnlocked`/
`VerseMemorized` — ce dernier au moment où le bonus permanent de
mémorisation est acquis). Contrairement aux rounds précédents,
`MissionManager` et `VerseManager` étaient déjà entièrement câblés
(`GameManager`, `ProjectSceneSetup`) et leurs événements existaient déjà
— il ne manquait que l'abonnement côté `AudioManager`.

---

## 5. Voix et narration

**Options recommandées :**
- Narrateur principal (voix masculine grave et posée, style documentaire noble)
- Voix des personnages importants (Moïse, David, prophètes…) – pas forcément 100 % doublées, mais phrases clés
- Lecture des versets : voix claire et respectueuse (possibilité de choisir entre plusieurs voix)

**Langues :**
- Français (priorité)
- Anglais
- Possibilité d'ajouter de l'hébreu pour certains chants ou lectures de versets (avec sous-titres)

Les versets mémorisés peuvent être écoutés en boucle avec une musique
très douce en fond.

Implémentation : `VoiceLineData` (`Assets/_Project/Scripts/Audio/VoiceLineData.cs`)
couvre le Narrateur et les Personnages — un `ScriptableObject` par ligne
avec un rôle (`VoiceRole.Narrator`/`Character`), un `speaker` optionnel
(`LeaderData`), une référence optionnelle au verset biblique source
(`relatedVerse`), le texte de la ligne, et un clip par langue
(`clipFrench`/`clipEnglish`/`clipHebrew`) — laissés vides jusqu'à un
vrai enregistrement, comme les autres champs `AudioClip` du projet.
3 instances créées dans `Assets/_Project/ScriptableObjects/Audio/Voice/`
comme exemples du mécanisme : un placeholder Narrateur (style de
référence, sans texte inventé), et 2 lignes de personnage réutilisant
des paroles bibliques déjà présentes dans le jeu plutôt que d'en
inventer — Josué (« Fortifie-toi et prends courage », Josué 1:9) et
Élie (« Jusqu'à quand clocherez-vous des deux côtés ? », 1 Rois 18:21),
toutes deux tirées mot pour mot des `VerseData` correspondants.

« Lecture des versets » réutilise directement `VerseData`, qui porte
déjà le texte biblique exact (`text`) : 3 champs
`narrationClipFrench`/`narrationClipEnglish`/`narrationClipHebrew` y ont
été ajoutés (sur les 34 versets existants) plutôt que de dupliquer le
contenu dans une structure séparée.

`AudioManager` porte la langue courante (`CurrentLanguage`, Français par
défaut — « priorité » — réglable via `SetLanguage`, avec repli sur le
français si le clip de la langue choisie manque) et expose
`PlayVoiceLine` (une ligne, une fois) et `PlayVerseNarration` (en
boucle, avec mise en sourdine des autres pistes le temps de l'écoute —
`StopNarration` les restaure). `VerseJournalUI` (Bibliothèque de la
Torah / Mode Méditation) y est câblé : `PlayNarration`/`StopNarration`,
et `Close` arrête automatiquement la lecture en cours.

---

## 6. Dynamique et mixage

- La musique s'adapte en temps réel à la situation (système de layers)
- Pendant les moments de forte Foi ou de miracle, la musique et les effets prennent plus d'espace
- En cas de crise morale ou d'idolâtrie, le mixage devient plus étouffé et inconfortable
- Sur mobile : compression intelligente pour garder la clarté même avec de petits haut-parleurs

---

## 7. Références sonores inspirantes

- *Civilization VI* (thèmes de civilisations + adaptativité)
- *The Bible* (mini-série 2013) – ton général
- *Exodus: Gods and Kings* (Hans Zimmer) – aspects épiques et désertiques
- *The Prince of Egypt* (chants et émotion)
- Musiques traditionnelles juives et moyen-orientales (sans tomber dans le folklore excessif)
- *Assassin's Creed Origins* (ambiances)

---

Ce style sonore doit donner au joueur le sentiment d'évoluer dans une
**grande histoire sacrée**, à la fois humaine et transcendante.

---

## Implémentation

Le mécanisme (pas seulement la direction artistique) est inséré dans le
projet :

- **`MusicThemeData`** (`Assets/_Project/Scripts/Audio/MusicThemeData.cs`)
  — un `ScriptableObject` par situation du tableau de la section 2
  (`MusicContext` : MainMenu, Exploration, Construction, Battle, Miracle,
  Crisis, Repentance, Temple), avec instrumentation, ambiance et
  leitmotiv principal en données. Clip audio laissé vide (`{fileID: 0}`)
  jusqu'à ce qu'une vraie composition existe — même convention que les
  champs `Sprite`/`icon` non illustrés ailleurs dans le projet.
- **`LeitmotifData`** (`Assets/_Project/Scripts/Audio/LeitmotifData.cs`)
  — un `ScriptableObject` par thème récurrent de la section 2, rattaché
  à l'Âge où il apparaît et à sa description de récurrence.
- **`AmbientSoundscapeData`** (`Assets/_Project/Scripts/Audio/AmbientSoundscapeData.cs`)
  — un `ScriptableObject` par environnement de la section 3, avec la
  liste des couches sonores à enregistrer/sourcer.
- **`AudioManager`** (`Assets/_Project/Scripts/Audio/AudioManager.cs`)
  — pilote le mixage dynamique (section 6) : crossfade entre les
  `MusicThemeData` au chargement de chaque scène (`MainMenu`/`Kingdom`/
  `Battle`), bascule automatique vers le contexte Crise quand l'Alliance
  tombe à Low (et retour en fondu quand elle remonte), bascule vers le
  contexte Miracle et ambiance étouffée (`duckedVolumeScale`) pendant
  `MiracleManager.BeginPrayer`/`AdvancePrayerTurn`/`AccelerateWithFaith`,
  et retour au contexte de la scène une fois le miracle résolu ou
  annulé. `SetDialogueDucked` est prêt pour un futur système de dialogue
  ou de cinématique.
- **`MiracleData.audioSignatureDescription`** — la signature sonore
  propre de chaque miracle (section 4), remplie pour Mer Rouge, Feu du
  Carmel, Soleil Arrêté et Colonne de Nuée et de Feu.
- **`SfxCueData`** (`Assets/_Project/Scripts/Audio/SfxCueData.cs`) — un
  `ScriptableObject` par effet ponctuel (par opposition aux boucles
  musique/ambiance) : les 4 signaux d'Interface, les 3 de Construction,
  les 3 de Bataille, les 4 de Miracle, les 7 de Foi & Alliance, les 5 de
  Progression, les 4 d'Economy et les 4 de Narrative sont créés dans
  `Assets/_Project/ScriptableObjects/Audio/Sfx/` (34 au total).
  `AudioManager.PlaySfx` les déclenche en un coup
  (`AudioSource.PlayOneShot`), appelé directement par
  `PrayerMenuUI`/`VerseJournalUI`/`MainMenuController` pour l'Interface
  et par `BattleManager` pour la Bataille (`SpawnUnit`/`TryAttack`/
  `OnUnitDied`), et automatiquement par `AudioManager` sur les
  événements de `BuildingManager.BuildingPlaced` /
  `TempleSystem.LevelUpgraded` (Construction — fanfare si le bâtiment
  est `Spiritual`/`Special`, son de pose ordinaire sinon, montée dédiée
  pour un Temple qui monte de niveau), de `MiracleManager` (Miracle —
  début de prière, interruption, annulation, déclenchement), de
  `ResourceManager.ResourceChanged` (filtré sur la Foi) /
  `AllianceSystem` (Foi & Alliance — hausse/baisse de chaque jauge via
  `ValueChanged`, détectée en comparant à la dernière valeur connue ;
  entrée en Crise/Faveur Élevée via `StandingChanged` ; Repentance via
  l'événement dédié `Repented`), de `TechTree.TechUnlocked` (un chime
  par arbre) / `LeaderManager` (Progression —
  `LeaderUnlocked`/`LeaderActivated`), de `PopulationSystem` (Economy —
  `PopulationChanged` hausse/baisse, `LoyaltyLow`/`LoyaltyCritical`), et
  de `MissionManager`/`VerseManager` (Narrative —
  `MissionStarted`/`MissionCompleted`,
  `VerseUnlocked`/`VerseMemorized`).

- **`VoiceLineData`** (`Assets/_Project/Scripts/Audio/VoiceLineData.cs`)
  et les 3 champs `narrationClip*` de `VerseData` (section 5) — voix du
  Narrateur/des Personnages et lecture des versets, avec sélection de
  langue (`AudioManager.CurrentLanguage`/`SetLanguage`,
  `PlayVoiceLine`/`PlayVerseNarration`/`StopNarration`) et repli sur le
  français si le clip demandé manque. 3 `VoiceLineData` créées dans
  `Assets/_Project/ScriptableObjects/Audio/Voice/` en exemples du
  mécanisme, câblées à `VerseJournalUI`.

`AudioManager` vit sur le même GameObject persistant `GameManager` que
les autres managers (scène `Bootstrap`), câblé par
`Kingdom of God → Setup → Create All Scenes` — voir README.
