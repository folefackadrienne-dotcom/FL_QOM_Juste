# Kingdom of God — Direction Artistique

## Style global

**Semi-réaliste stylisé** avec forte influence de l'art antique du
Proche-Orient et de l'esthétique méditerranéenne orientale. Entre le
réalisme historique d'*Assassin's Creed Origins*, la lisibilité de
*Civilization VI*, la gravité épique de *The Banner Saga*, avec une touche
de symbolisme inspirée de l'iconographie biblique et des manuscrits hébreux
anciens. Ni cartoon, ni hyper-réaliste — formes légèrement simplifiées pour
rester lisibles sur mobile tout en gardant une présence noble sur PC.

## Palette de couleurs

Ocres, sables, terres brûlées, or chaud, bleu profond, rouge pourpre et
bordeaux, vert olive/cyprès, blanc cassé/ivoire.

Ambiance par âge : Patriarches/Exode = chaud et désertique · Conquête/Juges
= contrasté et terreux · Monarchie = riche et doré · Royaumes Divisés =
terne, ciels lourds, idolâtrie en rouge sombre · Exil = froid, gris-bleuté ·
Retour = lumière et or progressifs.

La lumière divine (miracles, présence de Dieu) est toujours un **or chaud
lumineux** reconnaissable.

## Personnages

Proportions légèrement héroïques (épaules un peu plus larges, silhouettes
claires, sans exagération extrême), visages expressifs et dignes, traits
sémitiques réalistes (nez, barbe, cheveux bouclés ou ondulés) — les
personnages importants (Abraham, Moïse, David...) ont des traits mémorables
et reconnaissables. Costumes : tissus drapés, tuniques, manteaux, ceintures
de cuir, couleurs symboliques par statut (blanc = prêtres, pourpre = rois,
simple = prophètes), bijoux et accessoires inspirés de l'archéologie
(sceaux, fibules, colliers). Animation : pose noble et mesurée, jamais
caricaturale ; les prophètes et rois ont une présence charismatique.

## Environnements & Architecture

Déserts vastes avec dunes et rochers, collines de Judée plantées d'oliviers
et de cyprès, vallées fertiles, côtes méditerranéennes, montagnes (Sinaï,
Carmel). Maisons en pierre/torchis, villes fortifiées à murailles
irrégulières, palais et temples à colonnes, chapiteaux simples, motifs
géométriques et végétaux. Le **Temple de Salomon** est le point culminant
visuel du jeu : imposant, lumineux, couvert d'or et de motifs sacrés. Les
lieux bibliques emblématiques (Jéricho, Jérusalem, Sichem, Béthel...)
restent reconnaissables tout en étant stylisés.

## VFX

Miracles = lumière dorée intense, particules de poussière d'or, vent sacré,
colonnes de nuée et de feu, eau qui se fend, murailles qui s'effondrent en
slow-motion élégant. Présence divine = léger halo doré ou lumière filtrant
à travers les nuages. Batailles = impacts réalistes mais peu de gore (sang
très limité ou stylisé). Idolâtrie/malédiction = rouge sombre, fumée noire,
lueurs malsaines. Restauration/repentance = lumière qui revient
progressivement, couleurs qui se réchauffent.

## UI

Élégante, motifs géométriques inspirés de l'art juif ancien et des frises
du Proche-Orient, bordures dorées fines, fond semi-transparent en parchemin
ou pierre claire. Icônes lisibles et symboliques (épi de blé, goutte d'eau,
flamme, rouleau de Torah, etc.). PC = interface dense, tooltips riches,
caméra libre, plus de détails décoratifs. Mobile = boutons plus grands,
menus contextuels, moins d'éléments simultanés à l'écran, icônes très
claires.

La palette dominante de cette section est déjà appliquée dans le code —
voir `UIThemeData` (`Assets/_Project/Scripts/UI/UIThemeData.cs`) et l'asset
`Assets/_Project/ScriptableObjects/UI/UITheme.asset`, câblés par
`ProjectSceneSetup` sur les boutons, panneaux et libellés générés (bordures
dorées via `Outline`, fond parchemin, texte ivoire) — un premier pas en
couleurs plates, en attendant les vraies textures/sprites 9-slice. Le
`WorldMoodUI` teinte en plus tout l'écran de `Kingdom` selon
`AllianceSystem.StandingChanged`, concrétisant la section Atmosphère
ci-dessous.

## Atmosphère

Gravité et respect du récit biblique, espoir même dans les moments sombres,
épopée sans excès. Le monde répond visuellement à l'Alliance : plus
lumineux quand le joueur est fidèle, plus terne et hostile quand il s'en
éloigne — `WorldMoodUI` (`Assets/_Project/Scripts/UI/WorldMoodUI.cs`) teinte
déjà un calque plein écran de `Kingdom` en `divineLight` sur Alliance
Haute et en `crisisRed` sur Alliance Basse, sur le même événement
`AllianceSystem.StandingChanged` qu'`AudioManager` utilise déjà pour la
musique et les SFX.

## Références visuelles

*Assassin's Creed Origins*, *Civilization VI*, *The Banner Saga*,
illustrations de Gustave Doré, art conceptuel d'*Exodus: Gods and Kings*,
manuscrits hébreux enluminés et mosaïques anciennes.
