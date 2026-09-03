#!/usr/bin/env python3
"""Generates Faith Run levels 4-20 from a shared engine template + per-level data.

Levels 1-3 (Eden/Exodus/Canaan) stay hand-authored as-is. Everything from
level 4 onward shares one proven engine (physics, particles, audio, hearts,
HUD, overlays) and varies only by data: palette, verses, obstacle/landmark
theme, and dialogue copy. This is the only way to reach 20 levels without
duplicating ~900 lines of engine code by hand 17 times.
"""
import os
import re

OUT_DIR = os.path.join(os.path.dirname(__file__), "..", "game")

LEVELS = [
  dict(
    n=4, slug="judges", title="Les Juges — Gédéon", subtitle="Niveau 4 — Les Juges",
    favicon="🔥",
    accent="#e0b84a", accent_soft="#f2d585", danger="#ff6a54", aqua="#8fd6c0", leaf="#7fbf8f",
    void="#161c2b", void2="#1f2638", panel="#283148", arch1="#3a4560", arch2="#181f30", canvasbg="#1c2740",
    sky=("#1c2c4a","#33456e","#5a5f7a","#8a7a6a"), hillfar="rgba(70,90,120,.5)", hillnear="rgba(45,60,90,.65)",
    ground=("#4a5a7a","#333f5c","#1e2438","#7a8ab0"),
    motif="fire", motif_colors=("#ffb64f","#ff7a3d","#f2e6c8"),
    patrol_shape="humanoid", patrol_color="#5a4a3a", patrol_accent="#c9a84e",
    static_color="#8f8a78", static_accent="#5a4326",
    start_title="Le camp de Madian", start_blurb="Descends dans la vallée de nuit, torche et cruche en main. Trois cents suffiront — évite les sentinelles et les pièges du camp.",
    verses=[
      ("Juges 6:12", "L'Éternel est avec toi, vaillant héros !"),
      ("Juges 6:14", "Va avec cette force que tu as, et délivre Israël... N'est-ce pas moi qui t'envoie ?"),
      ("Juges 6:24", "Gédéon bâtit là un autel à l'Éternel, et lui donna pour nom : l'Éternel paix."),
      ("Juges 7:2", "Le peuple que tu as avec toi est trop nombreux, pour que je livre Madian entre ses mains."),
      ("Juges 7:20", "Ils sonnèrent des trompettes... L'épée de l'Éternel et de Gédéon !"),
    ],
    dialogue_ref="Juges 7:7", dialogue_title="Trois cents suffisent",
    dialogue_quote="« C'est par les trois cents hommes... que je vous sauverai, et je livrerai Madian entre tes mains. »",
    reflection="Dieu a réduit l'armée de Gédéon avant la victoire, pour qu'Israël sache que la délivrance venait de Lui, non de sa force. Le disciple aussi apprend parfois par le manque : ce n'est pas ta force qui porte du fruit, mais Celui qui t'envoie.",
    complete_blurb="Tu as traversé le camp de nuit et vu tomber la peur devant trois cents hommes et des torches.",
    gameover_title="Repéré par une sentinelle", gameover_blurb="Même repéré dans l'obscurité, la grâce relève. Reprends la course.",
    next_slug="ruth", next_title="Niveau 5 : Ruth",
  ),
  dict(
    n=5, slug="ruth", title="Ruth", subtitle="Niveau 5 — Ruth",
    favicon="🌾",
    accent="#e8c15a", accent_soft="#f7dd94", danger="#c96b3e", aqua="#8fc9a8", leaf="#c9a13e",
    void="#241e12", void2="#2e2717", panel="#3a2f1c", arch1="#6b5730", arch2="#2c2414", canvasbg="#f2d98a",
    sky=("#7fc4e8","#bcdca0","#e8dfa0","#f2d98a"), hillfar="rgba(196,168,90,.45)", hillnear="rgba(163,132,58,.6)",
    ground=("#e8c96a","#c9a13e","#6b4f26","#f7e0a0"),
    motif="growth", motif_colors=("#c9a13e","#e8c15a","#f7e0a0"),
    patrol_shape="humanoid", patrol_color="#8a6a3e", patrol_accent="#e8c15a",
    static_color="#a67c3e", static_accent="#5a4326",
    start_title="Les champs de Bethléem", start_blurb="Suis Ruth dans les champs de Boaz : glane l'orge, garde ta loyauté, avance vers l'aire à battre.",
    verses=[
      ("Ruth 1:16", "Où tu iras j'irai... ton peuple sera mon peuple, et ton Dieu sera mon Dieu."),
      ("Ruth 2:12", "Que l'Éternel te rende ce que tu as fait, et que ta récompense soit entière..."),
      ("Ruth 2:2", "Laisse-moi glaner et ramasser des épis derrière celui aux yeux duquel je trouverai grâce."),
      ("Ruth 3:9", "Étends ton aile sur ta servante, car tu as droit de rachat."),
      ("Ruth 4:14", "Béni soit l'Éternel, qui ne t'a pas laissé manquer aujourd'hui d'un rédempteur !"),
    ],
    dialogue_ref="Ruth 1:16", dialogue_title="Ton peuple sera mon peuple",
    dialogue_quote="« Ne me presse pas de te laisser, de retourner loin de toi. Où tu iras j'irai... ton peuple sera mon peuple, et ton Dieu sera mon Dieu. »",
    reflection="Ruth a choisi la fidélité sans rien voir de la suite — ni Boaz, ni la moisson, ni l'aïeule qu'elle deviendrait. Le disciple avance parfois dans la même obscurité : ce que Dieu demande aujourd'hui, c'est la loyauté du prochain pas, pas la vision du chemin entier.",
    complete_blurb="Tu as glané dans les champs de Boaz et trouvé grâce jusqu'à l'aire à battre.",
    gameover_title="Perdu dans les champs", gameover_blurb="Même égaré dans le champ, la grâce relève. Reprends la course.",
    next_slug="goliath", next_title="Niveau 6 : David et Goliath",
  ),
  dict(
    n=6, slug="goliath", title="David et Goliath", subtitle="Niveau 6 — David et Goliath",
    favicon="🪨",
    accent="#e0a04a", accent_soft="#f2c586", danger="#b8482e", aqua="#8fc0d6", leaf="#8fae5f",
    void="#221a14", void2="#2c221a", panel="#3a2c20", arch1="#6b4c34", arch2="#241a12", canvasbg="#e8b878",
    sky=("#7fb8e0","#d0c090","#e8b878","#c98a4e"), hillfar="rgba(196,150,90,.45)", hillnear="rgba(150,104,58,.62)",
    ground=("#d4a860","#a87c3e","#5a3f22","#f2ce8e"),
    motif="mountain", motif_colors=("#8a6a4a","#b8905a","#e8c98a"),
    patrol_shape="humanoid", patrol_color="#5a4a3a", patrol_accent="#8f2f22",
    static_color="#9a9080", static_accent="#5a4326",
    start_title="La vallée d'Éla", start_blurb="Descends dans la vallée, cinq pierres lisses en main. Ne recule pas devant la taille du géant.",
    verses=[
      ("1 Samuel 17:37", "L'Éternel, qui m'a délivré de la griffe du lion et de la patte de l'ours, me délivrera aussi..."),
      ("1 Samuel 17:45", "Tu marches contre moi avec l'épée... et moi, je marche contre toi au nom de l'Éternel des armées."),
      ("1 Samuel 17:47", "Ce n'est ni par l'épée ni par la lance que l'Éternel sauve, car la victoire appartient à l'Éternel."),
      ("1 Samuel 16:7", "L'homme regarde à ce qui frappe les yeux, mais l'Éternel regarde au cœur."),
      ("1 Samuel 17:32", "Que personne ne se décourage à cause de ce Philistin ! Ton serviteur ira se battre."),
    ],
    dialogue_ref="1 Samuel 17:47", dialogue_title="La bataille est à l'Éternel",
    dialogue_quote="« Toute cette multitude saura que ce n'est ni par l'épée ni par la lance que l'Éternel sauve, car la victoire appartient à l'Éternel. »",
    reflection="David n'a pas vaincu Goliath par la taille de son courage, mais par la petitesse de son regard sur lui-même et la grandeur de son regard sur Dieu. Le disciple n'a pas besoin d'une armure qui ne lui va pas — juste de ce qu'il connaît déjà : une fronde, cinq pierres, et un Dieu vivant.",
    complete_blurb="Tu as traversé la vallée d'Éla et vu tomber le géant devant une fronde et cinq pierres.",
    gameover_title="Terrassé dans la vallée", gameover_blurb="Même face au géant, la grâce relève. Reprends la course.",
    next_slug="david-repentance", next_title="Niveau 7 : Le Roi David",
  ),
  dict(
    n=7, slug="david-repentance", title="Le Roi David", subtitle="Niveau 7 — Le Roi David",
    favicon="💧",
    accent="#8fb8e0", accent_soft="#bcd8f2", danger="#c9603e", aqua="#a8d6e6", leaf="#7fa8bf",
    void="#141b26", void2="#1c2530", panel="#243040", arch1="#3a5068", arch2="#161f2a", canvasbg="#5f8fbf",
    sky=("#4a7fb8","#7fa8cf","#bcd0e0","#e8ecec"), hillfar="rgba(90,120,150,.45)", hillnear="rgba(58,84,110,.62)",
    ground=("#6fa0c9","#4a7396","#22384a","#a8d0e8"),
    motif="water", motif_colors=("#a8d6e6","#6fa0c9","#e8f4f8"),
    patrol_shape="humanoid", patrol_color="#4a4038", patrol_accent="#8f3a2e",
    static_color="#7a8890", static_accent="#3a2e26",
    start_title="Un cœur brisé", start_blurb="Traverse la nuit du roi David, entre la faute et le pardon, jusqu'à un cœur pur et renouvelé.",
    verses=[
      ("Psaume 51:3", "Aie pitié de moi, ô Dieu, dans ta bonté... selon ta grande miséricorde, efface mes transgressions."),
      ("Psaume 51:9", "Lave-moi, et je serai plus blanc que la neige."),
      ("Psaume 51:12", "Crée en moi un cœur pur, ô Dieu, renouvelle en moi un esprit bien disposé."),
      ("Psaume 51:19", "Les sacrifices qui plaisent à Dieu, c'est un esprit brisé : ô Dieu, tu ne dédaignes pas un cœur brisé et contrit."),
      ("2 Samuel 12:13", "David dit à Nathan : J'ai péché contre l'Éternel. Et Nathan dit à David : L'Éternel pardonne ton péché."),
    ],
    dialogue_ref="Psaume 51:12", dialogue_title="Crée en moi un cœur pur",
    dialogue_quote="« Crée en moi un cœur pur, ô Dieu, renouvelle en moi un esprit bien disposé. »",
    reflection="David, l'homme selon le cœur de Dieu, est aussi l'homme qui est tombé bas. Sa grandeur n'a pas été de ne jamais faillir, mais de ne jamais fuir la lumière quand Nathan est venu. Le disciple n'est pas défini par sa chute, mais par le chemin du retour.",
    complete_blurb="Tu as traversé la nuit du roi et trouvé, de l'autre côté de l'aveu, un cœur renouvelé.",
    gameover_title="Repris par la faute", gameover_blurb="Même dans la chute, la grâce relève. Reprends la course.",
    next_slug="elie", next_title="Niveau 8 : Élie",
  ),
  dict(
    n=8, slug="elie", title="Élie", subtitle="Niveau 8 — Élie",
    favicon="⛰️",
    accent="#ff9a4a", accent_soft="#ffc98a", danger="#8a3a2e", aqua="#8fc0c6", leaf="#7fae7f",
    void="#1c140f", void2="#261c14", panel="#332417", arch1="#6b4526", arch2="#221810", canvasbg="#d9905a",
    sky=("#3a2a3e","#7a4a3e","#d9905a","#f2c07a"), hillfar="rgba(120,80,60,.5)", hillnear="rgba(80,50,36,.65)",
    ground=("#a8683e","#7a4a2a","#3a2414","#d9a068"),
    motif="fire", motif_colors=("#ff7a3d","#ffb64f","#ffe1a3"),
    patrol_shape="humanoid", patrol_color="#4a2e20", patrol_accent="#8a5a3e",
    static_color="#8a7a6a", static_accent="#3a2414",
    start_title="Du torrent au Carmel", start_blurb="Sois nourri au torrent de Kerith, puis affronte les prophètes de Baal sur le mont Carmel : que le feu réponde.",
    verses=[
      ("1 Rois 17:4", "Tu boiras au torrent, et j'ai ordonné aux corbeaux de te nourrir là."),
      ("1 Rois 17:16", "La farine dans le pot ne manqua point, et l'huile dans la cruche ne diminua point, selon la parole de l'Éternel."),
      ("1 Rois 18:21", "Jusqu'à quand clocherez-vous des deux côtés ? Si l'Éternel est Dieu, allez après lui."),
      ("1 Rois 18:38", "Le feu de l'Éternel tomba, et consuma l'holocauste... et l'eau qui était dans le fossé."),
      ("1 Rois 19:12", "Après le feu, un murmure doux et léger. Et l'Éternel était dans ce murmure."),
    ],
    dialogue_ref="1 Rois 19:12", dialogue_title="Un murmure doux et léger",
    dialogue_quote="« Après le tremblement de terre, un feu... et après le feu, un murmure doux et léger. Et l'Éternel était dans ce murmure. »",
    reflection="Élie a vu le feu tomber du ciel devant tout un peuple — puis, seul et épuisé, il a rencontré Dieu non dans la tempête mais dans le silence. Le disciple ne vit pas seulement des sommets spectaculaires : il apprend aussi à reconnaître la voix de Dieu dans le murmure du quotidien.",
    complete_blurb="Tu as été nourri au torrent et vu le feu tomber sur le mont Carmel.",
    gameover_title="À bout de forces", gameover_blurb="Même épuisé sous le genêt, la grâce relève. Reprends la course.",
    next_slug="daniel-lions", next_title="Niveau 9 : Daniel",
  ),
  dict(
    n=9, slug="daniel-lions", title="Daniel dans la fosse", subtitle="Niveau 9 — Daniel",
    favicon="🦁",
    accent="#e0b06a", accent_soft="#f2d29a", danger="#c9602e", aqua="#8fb8c6", leaf="#7f9e7f",
    void="#181414", void2="#221c1a", panel="#2e2622", arch1="#5a4030", arch2="#1c1614", canvasbg="#8a7050",
    sky=("#2a2430","#4a3c40","#7a6050","#a88860"), hillfar="rgba(120,100,80,.45)", hillnear="rgba(80,66,52,.62)",
    ground=("#8a7050","#5a4630","#2a2018","#c9a870"),
    motif="creature", creature_kind="lion", motif_colors=("#c9a04e","#8a6a2e","#4a3a1a"),
    patrol_shape="humanoid", patrol_color="#4a4038", patrol_accent="#c9a04e",
    static_color="#8f8578", static_accent="#3a2e22",
    start_title="La fosse aux lions", start_blurb="Reste fidèle à ta prière malgré le décret du roi, et descends jusqu'à la fosse où les lions attendent.",
    verses=[
      ("Daniel 6:11", "Il continuait à se mettre à genoux trois fois le jour, pour prier et invoquer son Dieu, comme il le faisait auparavant."),
      ("Daniel 6:17", "On apporta une pierre, et on la mit sur l'ouverture de la fosse."),
      ("Daniel 6:22", "Mon Dieu a envoyé son ange, et il a fermé la gueule des lions, qui ne m'ont fait aucun mal."),
      ("Daniel 6:23", "On ne trouva sur lui aucune blessure, parce qu'il avait cru en son Dieu."),
      ("Daniel 3:17", "Notre Dieu que nous servons peut nous délivrer... et il nous délivrera de ta main, ô roi."),
    ],
    dialogue_ref="Daniel 6:23", dialogue_title="Il avait cru en son Dieu",
    dialogue_quote="« On ne trouva sur lui aucune blessure, parce qu'il avait cru en son Dieu. »",
    reflection="Daniel n'a pas changé sa prière pour éviter la fosse — trois fois par jour, fenêtre ouverte, comme toujours. La fidélité du disciple ne se mesure pas à l'absence de danger, mais à ce qu'il continue de faire quand le danger est réel.",
    complete_blurb="Tu as traversé la fosse aux lions et vu leur gueule fermée jusqu'au matin.",
    gameover_title="Saisi par les lions", gameover_blurb="Même dans la fosse, la grâce relève. Reprends la course.",
    next_slug="fournaise", next_title="Niveau 10 : La fournaise",
  ),
  dict(
    n=10, slug="fournaise", title="La fournaise ardente", subtitle="Niveau 10 — La fournaise",
    favicon="🔥",
    accent="#ff8a4a", accent_soft="#ffc07a", danger="#b8402a", aqua="#8fc6c0", leaf="#7fae8f",
    void="#1a1210", void2="#241a16", panel="#30221c", arch1="#6b3a24", arch2="#20140f", canvasbg="#e07a3e",
    sky=("#2a1a18","#6b2e20","#c9502e","#f2924a"), hillfar="rgba(140,80,50,.45)", hillnear="rgba(96,50,30,.62)",
    ground=("#a85a30","#6b3a1e","#301c12","#e0925a"),
    motif="fire", motif_colors=("#ff5a2e","#ffa04a","#ffe1a3"),
    patrol_shape="humanoid", patrol_color="#4a3226", patrol_accent="#c9502e",
    static_color="#8f7a68", static_accent="#301c12",
    start_title="Sept fois plus chaude", start_blurb="Refuse de te prosterner devant la statue d'or, et avance jusqu'à la fournaise chauffée sept fois plus.",
    verses=[
      ("Daniel 3:17", "Notre Dieu que nous servons peut nous délivrer de la fournaise ardente, et il nous délivrera de ta main, ô roi."),
      ("Daniel 3:18", "Sinon, sache, ô roi, que nous ne servirons pas tes dieux, et que nous n'adorerons pas la statue d'or."),
      ("Daniel 3:25", "Je vois quatre hommes... et le quatrième ressemble à un fils des dieux."),
      ("Daniel 3:27", "Le feu n'avait eu aucun pouvoir sur leur corps, et pas un cheveu de leur tête n'avait été brûlé."),
      ("Ésaïe 43:2", "Quand tu marcheras dans le feu, tu ne te brûleras pas, et la flamme ne t'embrasera pas."),
    ],
    dialogue_ref="Daniel 3:25", dialogue_title="Un quatrième homme",
    dialogue_quote="« Je vois quatre hommes sans liens, qui marchent au milieu du feu, et qui n'ont point de mal ; et la figure du quatrième ressemble à un fils des dieux. »",
    reflection="Sadrac, Méschac et Abed-Nego n'ont pas su s'ils seraient sauvés du feu — seulement qu'ils ne serviraient pas d'autre dieu, sauvés ou non. Dieu n'a pas empêché la fournaise ; il est entré dedans avec eux. Le disciple n'a pas la promesse d'éviter le feu, mais celle de ne jamais y marcher seul.",
    complete_blurb="Tu as traversé la fournaise et vu qu'un quatrième homme y marchait avec toi.",
    gameover_title="Consumé par les flammes", gameover_blurb="Même dans la fournaise, la grâce relève. Reprends la course.",
    next_slug="jonas", next_title="Niveau 11 : Jonas",
  ),
  dict(
    n=11, slug="jonas", title="Jonas et Ninive", subtitle="Niveau 11 — Jonas",
    favicon="🐋",
    accent="#6fb0c9", accent_soft="#a8d6e6", danger="#3a5a70", aqua="#8fd6e6", leaf="#5f9e8f",
    void="#101a20", void2="#18242c", panel="#20303a", arch1="#2e5468", arch2="#141e24", canvasbg="#3a7090",
    sky=("#1a3a4a","#2e5a70","#4a7f95","#7fb0c0"), hillfar="rgba(50,90,110,.45)", hillnear="rgba(30,60,78,.62)",
    ground=("#3a6a80","#264a5a","#122430","#6fa0b8"),
    motif="creature", creature_kind="fish", motif_colors=("#4a7f95","#2e5a70","#8fd6e6"),
    patrol_shape="creature", patrol_color="#264a5a", patrol_accent="#8fd6e6",
    static_color="#6a8a94", static_accent="#122430",
    start_title="Fuir, puis obéir", start_blurb="De la tempête au ventre du poisson, jusqu'aux rues de Ninive : la fuite n'est jamais plus loin que la main de Dieu.",
    verses=[
      ("Jonas 1:3", "Jonas se leva pour s'enfuir à Tarsis, loin de la face de l'Éternel."),
      ("Jonas 1:17", "L'Éternel fit venir un grand poisson pour engloutir Jonas ; et Jonas fut dans les entrailles du poisson trois jours."),
      ("Jonas 2:3", "Dans ma détresse, j'ai invoqué l'Éternel, et il m'a exaucé."),
      ("Jonas 3:4", "Encore quarante jours, et Ninive est détruite ! Et les gens de Ninive crurent à Dieu."),
      ("Jonas 4:2", "Je le savais... que tu es un Dieu compatissant, miséricordieux, lent à la colère et riche en bonté."),
    ],
    dialogue_ref="Jonas 3:10", dialogue_title="Dieu revint sur sa décision",
    dialogue_quote="« Dieu vit qu'ils agissaient ainsi et qu'ils revenaient de leur mauvaise voie. Alors Dieu se repentit du mal qu'il avait déclaré vouloir leur faire, et il ne le fit pas. »",
    reflection="Jonas a fui vers l'ouest ce que Dieu appelait vers l'est — et jusque dans le poisson, il n'a pas échappé, il a été rattrapé. Le disciple découvre parfois que la fuite elle-même devient le chemin du retour : Dieu poursuit ceux qu'il envoie.",
    complete_blurb="Tu as traversé la mer, le poisson, et les rues de Ninive jusqu'au repentir de toute une ville.",
    gameover_title="Englouti par la tempête", gameover_blurb="Même dans la fuite, la grâce relève. Reprends la course.",
    next_slug="nehemie", next_title="Niveau 12 : Néhémie",
  ),
  dict(
    n=12, slug="nehemie", title="Néhémie reconstruit", subtitle="Niveau 12 — Néhémie",
    favicon="🧱",
    accent="#c9a05a", accent_soft="#e8c98a", danger="#8f4a2e", aqua="#8fb0c6", leaf="#7f9e6f",
    void="#1a1610", void2="#241e16", panel="#302818", arch1="#6b5230", arch2="#201a10", canvasbg="#c9a468",
    sky=("#6fa0c9","#a8c090","#d4c090","#e8c98a"), hillfar="rgba(160,130,80,.45)", hillnear="rgba(110,88,52,.62)",
    ground=("#b89058","#8a6a3a","#4a3a1e","#e0c088"),
    motif="structure", motif_colors=("#c9a468","#8a6a3a","#e8c98a"),
    patrol_shape="humanoid", patrol_color="#4a3a26", patrol_accent="#8f4a2e",
    static_color="#9a8868", static_accent="#3a2e1a",
    start_title="Une truelle et une épée", start_blurb="Rebâtis les murailles de Jérusalem, une pierre à la fois, malgré les moqueries et les menaces alentour.",
    verses=[
      ("Néhémie 2:18", "Levons-nous, et bâtissons ! Et ils se fortifièrent dans cette bonne œuvre."),
      ("Néhémie 4:6", "Nous rebâtîmes la muraille... car le peuple avait à cœur de travailler."),
      ("Néhémie 4:17", "Ceux qui bâtissaient la muraille... d'une main ils travaillaient, et de l'autre ils tenaient une arme."),
      ("Néhémie 6:3", "Je fais un grand travail, et je ne puis descendre. Pourquoi le travail cesserait-il ?"),
      ("Néhémie 8:10", "Ne vous affligez pas, car la joie de l'Éternel sera votre force."),
    ],
    dialogue_ref="Néhémie 6:3", dialogue_title="Je ne puis descendre",
    dialogue_quote="« Je fais un grand travail, et je ne puis descendre. Pourquoi le travail cesserait-il pendant que je le quitterais pour descendre vers vous ? »",
    reflection="Néhémie a bâti la truelle dans une main et l'épée dans l'autre — sans prétendre que l'opposition n'existait pas, sans la laisser interrompre l'ouvrage. Le disciple aussi construit au milieu, pas après, les moqueries et les menaces.",
    complete_blurb="Tu as rebâti la muraille pierre par pierre, malgré les voix qui voulaient te faire descendre.",
    gameover_title="Interrompu dans l'ouvrage", gameover_blurb="Même arrêté sur le chantier, la grâce relève. Reprends la course.",
    next_slug="esther", next_title="Niveau 13 : Esther",
  ),
  dict(
    n=13, slug="esther", title="Esther", subtitle="Niveau 13 — Esther",
    favicon="👑",
    accent="#c98ac0", accent_soft="#e8c0e0", danger="#8f3a4a", aqua="#a890c9", leaf="#8f9ec0",
    void="#1a1420", void2="#241c2c", panel="#302638", arch1="#5a3a5e", arch2="#1e1626", canvasbg="#6b3a68",
    sky=("#2e1a3a","#5a2e58","#8a4a7a","#c98ac0"), hillfar="rgba(120,70,110,.45)", hillnear="rgba(80,44,74,.62)",
    ground=("#7a4a70","#4a2a48","#241226","#a878a0"),
    motif="structure", motif_colors=("#c9a04e","#8a6a2e","#e8d29a"),
    patrol_shape="humanoid", patrol_color="#4a2e40", patrol_accent="#8f3a4a",
    static_color="#9a7a8a", static_accent="#2e1826",
    start_title="Pour un temps comme celui-ci", start_blurb="Traverse le palais de Suse, entre le jeûne et le sceptre d'or, jusqu'au trône du roi.",
    verses=[
      ("Esther 4:14", "Qui sait si ce n'est pas pour un temps comme celui-ci que tu es parvenue à la royauté ?"),
      ("Esther 4:16", "Va, rassemble tous les Juifs... jeûnez pour moi... si je dois périr, je périrai."),
      ("Esther 5:2", "Le roi tendit à Esther le sceptre d'or qui était dans sa main. Alors Esther s'approcha."),
      ("Esther 7:3", "Que ma vie me soit accordée, voilà ma demande ; et celle de mon peuple, voilà mon souhait."),
      ("Esther 9:22", "Un jour où les Juifs eurent du repos... un jour de joie... et de dons mutuels."),
    ],
    dialogue_ref="Esther 4:14", dialogue_title="Pour un temps comme celui-ci",
    dialogue_quote="« Qui sait si ce n'est pas pour un temps comme celui-ci que tu es parvenue à la royauté ? »",
    reflection="Esther n'a pas choisi le palais, ni le danger d'approcher le roi sans être appelée. Mais au moment venu, elle a choisi de ne pas se taire. Le disciple n'a pas toujours choisi la place où il se trouve — mais il peut toujours choisir ce qu'il en fait.",
    complete_blurb="Tu as traversé le palais de Suse et vu ton peuple délivré, pour un temps comme celui-ci.",
    gameover_title="Réduit au silence", gameover_blurb="Même face au roi, la grâce relève. Reprends la course.",
    next_slug="bon-berger", next_title="Niveau 14 : Le Bon Berger",
  ),
  dict(
    n=14, slug="bon-berger", title="Le Bon Berger", subtitle="Niveau 14 — Le Bon Berger",
    favicon="🐑",
    accent="#8fc0a8", accent_soft="#c0e0cc", danger="#c9603e", aqua="#a8d6cc", leaf="#7fbf8f",
    void="#121c1a", void2="#1a2624", panel="#22322e", arch1="#345a4e", arch2="#161e1c", canvasbg="#7fc0b8",
    sky=("#6fb8d0","#a8d6b8","#d4e8b8","#e8f2c8"), hillfar="rgba(110,160,110,.45)", hillnear="rgba(70,120,74,.62)",
    ground=("#8fcf9a","#5a9e6a","#264a30","#c0e8b0"),
    motif="water", motif_colors=("#a8d6e6","#7fc0d6","#eaf7f8"),
    patrol_shape="creature", patrol_color="#5a4a3a", patrol_accent="#8a6a4a",
    static_color="#8f8a68", static_accent="#3a3222",
    start_title="Les verts pâturages", start_blurb="Suis le Berger à travers les pâturages, la vallée de l'ombre, jusqu'aux eaux paisibles.",
    verses=[
      ("Psaume 23:1", "L'Éternel est mon berger : je ne manquerai de rien."),
      ("Psaume 23:2", "Il me fait reposer dans de verts pâturages, il me dirige près des eaux paisibles."),
      ("Psaume 23:4", "Quand je marche dans la vallée de l'ombre de la mort, je ne crains aucun mal, car tu es avec moi."),
      ("Psaume 23:5", "Tu dresses devant moi une table... tu oins d'huile ma tête."),
      ("Jean 10:11", "Je suis le bon berger. Le bon berger donne sa vie pour ses brebis."),
    ],
    dialogue_ref="Psaume 23:4", dialogue_title="Tu es avec moi",
    dialogue_quote="« Quand je marche dans la vallée de l'ombre de la mort, je ne crains aucun mal, car tu es avec moi : ta houlette et ton bâton me rassurent. »",
    reflection="Le Berger ne fait pas éviter la vallée à ses brebis — il marche dedans avec elles. Le disciple n'a pas la promesse d'un chemin sans ombre, mais celle d'une présence qui ne le quitte pas au milieu.",
    complete_blurb="Tu as traversé la vallée de l'ombre et trouvé, de l'autre côté, des eaux paisibles.",
    gameover_title="Égaré loin du troupeau", gameover_blurb="Même égaré, le Berger relève. Reprends la course.",
    next_slug="nativite", next_title="Niveau 15 : La Nativité",
  ),
  dict(
    n=15, slug="nativite", title="La Nativité", subtitle="Niveau 15 — La Nativité",
    favicon="⭐",
    accent="#a8c0e8", accent_soft="#d0e0f7", danger="#8a4a3e", aqua="#c0d6e8", leaf="#7f9ec0",
    void="#0e1220", void2="#161c2c", panel="#1e2638", arch1="#2e3a5e", arch2="#121828", canvasbg="#1a2440",
    sky=("#0e1a3a","#1e2c5a","#3a4a7a","#6a7ab0"), hillfar="rgba(60,74,110,.5)", hillnear="rgba(38,48,80,.65)",
    ground=("#3a4a6a","#242e48","#12182a","#5a6a94"),
    motif="star", motif_colors=("#fff4d6","#ffe1a3","#f2c66b"),
    patrol_shape="humanoid", patrol_color="#3a2e28", patrol_accent="#8a5a3e",
    static_color="#6a7890", static_accent="#161c2c",
    start_title="Nulle place pour eux", start_blurb="Suis Marie et Joseph de Nazareth à Bethléem, sous l'étoile, jusqu'à la crèche.",
    verses=[
      ("Luc 2:7", "Elle enfanta son fils premier-né... et le coucha dans une crèche, car il n'y avait pas de place pour eux."),
      ("Luc 2:10", "N'ayez pas peur, car je vous annonce une bonne nouvelle, qui sera pour tout le peuple le sujet d'une grande joie."),
      ("Luc 2:11", "Il vous est né aujourd'hui... un Sauveur, qui est le Christ, le Seigneur."),
      ("Matthieu 2:2", "Nous avons vu son étoile en Orient, et nous sommes venus pour l'adorer."),
      ("Ésaïe 9:5", "On l'appellera Admirable, Conseiller, Dieu puissant, Père éternel, Prince de la paix."),
    ],
    dialogue_ref="Luc 2:11", dialogue_title="Il vous est né un Sauveur",
    dialogue_quote="« Il vous est né aujourd'hui, dans la ville de David, un Sauveur, qui est le Christ, le Seigneur. »",
    reflection="Dieu est venu sans réservation, dans une ville trop pleine, sous une étoile que seuls des bergers et des étrangers ont pris le temps de suivre. Le disciple commence ici : l'Incarnation dit que Dieu ne reste pas loin de ce qui est petit, pauvre ou de passage.",
    complete_blurb="Tu as suivi l'étoile de Nazareth à Bethléem, jusqu'à la crèche.",
    gameover_title="Perdu dans la nuit", gameover_blurb="Même dans la nuit, l'étoile relève. Reprends la course.",
    next_slug="tentation", next_title="Niveau 16 : Le désert",
  ),
  dict(
    n=16, slug="tentation", title="Le baptême et le désert", subtitle="Niveau 16 — Le désert",
    favicon="🕊️",
    accent="#8fb0e0", accent_soft="#c0d6f2", danger="#c9a05a", aqua="#a8d0e8", leaf="#8fae7f",
    void="#161810", void2="#202218", panel="#2c2e20", arch1="#4a4a30", arch2="#1a1c12", canvasbg="#c9b878",
    sky=("#7fb0d6","#c9c898","#e8d898","#f2e8b8"), hillfar="rgba(160,150,90,.45)", hillnear="rgba(112,104,58,.62)",
    ground=("#c0a868","#8a763e","#4a3c1e","#e0cf98"),
    motif="water", motif_colors=("#a8d0e8","#7fb0d6","#eaf4f8"),
    patrol_shape="creature", patrol_color="#4a3c26", patrol_accent="#8a763e",
    static_color="#9a8a68", static_accent="#3a3016",
    start_title="Du Jourdain au désert", start_blurb="Sois baptisé dans le Jourdain, puis suis l'Esprit au désert : quarante jours, trois épreuves.",
    verses=[
      ("Matthieu 3:16", "Aussitôt il vit les cieux s'ouvrir, et l'Esprit de Dieu descendre comme une colombe et venir sur lui."),
      ("Matthieu 3:17", "Celui-ci est mon Fils bien-aimé, en qui j'ai mis toute mon affection."),
      ("Matthieu 4:4", "L'homme ne vivra pas de pain seulement, mais de toute parole qui sort de la bouche de Dieu."),
      ("Matthieu 4:10", "Retire-toi, Satan ! Car il est écrit : Tu adoreras le Seigneur, ton Dieu, et tu le serviras lui seul."),
      ("Hébreux 4:15", "Un souverain sacrificateur... a été tenté comme nous en toutes choses, sans commettre de péché."),
    ],
    dialogue_ref="Matthieu 4:4", dialogue_title="De toute parole",
    dialogue_quote="« Il est écrit : L'homme ne vivra pas de pain seulement, mais de toute parole qui sort de la bouche de Dieu. »",
    reflection="Avant tout miracle, avant toute foule, Jésus a d'abord été seul, affamé, et tenté. Il a répondu chaque fois par l'Écriture, pas par la puissance. Le disciple apprend la même arme : ce n'est pas la force qui tient au désert, c'est la Parole gardée près du cœur.",
    complete_blurb="Tu as traversé le Jourdain et le désert, et vu l'Esprit descendre comme une colombe.",
    gameover_title="Vaincu par la tentation", gameover_blurb="Même au désert, la grâce relève. Reprends la course.",
    next_slug="galilee", next_title="Niveau 17 : La Galilée",
  ),
  dict(
    n=17, slug="galilee", title="Paraboles et miracles", subtitle="Niveau 17 — La Galilée",
    favicon="🌿",
    accent="#7fbf7f", accent_soft="#b0e0b0", danger="#c9603e", aqua="#8fd6d0", leaf="#6fae5f",
    void="#131c14", void2="#1c261c", panel="#243224", arch1="#3a5a38", arch2="#182018", canvasbg="#8fd0e0",
    sky=("#6fc0e8","#9fdcc0","#d4eca0","#eef7c8"), hillfar="rgba(100,170,100,.45)", hillnear="rgba(60,120,56,.62)",
    ground=("#8fd68f","#5a9e5a","#264a24","#c0f0b0"),
    motif="growth", motif_colors=("#7fbf7f","#a8e098","#e8f7c8"),
    patrol_shape="humanoid", patrol_color="#3a3226", patrol_accent="#6a5a3e",
    static_color="#8a9a68", static_accent="#2e3a1e",
    start_title="Le long du lac", start_blurb="Marche avec la foule au bord du lac de Galilée, entre paraboles semées et miracles accomplis.",
    verses=[
      ("Marc 4:9", "Que celui qui a des oreilles pour entendre entende."),
      ("Matthieu 13:31", "Le royaume des cieux est semblable à un grain de sénevé... la plus petite de toutes les semences."),
      ("Marc 4:39", "Il se leva, menaça le vent, et dit à la mer : Silence ! tais-toi ! Et le vent cessa, et il y eut un grand calme."),
      ("Jean 6:35", "Je suis le pain de vie. Celui qui vient à moi n'aura jamais faim."),
      ("Luc 15:20", "Comme il était encore loin, son père le vit et fut ému de compassion, il courut se jeter à son cou."),
    ],
    dialogue_ref="Marc 4:39", dialogue_title="Silence, tais-toi",
    dialogue_quote="« Il se leva, menaça le vent, et dit à la mer : Silence ! tais-toi ! Et le vent cessa, et il y eut un grand calme. »",
    reflection="Jésus enseignait en paraboles pour ceux qui prenaient le temps de chercher, et il calmait des tempêtes pour ceux qui avaient cessé de croire qu'il s'en souciait. Le disciple découvre en Galilée un Royaume qui grandit en secret, et un Maître qui n'est jamais indifférent à la tempête.",
    complete_blurb="Tu as marché le long du lac et vu la tempête se taire à une parole.",
    gameover_title="Emporté par la tempête", gameover_blurb="Même dans la tempête, une parole relève. Reprends la course.",
    next_slug="croix", next_title="Niveau 18 : La croix",
  ),
  dict(
    n=18, slug="croix", title="La croix", subtitle="Niveau 18 — La croix",
    favicon="✝️",
    accent="#c9a878", accent_soft="#e8d0a8", danger="#8a1a1a", aqua="#8a8a98", leaf="#6a6a58",
    void="#0e0c0e", void2="#161214", panel="#1e181a", arch1="#3a2626", arch2="#120e10", canvasbg="#3a2c30",
    sky=("#241418","#4a2020","#7a3c2e","#a8683a"), hillfar="rgba(80,60,60,.5)", hillnear="rgba(50,36,36,.65)",
    ground=("#5a4a44","#3a2e28","#1a1412","#8a7268"),
    motif="cross", motif_colors=("#e8d0a8","#c9a878","#8a6a48"),
    patrol_shape="humanoid", patrol_color="#2e2422", patrol_accent="#6a4a3a",
    static_color="#7a6a5a", static_accent="#1a1412",
    start_title="Le chemin du Golgotha", start_blurb="Marche le chemin de la croix, sous le poids et les moqueries, jusqu'au Golgotha.",
    verses=[
      ("Ésaïe 53:5", "Il était blessé pour nos péchés, brisé pour nos iniquités... et c'est par ses meurtrissures que nous sommes guéris."),
      ("Luc 23:34", "Père, pardonne-leur, car ils ne savent ce qu'ils font."),
      ("Jean 19:30", "Quand Jésus eut pris le vinaigre, il dit : Tout est accompli. Et, baissant la tête, il rendit l'esprit."),
      ("Luc 23:43", "Je te le dis en vérité, aujourd'hui tu seras avec moi dans le paradis."),
      ("2 Corinthiens 5:21", "Celui qui n'a point connu le péché, il l'a fait devenir péché pour nous."),
    ],
    dialogue_ref="Jean 19:30", dialogue_title="Tout est accompli",
    dialogue_quote="« Quand Jésus eut pris le vinaigre, il dit : Tout est accompli. Et, baissant la tête, il rendit l'esprit. »",
    reflection="Ce n'est pas une défaite que la croix raconte, mais un accomplissement. Ce que ni la loi, ni le sacrifice, ni l'effort du disciple ne pouvaient finir, Jésus l'a fini lui-même, une fois pour toutes. Il n'y a rien à ajouter à « tout est accompli ».",
    complete_blurb="Tu as marché jusqu'au Golgotha et entendu : tout est accompli.",
    gameover_title="Écrasé sous le poids", gameover_blurb="Même sous la croix, la grâce relève. Reprends la course.",
    next_slug="resurrection", next_title="Niveau 19 : La résurrection",
  ),
  dict(
    n=19, slug="resurrection", title="La résurrection", subtitle="Niveau 19 — La résurrection",
    favicon="🌅",
    accent="#ffcf6e", accent_soft="#ffe6a8", danger="#8a4a3e", aqua="#a8d6c6", leaf="#8fc088",
    void="#161022", void2="#20182e", panel="#2a2038", arch1="#4a3860", arch2="#1a1428", canvasbg="#e8b878",
    sky=("#2e2050","#6a4880","#c98a6a","#ffcf8a"), hillfar="rgba(150,110,90,.45)", hillnear="rgba(100,70,58,.62)",
    ground=("#c9a068","#8a6a3e","#4a3620","#f0d08e"),
    motif="star", motif_colors=("#fff4d6","#ffcf6e","#ff9a4a"),
    patrol_shape="humanoid", patrol_color="#3a3040", patrol_accent="#6a5878",
    static_color="#8a8078", static_accent="#2a2032",
    start_title="Le tombeau vide", start_blurb="Marche vers le tombeau à l'aube du premier jour, entre le doute et la joie d'une nouvelle vie.",
    verses=[
      ("Matthieu 28:5", "Ne craignez pas, car je sais que vous cherchez Jésus qui a été crucifié. Il n'est point ici ; il est ressuscité, comme il l'avait dit."),
      ("Jean 20:16", "Jésus lui dit : Marie ! Elle se retourna, et lui dit en hébreu : Rabbouni !"),
      ("Luc 24:6", "Il n'est point ici, mais il est ressuscité. Souvenez-vous de quelle manière il vous a parlé..."),
      ("1 Corinthiens 15:20", "Christ est ressuscité des morts, il est les prémices de ceux qui sont morts."),
      ("Jean 20:29", "Heureux ceux qui n'ont pas vu, et qui ont cru !"),
    ],
    dialogue_ref="Matthieu 28:6", dialogue_title="Il n'est point ici",
    dialogue_quote="« Il n'est point ici ; il est ressuscité, comme il l'avait dit. Venez, voyez le lieu où il était couché. »",
    reflection="Le tombeau vide n'est pas la fin de l'histoire de la croix — c'est la preuve qu'elle a réellement fini quelque chose. Tout ce que le disciple traverse depuis Éden trouve ici sa réponse : la mort elle-même a été vaincue, et n'a plus le dernier mot.",
    complete_blurb="Tu as marché vers le tombeau à l'aube, et il était vide.",
    gameover_title="Retenu par le doute", gameover_blurb="Même dans le doute, le tombeau vide relève. Reprends la course.",
    next_slug="pentecote", next_title="Niveau 20 : La Pentecôte",
  ),
  dict(
    n=20, slug="pentecote", title="La Pentecôte", subtitle="Niveau 20 — La Pentecôte",
    favicon="🕊️",
    accent="#ff8a4a", accent_soft="#ffc07a", danger="#8a3a2e", aqua="#8fd6d0", leaf="#7fbf8f",
    void="#161014", void2="#20181c", panel="#2c2024", arch1="#5a3a34", arch2="#1c1416", canvasbg="#e07a4a",
    sky=("#3a1e3a","#7a3a4a","#c9683e","#ffb24a"), hillfar="rgba(150,90,60,.45)", hillnear="rgba(100,58,38,.62)",
    ground=("#c98a4e","#8a5a2e","#4a2e16","#f0b878"),
    motif="fire", motif_colors=("#ff5a2e","#ffa04a","#ffe1a3"),
    patrol_shape="humanoid", patrol_color="#3a2c28", patrol_accent="#8a4a3e",
    static_color="#9a8068", static_accent="#2a1e16",
    start_title="Jusqu'aux extrémités de la terre", start_blurb="Attends la promesse à Jérusalem, reçois les langues de feu, et pars — jusqu'aux extrémités de la terre.",
    verses=[
      ("Actes 1:8", "Vous recevrez une puissance, le Saint-Esprit survenant sur vous, et vous serez mes témoins... jusqu'aux extrémités de la terre."),
      ("Actes 2:3", "Des langues, semblables à des langues de feu, leur apparurent, séparées les unes des autres, et se posèrent sur chacun d'eux."),
      ("Actes 2:4", "Ils furent tous remplis du Saint-Esprit, et se mirent à parler en d'autres langues."),
      ("Actes 2:41", "Ceux qui acceptèrent sa parole furent baptisés ; et, en ce jour-là, le nombre des disciples s'augmenta d'environ trois mille âmes."),
      ("Matthieu 28:19", "Allez, faites de toutes les nations des disciples, les baptisant... et enseignez-leur à observer tout ce que je vous ai prescrit."),
    ],
    dialogue_ref="Actes 1:8", dialogue_title="Jusqu'aux extrémités de la terre",
    dialogue_quote="« Vous recevrez une puissance, le Saint-Esprit survenant sur vous, et vous serez mes témoins... jusqu'aux extrémités de la terre. »",
    reflection="D'Éden à la Pentecôte, le chemin a toujours mené vers l'extérieur : un jardin à garder, un peuple à délivrer, une terre à habiter, une bonne nouvelle à porter. Le disciple qui arrive ici n'arrive pas à une fin, mais à un envoi — la course continue, maintenant, par toi.",
    complete_blurb="Tu as reçu la promesse et vu naître une Église envoyée jusqu'aux extrémités de la terre.",
    gameover_title="Resté dans la chambre haute", gameover_blurb="Même avant l'envoi, la grâce relève. Reprends la course.",
    next_slug=None, next_title=None,
  ),
]

TEMPLATE = r'''<title>Faith Run — @@TITLE@@</title>
<meta name="viewport" content="width=device-width, initial-scale=1" />

<style>
  :root{
    --font-display:Georgia,'Iowan Old Style','Palatino Linotype','Times New Roman',serif;
    --font-body:-apple-system,BlinkMacSystemFont,'Segoe UI',Roboto,'Helvetica Neue',Arial,sans-serif;
    --font-mono:ui-monospace,'SF Mono','Cascadia Mono','Consolas','Liberation Mono',monospace;
    --bg-void:@@VOID@@;
    --bg-void-2:@@VOID2@@;
    --panel:@@PANEL@@;
    --gold:@@ACCENT@@;
    --gold-soft:@@ACCENT_SOFT@@;
    --aqua:@@AQUA@@;
    --leaf:@@LEAF@@;
    --parchment:#fbf1dc;
    --parchment-dim:#d9cdb0;
    --ink:#1c1410;
    --danger:@@DANGER@@;
    --line-gold: color-mix(in srgb, @@ACCENT@@ 35%, transparent);
  }

  *{box-sizing:border-box;}
  html,body{margin:0;padding:0;}

  body{
    background:
      radial-gradient(ellipse 60% 46% at 18% -6%, color-mix(in srgb, @@ACCENT@@ 16%, transparent) 0%, transparent 62%),
      radial-gradient(ellipse 70% 50% at 82% -8%, color-mix(in srgb, @@AQUA@@ 14%, transparent) 0%, transparent 62%),
      var(--bg-void);
    color:var(--parchment);
    font-family:var(--font-body);
    min-height:100vh;
    display:flex;
    justify-content:center;
    padding:clamp(20px,4vw,48px) 16px 40px;
  }

  .page{width:100%;max-width:1040px;display:flex;flex-direction:column;align-items:center;gap:18px;}
  header{text-align:center;display:flex;flex-direction:column;align-items:center;gap:6px;}
  .kicker{font-family:var(--font-body);font-weight:700;font-size:.72rem;letter-spacing:.28em;text-transform:uppercase;color:var(--aqua);opacity:.9;}
  h1{font-family:var(--font-display);font-weight:700;font-size:clamp(2.1rem,5vw + .5rem,3.4rem);letter-spacing:.03em;margin:0;text-wrap:balance;color:var(--parchment);text-shadow:0 0 30px color-mix(in srgb, @@ACCENT@@ 35%, transparent);}
  .subtitle{font-family:var(--font-body);font-weight:600;font-size:.95rem;letter-spacing:.1em;text-transform:uppercase;color:var(--gold-soft);}

  .hud{width:100%;max-width:820px;display:flex;align-items:center;gap:16px;padding:10px 16px;background:linear-gradient(180deg,var(--panel),var(--bg-void-2));border:1px solid color-mix(in srgb, @@ACCENT@@ 22%, transparent);border-radius:10px;box-shadow:0 10px 30px -14px rgba(0,0,0,.6);}
  .hearts{display:flex;gap:6px;flex-shrink:0;}
  .hearts svg{width:20px;height:20px;transition:opacity .3s;}
  .hearts svg path{fill:var(--gold-soft);stroke:var(--gold);stroke-width:1;}
  .hearts svg.lost path{fill:transparent;stroke:rgba(251,241,220,.3);}
  .hud-sep{width:1px;align-self:stretch;background:rgba(251,241,220,.14);}
  .verse-count{font-family:var(--font-mono);font-weight:600;font-size:.85rem;color:var(--parchment-dim);white-space:nowrap;flex-shrink:0;}
  .verse-count b{color:var(--gold-soft);font-weight:600;}
  .progress-track{flex:1;height:6px;border-radius:4px;background:rgba(251,241,220,.12);overflow:hidden;}
  .progress-fill{height:100%;width:0%;background:linear-gradient(90deg,var(--leaf),var(--gold));border-radius:4px;transition:width .15s linear;}
  .icon-btn{flex-shrink:0;width:30px;height:30px;border-radius:8px;border:1px solid color-mix(in srgb, @@ACCENT@@ 25%, transparent);background:color-mix(in srgb, @@ACCENT@@ 8%, transparent);color:var(--gold-soft);cursor:pointer;display:flex;align-items:center;justify-content:center;}
  .icon-btn svg{width:16px;height:16px;}
  .icon-btn .wave{transition:opacity .15s;}
  .icon-btn.muted .wave{opacity:0;}
  .icon-btn:focus-visible{outline:2px solid var(--aqua);outline-offset:2px;}

  .stage-wrap{position:relative;width:100%;max-width:960px;}
  .arch{position:relative;width:100%;aspect-ratio:960/540;border-radius:46% 46% 10px 10px / 20% 20% 6px 6px;padding:10px;background:linear-gradient(180deg,@@ARCH1@@,@@ARCH2@@);box-shadow:0 0 0 1px color-mix(in srgb, @@ACCENT@@ 30%, transparent),0 0 0 8px var(--bg-void-2),0 0 0 9px color-mix(in srgb, @@ACCENT@@ 16%, transparent),0 30px 70px -20px rgba(0,0,0,.7);}
  .canvas-mask{position:relative;width:100%;height:100%;border-radius:44% 44% 4px 4px / 18% 18% 4px 4px;overflow:hidden;background:@@CANVASBG@@;}
  canvas#game{display:block;width:100%;height:100%;}

  #toastLayer{position:absolute;top:14px;left:0;right:0;display:flex;flex-direction:column;align-items:center;gap:6px;pointer-events:none;z-index:5;}
  .toast{font-family:var(--font-body);font-size:.78rem;font-weight:600;color:var(--ink);background:linear-gradient(180deg,var(--gold-soft),var(--gold));padding:6px 14px;border-radius:20px;box-shadow:0 6px 18px -6px rgba(0,0,0,.5);animation:toastFade 2.4s ease forwards;max-width:80%;text-align:center;}
  @keyframes toastFade{0%{opacity:0;transform:translateY(-6px) scale(.94);}10%{opacity:1;transform:translateY(0) scale(1);}80%{opacity:1;}100%{opacity:0;transform:translateY(-4px) scale(1);}}

  .overlay{position:absolute;inset:0;display:flex;align-items:center;justify-content:center;padding:22px;background:radial-gradient(ellipse at 50% 40%, rgba(20,16,14,.5), rgba(10,8,6,.88));z-index:10;text-align:center;}
  .overlay[hidden]{display:none;}
  .scroll-panel{max-width:520px;max-height:100%;overflow-y:auto;-webkit-overflow-scrolling:touch;background:linear-gradient(180deg,@@ARCH2@@,var(--ink));border:1px solid var(--line-gold);border-radius:14px;padding:clamp(16px,4vw,34px);box-shadow:0 20px 60px -20px rgba(0,0,0,.7), inset 0 0 40px color-mix(in srgb, @@ACCENT@@ 8%, transparent);}
  .scroll-panel h2{font-family:var(--font-display);font-weight:600;font-size:1.5rem;color:var(--gold-soft);margin:0 0 12px;}
  .scroll-panel p{font-size:.95rem;line-height:1.6;color:var(--parchment);margin:0 0 10px;}
  .scroll-panel p.ref{font-family:var(--font-mono);font-size:.72rem;letter-spacing:.08em;text-transform:uppercase;color:var(--aqua);margin-bottom:4px;}
  .stat-row{display:flex;justify-content:center;gap:22px;margin:16px 0;font-family:var(--font-mono);font-size:.8rem;color:var(--parchment-dim);flex-wrap:wrap;}
  .stat-row b{display:block;color:var(--gold-soft);font-size:1.1rem;}
  button.cta{font-family:var(--font-body);font-weight:700;font-size:.9rem;letter-spacing:.03em;color:var(--ink);background:linear-gradient(180deg,var(--gold-soft),var(--gold));border:none;padding:12px 26px;border-radius:8px;cursor:pointer;margin-top:6px;box-shadow:0 8px 22px -8px color-mix(in srgb, @@ACCENT@@ 55%, transparent);transition:transform .15s ease, box-shadow .15s ease;}
  button.cta:hover{transform:translateY(-1px);}
  button.cta:focus-visible{outline:2px solid var(--aqua);outline-offset:3px;}
  .next-note{margin-top:14px;font-size:.72rem;letter-spacing:.05em;color:var(--parchment-dim);opacity:.75;}
  .hub-link{font-size:.72rem;color:var(--parchment-dim);text-decoration:none;letter-spacing:.03em;border-bottom:1px solid transparent;transition:border-color .15s ease, color .15s ease;}
  .hub-link:hover{color:var(--gold-soft);border-color:var(--gold-soft);}
  .nav-links{display:flex;justify-content:center;gap:16px;margin-top:14px;flex-wrap:wrap;}
  .nav-links a{font-size:.78rem;font-weight:600;color:var(--gold-soft);text-decoration:none;border-bottom:1px solid var(--line-gold);padding-bottom:1px;}
  .nav-links a:hover{color:var(--gold);}

  .legend{display:flex;align-items:center;justify-content:center;gap:10px;flex-wrap:wrap;font-family:var(--font-mono);font-size:.74rem;color:var(--parchment-dim);letter-spacing:.02em;}
  .legend kbd{font-family:var(--font-mono);background:var(--panel);border:1px solid rgba(251,241,220,.18);border-bottom-width:2px;padding:2px 7px;border-radius:5px;color:var(--parchment);font-size:.72rem;}

  .touchpad{display:none;width:100%;max-width:960px;justify-content:space-between;align-items:flex-end;gap:14px;}
  @media (hover:none) and (pointer:coarse){.touchpad{display:flex;}}
  .touch-move{display:flex;gap:10px;}
  .touch-btn{user-select:none;-webkit-user-select:none;touch-action:none;width:56px;height:56px;border-radius:50%;background:linear-gradient(180deg,var(--panel),var(--bg-void-2));border:1px solid color-mix(in srgb, @@ACCENT@@ 30%, transparent);color:var(--gold-soft);font-size:1.3rem;display:flex;align-items:center;justify-content:center;font-family:var(--font-body);}
  .touch-btn:active{background:var(--panel);}
  .touch-btn.jump{width:72px;height:72px;font-size:.85rem;font-weight:700;}

  footer{font-size:.72rem;color:var(--parchment-dim);opacity:.65;text-align:center;max-width:520px;}
  @media (prefers-reduced-motion:reduce){.toast{animation:none;opacity:1;}button.cta{transition:none;}}
</style>

<div class="page">
  <header>
    <span class="kicker">Faith Run · Prototype jouable</span>
    <h1>FAITH RUN</h1>
    <span class="subtitle">@@SUBTITLE@@</span>
    <a class="hub-link" href="faith-run-hub.html">← Tous les niveaux</a>
  </header>

  <div class="hud">
    <div class="hearts" id="hearts"></div>
    <div class="hud-sep"></div>
    <div class="verse-count">Versets&nbsp;<b id="verseCount">0</b>/5</div>
    <div class="progress-track"><div class="progress-fill" id="progressFill"></div></div>
    <button class="icon-btn" id="btnMute" aria-label="Couper le son" title="Couper / activer le son">
      <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
        <polygon points="4,9 8,9 13,4.5 13,19.5 8,15 4,15" fill="currentColor" stroke="none"></polygon>
        <path class="wave" d="M16.5 8.5a5 5 0 0 1 0 7"></path>
        <path class="wave" d="M19 6a8.5 8.5 0 0 1 0 12"></path>
      </svg>
    </button>
  </div>

  <div class="stage-wrap">
    <div class="arch">
      <div class="canvas-mask">
        <canvas id="game"></canvas>
        <div id="toastLayer"></div>

        <div class="overlay" id="overlayStart">
          <div class="scroll-panel">
            <h2>@@START_TITLE@@</h2>
            <p>@@START_BLURB@@</p>
            <p style="font-size:.8rem;color:var(--parchment-dim);">← → ou A/D pour marcher · Espace / ↑ pour sauter</p>
            <button class="cta" id="btnStart">Commencer la course</button>
          </div>
        </div>

        <div class="overlay" id="overlayDialogue" hidden>
          <div class="scroll-panel">
            <p class="ref">@@DIALOGUE_REF@@ — Louis Segond</p>
            <h2>@@DIALOGUE_TITLE@@</h2>
            <p>@@DIALOGUE_QUOTE@@</p>
            <p style="font-size:.85rem;color:var(--parchment-dim);">@@DIALOGUE_REFLECTION@@</p>
            <div class="stat-row"><div><b id="dlgVerses">0/5</b>versets recueillis</div></div>
            <button class="cta" id="btnContinue">Continuer</button>
          </div>
        </div>

        <div class="overlay" id="overlayComplete" hidden>
          <div class="scroll-panel">
            <h2>Niveau accompli</h2>
            <p>@@COMPLETE_BLURB@@</p>
            <div class="stat-row">
              <div><b id="sumVerses">0/5</b>versets</div>
              <div><b id="sumHearts">3/3</b>cœurs</div>
              <div><b id="sumTime">0:00</b>temps</div>
            </div>
            <button class="cta" id="btnReplayWin">Rejouer le niveau</button>
            @@NEXT_NAV@@
          </div>
        </div>

        <div class="overlay" id="overlayGameover" hidden>
          <div class="scroll-panel">
            <h2>@@GAMEOVER_TITLE@@</h2>
            <p>@@GAMEOVER_BLURB@@</p>
            <button class="cta" id="btnRetry">Réessayer</button>
          </div>
        </div>
      </div>
    </div>
  </div>

  <div class="legend">
    <span><kbd>←</kbd><kbd>→</kbd> ou <kbd>A</kbd><kbd>D</kbd> marcher</span>
    <span><kbd>Espace</kbd> / <kbd>↑</kbd> sauter</span>
  </div>

  <div class="touchpad">
    <div class="touch-move">
      <div class="touch-btn" id="tLeft">◀</div>
      <div class="touch-btn" id="tRight">▶</div>
    </div>
    <div class="touch-btn jump" id="tJump">SAUT</div>
  </div>

  <footer>Textes bibliques : version Louis Segond (1910, domaine public). Sons générés en direct (Web Audio), aucun fichier externe.</footer>
</div>

<script>
(function(){
  "use strict";
  const THEME = @@THEME_JS@@;

  const canvas = document.getElementById('game');
  const ctx = canvas.getContext('2d');
  const W = 960, H = 540;
  function fitCanvas(){
    const dpr = Math.min(window.devicePixelRatio || 1, 2);
    canvas.width = W * dpr; canvas.height = H * dpr;
    ctx.setTransform(dpr,0,0,dpr,0,0);
  }
  fitCanvas();
  window.addEventListener('resize', fitCanvas);

  let audioCtx=null, masterGain=null, muted=false;
  function ensureAudio(){
    if(audioCtx){ if(audioCtx.state==='suspended') audioCtx.resume(); return; }
    try{
      audioCtx = new (window.AudioContext||window.webkitAudioContext)();
      masterGain = audioCtx.createGain();
      masterGain.gain.value = muted?0:0.85;
      masterGain.connect(audioCtx.destination);
    }catch(e){ audioCtx=null; }
  }
  function tone(freq,dur,type,gainPeak,delay,freqEnd){
    if(!audioCtx||muted) return;
    const t0 = audioCtx.currentTime+(delay||0);
    const osc = audioCtx.createOscillator();
    const gain = audioCtx.createGain();
    osc.type = type||'sine';
    osc.frequency.setValueAtTime(freq,t0);
    if(freqEnd) osc.frequency.exponentialRampToValueAtTime(Math.max(freqEnd,1),t0+dur);
    gain.gain.setValueAtTime(0.0001,t0);
    gain.gain.linearRampToValueAtTime(gainPeak,t0+0.012);
    gain.gain.exponentialRampToValueAtTime(0.0001,t0+dur);
    osc.connect(gain).connect(masterGain);
    osc.start(t0); osc.stop(t0+dur+0.03);
  }
  function noiseBurst(dur,gainPeak,filterType,filterFreq,delay){
    if(!audioCtx||muted) return;
    const t0 = audioCtx.currentTime+(delay||0);
    const n = Math.max(1,Math.floor(audioCtx.sampleRate*dur));
    const buf = audioCtx.createBuffer(1,n,audioCtx.sampleRate);
    const data = buf.getChannelData(0);
    for(let i=0;i<n;i++) data[i] = (Math.random()*2-1)*(1-i/n);
    const src = audioCtx.createBufferSource();
    src.buffer = buf;
    const filt = audioCtx.createBiquadFilter();
    filt.type = filterType||'lowpass';
    filt.frequency.value = filterFreq||600;
    const gain = audioCtx.createGain();
    gain.gain.setValueAtTime(gainPeak,t0);
    gain.gain.exponentialRampToValueAtTime(0.0001,t0+dur);
    src.connect(filt).connect(gain).connect(masterGain);
    src.start(t0);
  }
  const sfx = {
    click(){ ensureAudio(); tone(640,0.05,'square',0.08,0,900); },
    jump(){ tone(420,0.14,'square',0.14,0,760); },
    land(){ noiseBurst(0.12,0.16,'lowpass',260); tone(110,0.1,'sine',0.1,0,55); },
    collect(){ tone(880,0.16,'sine',0.18,0); tone(1108.73,0.16,'sine',0.16,0.06); tone(1318.51,0.22,'sine',0.14,0.12); },
    hit(){ noiseBurst(0.16,0.22,'bandpass',700); tone(180,0.22,'sawtooth',0.16,0,70); },
    chime(){ tone(523.25,1.3,'sine',0.09,0); tone(659.25,1.3,'sine',0.08,0.12); tone(783.99,1.5,'sine',0.07,0.24); },
    fanfare(){ tone(523.25,0.18,'triangle',0.2,0); tone(659.25,0.18,'triangle',0.2,0.15); tone(783.99,0.18,'triangle',0.2,0.3); tone(1046.5,0.4,'triangle',0.22,0.46); },
    heal(){ tone(523.25,0.2,'triangle',0.14,0); tone(659.25,0.24,'triangle',0.13,0.1); tone(783.99,0.32,'triangle',0.12,0.2); }
  };
  const btnMute = document.getElementById('btnMute');
  btnMute.addEventListener('click', ()=>{
    ensureAudio();
    muted = !muted;
    if(masterGain) masterGain.gain.setValueAtTime(muted?0:0.85, audioCtx.currentTime);
    btnMute.classList.toggle('muted', muted);
  });

  const GROUND_Y = 440;
  const GAPS = [ {x0:1700,x1:2000} ];
  const WORLD_W = 4000;
  const GROUND_SEGMENTS = (function(){
    const pts = [0, ...GAPS.flatMap(g=>[g.x0,g.x1]), WORLD_W];
    const segs = [];
    for(let i=0;i<pts.length;i+=2){ segs.push({x0:pts[i],x1:pts[i+1]}); }
    return segs;
  })();

  const PLATFORMS = [
    {x:850,y:360,w:110,h:18},{x:1020,y:300,w:100,h:18},
    {x:1720,y:410,w:70,h:18},{x:1830,y:378,w:70,h:18},{x:1940,y:410,w:70,h:18},
    {x:2220,y:380,w:90,h:18},{x:2380,y:322,w:90,h:18},{x:2540,y:380,w:110,h:18},
    {x:2740,y:322,w:90,h:18},{x:2900,y:380,w:90,h:18},{x:3580,y:392,w:90,h:18}
  ];

  const OBSTACLES = [
    {x:560,y:412,w:44,h:28,type:'static'},
    {x:2170,y:412,w:50,h:28,type:'static'},
    {x:3300,y:412,w:50,h:28,type:'static'},
    {x:2560,y: THEME.patrolShape==='humanoid'?320:352, w:34, h: THEME.patrolShape==='humanoid'?60:22, type:'patrol',range:[2540,2610],speed:42,dir:1}
  ];

  const VERSES = @@VERSES_JS@@.map((v,i)=>({...v, id:i, collected:false}));

  const LANDMARK_X = 3800;
  const LANDMARK_TRIGGER_X = 3760;
  const CHECKPOINTS = [0,780,2010,3130,3560];
  const HIDDEN_HEART = {x:1075,y:225,collected:false};

  const CLOUDS = Array.from({length:8},(_,i)=>({
    x: i*520 + (i*137)%210, y: 46+((i*71)%110),
    scale: 0.7+((i*53)%60)/100, parallax: 0.1+((i*31)%9)/100
  }));

  const HEARTS_START = 3, HEARTS_MAX = 5, HEART_REGEN_DELAY = 14;
  const player = { x:60, y:GROUND_Y-42, prevY:GROUND_Y-42, w:26, h:42, vx:0, vy:0, facing:1, onGround:true, hearts:HEARTS_START, invuln:0, running:0 };
  let camera = {x:0};
  let lastCheckpoint = 0, collectedCount = 0, gameState = 'start', startTime = 0, elapsed = 0;
  let raf = null, lastT = 0, shake = 0, particles = [], dialogueSoundPlayed = false, noHitTimer = 0;
  const keys = new Set();
  let touchLeft=false, touchRight=false;
  const GRAVITY = 1900, MOVE_SPEED = 230, JUMP_V = -640, MAX_FALL = 900;

  function spawnBurst(x,y,opts){
    opts = opts||{};
    const count = opts.count||10, color = opts.color||'#ffd873', speed = opts.speed||120;
    const life = opts.life||0.6, size = opts.size||3, gravity = opts.gravity!==undefined?opts.gravity:400;
    const spread = opts.spread!==undefined?opts.spread:Math.PI*2, angleBase = opts.angleBase||0;
    for(let i=0;i<count;i++){
      const ang = angleBase + (Math.random()-0.5)*spread;
      const spd = speed*(0.4+Math.random()*0.8);
      particles.push({x,y, vx:Math.cos(ang)*spd, vy:Math.sin(ang)*spd - 30, life, maxLife:life, size:size*(0.6+Math.random()*0.9), color, gravity});
    }
    if(particles.length>240) particles.splice(0, particles.length-240);
  }
  function updateParticles(dt){
    for(let i=particles.length-1;i>=0;i--){
      const p=particles[i];
      p.vy += p.gravity*dt; p.x += p.vx*dt; p.y += p.vy*dt; p.life -= dt;
      if(p.life<=0) particles.splice(i,1);
    }
  }
  function drawParticles(){
    for(const p of particles){
      const sx = p.x-camera.x, sy=p.y;
      if(sx<-20||sx>W+20) continue;
      ctx.globalAlpha = Math.max(0,p.life/p.maxLife);
      ctx.fillStyle = p.color;
      ctx.beginPath(); ctx.arc(sx,sy,Math.max(0.4,p.size),0,Math.PI*2); ctx.fill();
    }
    ctx.globalAlpha = 1;
  }
  function triggerShake(mag){ shake = Math.max(shake, mag); }

  window.addEventListener('keydown', (e)=>{
    if(['ArrowLeft','ArrowRight','ArrowUp',' ','Space','KeyA','KeyD','KeyW'].includes(e.code) || e.key===' '){ e.preventDefault(); }
    keys.add(e.code);
    if((e.code==='Space'||e.code==='ArrowUp'||e.code==='KeyW')){ ensureAudio(); tryJump(); }
    if(gameState==='start' && (e.code==='Space'||e.code==='Enter')) startGame();
  }, {passive:false});
  window.addEventListener('keyup', (e)=>keys.delete(e.code));
  function bindTouch(el, onDown, onUp){
    el.addEventListener('pointerdown', (e)=>{ e.preventDefault(); ensureAudio(); onDown(); }, {passive:false});
    ['pointerup','pointerleave','pointercancel'].forEach(ev=>el.addEventListener(ev, ()=>onUp && onUp()));
  }
  bindTouch(document.getElementById('tLeft'), ()=>touchLeft=true, ()=>touchLeft=false);
  bindTouch(document.getElementById('tRight'), ()=>touchRight=true, ()=>touchRight=false);
  bindTouch(document.getElementById('tJump'), ()=>{ tryJump(); if(gameState==='start') startGame(); });

  function tryJump(){
    if(gameState!=='playing') return;
    if(player.onGround){
      player.vy = JUMP_V; player.onGround = false;
      spawnBurst(player.x+player.w/2, player.y+player.h, {count:7,color:'#fff3d6',speed:70,life:0.35,size:2.4,gravity:500,spread:1.4,angleBase:-Math.PI/2});
      sfx.jump();
    }
  }
  function overlap(a,b){ return a.x < b.x+b.w && a.x+a.w > b.x && a.y < b.y+b.h && a.y+a.h > b.y; }

  function heartHit(){
    if(player.invuln>0) return;
    player.hearts--; player.invuln = 1.1; noHitTimer = 0;
    player.x = Math.max(0, player.x - player.facing*26);
    player.vy = -320;
    spawnBurst(player.x+player.w/2, player.y+player.h/2, {count:14,color:'#ff6a54',speed:180,life:0.5,size:3,gravity:600});
    triggerShake(9); sfx.hit(); updateHUD();
    if(player.hearts<=0){ gameState = 'gameover'; show('overlayGameover'); }
  }
  function gainHeart(reason){
    if(player.hearts>=HEARTS_MAX) return false;
    player.hearts++;
    spawnBurst(player.x+player.w/2, player.y+player.h/2, {count:16,color:'#ff8fae',speed:130,life:0.6,size:3,gravity:150,spread:Math.PI*2});
    sfx.heal();
    const messages = {milestone:'Cœur bonus — palier de versets atteint !',regen:'Cœur restauré par le repos.',hidden:'Cœur caché trouvé !'};
    showToast(messages[reason]||'Cœur restauré.');
    updateHUD();
    return true;
  }
  function respawnAtCheckpoint(){
    player.x = lastCheckpoint + 20; player.y = GROUND_Y - player.h;
    player.vx = 0; player.vy = 0; player.invuln = 1.0;
    heartHit();
  }
  function show(id){ document.getElementById(id).hidden = false; }
  function hide(id){ document.getElementById(id).hidden = true; }
  function updateHUD(){
    const heartsEl = document.getElementById('hearts');
    heartsEl.innerHTML = '';
    for(let i=0;i<HEARTS_MAX;i++){
      const lost = i >= player.hearts;
      heartsEl.insertAdjacentHTML('beforeend', `<svg viewBox="0 0 24 24" class="${lost?'lost':''}"><path d="M12 21s-7.5-4.7-10-9.3C.4 8.4 2 5 5.4 5c2 0 3.4 1.1 4.1 2.2C10.2 6.1 11.6 5 13.6 5 17 5 18.6 8.4 17 11.7 15 16.3 12 21 12 21z"/></svg>`);
    }
    document.getElementById('verseCount').textContent = collectedCount;
    const pct = Math.min(100, Math.max(0, (player.x/WORLD_W)*100));
    document.getElementById('progressFill').style.width = pct+'%';
  }
  function showToast(text){
    const layer = document.getElementById('toastLayer');
    const div = document.createElement('div');
    div.className = 'toast'; div.textContent = text;
    layer.appendChild(div);
    setTimeout(()=>div.remove(), 2500);
  }
  function toast(ref, text){ showToast(ref + ' — ' + text); }
  function fmtTime(s){ const m = Math.floor(s/60), r = Math.floor(s%60); return m+':'+String(r).padStart(2,'0'); }

  function startGame(){ ensureAudio(); sfx.click(); hide('overlayStart'); gameState = 'playing'; startTime = performance.now(); }
  function resetAll(){
    player.x=60; player.y=GROUND_Y-42; player.prevY=player.y;
    player.vx=0; player.vy=0; player.facing=1; player.onGround=true;
    player.hearts=HEARTS_START; player.invuln=0;
    camera.x=0; lastCheckpoint=0; collectedCount=0; noHitTimer=0;
    particles = []; shake = 0; dialogueSoundPlayed = false;
    VERSES.forEach(v=>v.collected=false);
    HIDDEN_HEART.collected = false;
    OBSTACLES.forEach(o=>{ if(o.type==='patrol'){ o.x=o.range[0]; o.dir=1; }});
    updateHUD();
  }
  document.getElementById('btnStart').addEventListener('click', startGame);
  document.getElementById('btnContinue').addEventListener('click', ()=>{
    sfx.click(); hide('overlayDialogue');
    elapsed = (performance.now()-startTime)/1000;
    document.getElementById('sumVerses').textContent = collectedCount+'/5';
    document.getElementById('sumHearts').textContent = player.hearts+'/'+HEARTS_MAX;
    document.getElementById('sumTime').textContent = fmtTime(elapsed);
    gameState = 'complete'; show('overlayComplete'); sfx.fanfare();
  });
  document.getElementById('btnReplayWin').addEventListener('click', ()=>{ sfx.click(); hide('overlayComplete'); resetAll(); startGame(); });
  document.getElementById('btnRetry').addEventListener('click', ()=>{ sfx.click(); hide('overlayGameover'); resetAll(); startGame(); });

  function update(dt){
    updateParticles(dt);
    if(shake>0) shake = Math.max(0, shake - dt*46);
    if(gameState!=='playing'){ return; }

    let moveDir = 0;
    if(keys.has('ArrowLeft')||keys.has('KeyA')||touchLeft) moveDir -= 1;
    if(keys.has('ArrowRight')||keys.has('KeyD')||touchRight) moveDir += 1;
    player.vx = moveDir * MOVE_SPEED;
    if(moveDir!==0) player.facing = moveDir;
    player.running = moveDir!==0 ? player.running+dt : 0;

    const wasOnGround = player.onGround, prevVy = player.vy;
    player.prevY = player.y;
    player.vy = Math.min(player.vy + GRAVITY*dt, MAX_FALL);
    player.x += player.vx*dt; player.y += player.vy*dt;
    player.x = Math.max(0, Math.min(player.x, WORLD_W-player.w));

    player.onGround = false;
    for(const seg of GROUND_SEGMENTS){
      if(player.x+player.w>seg.x0 && player.x<seg.x1){
        const feetY = player.y+player.h, prevFeetY = player.prevY+player.h;
        if(player.vy>=0 && prevFeetY<=GROUND_Y+1 && feetY>=GROUND_Y){ player.y = GROUND_Y-player.h; player.vy=0; player.onGround=true; }
      }
    }
    for(const p of PLATFORMS){
      if(player.x+player.w>p.x && player.x<p.x+p.w){
        const feetY = player.y+player.h, prevFeetY = player.prevY+player.h;
        if(player.vy>=0 && prevFeetY<=p.y+1 && feetY>=p.y && feetY<=p.y+16){ player.y = p.y-player.h; player.vy=0; player.onGround=true; }
      }
    }
    if(!wasOnGround && player.onGround && prevVy>260){
      spawnBurst(player.x+player.w/2, player.y+player.h, {count:9,color:'#fff3d6',speed:80,life:0.4,size:2.6,gravity:500,spread:1.6,angleBase:-Math.PI/2});
      sfx.land();
    }
    if(player.y > H+60){ respawnAtCheckpoint(); }
    if(player.onGround){ for(const c of CHECKPOINTS){ if(player.x>=c) lastCheckpoint = Math.max(lastCheckpoint, c); } }
    if(player.invuln>0) player.invuln = Math.max(0, player.invuln-dt);

    noHitTimer += dt;
    if(noHitTimer>=HEART_REGEN_DELAY){ noHitTimer = 0; gainHeart('regen'); }

    for(const o of OBSTACLES){
      if(o.type==='patrol'){
        o.x += o.dir*o.speed*dt;
        if(o.x<o.range[0]){ o.x=o.range[0]; o.dir=1; }
        if(o.x>o.range[1]){ o.x=o.range[1]; o.dir=-1; }
      }
      if(overlap(player,o)) heartHit();
    }
    for(const v of VERSES){
      if(v.collected) continue;
      const vb = {x:v.x-14,y:v.y-14,w:28,h:28};
      if(overlap(player,vb)){
        v.collected = true; collectedCount++;
        toast(v.ref, v.text);
        spawnBurst(v.x, v.y, {count:18,color:'#ffd873',speed:150,life:0.7,size:3,gravity:200,spread:Math.PI*2});
        sfx.collect();
        if(collectedCount%2===0) gainHeart('milestone');
        updateHUD();
      }
    }
    if(!HIDDEN_HEART.collected){
      const hb = {x:HIDDEN_HEART.x-14,y:HIDDEN_HEART.y-14,w:28,h:28};
      if(overlap(player,hb)){
        HIDDEN_HEART.collected = true;
        if(!gainHeart('hidden')) showToast('Cœur caché trouvé — cœurs déjà au maximum.');
      }
    }
    camera.x = Math.max(0, Math.min(player.x - W/2, WORLD_W-W));
    if(player.x > LANDMARK_TRIGGER_X){
      gameState = 'dialogue';
      document.getElementById('dlgVerses').textContent = collectedCount+'/5';
      show('overlayDialogue');
      if(!dialogueSoundPlayed){
        dialogueSoundPlayed = true;
        spawnBurst(player.x, player.y, {count:26,color: THEME.motifColors[0],speed:130,life:1.0,size:3.2,gravity:100,spread:Math.PI*2});
        sfx.chime();
      }
    }
    updateHUD();
  }

  const AMBIENT = Array.from({length:26},(_,i)=>({
    x:Math.random()*WORLD_W, y:50+Math.random()*380,
    r:1.1+Math.random()*2, phase:Math.random()*Math.PI*2, speed:.4+Math.random()*.6
  }));

  function drawSky(t){
    const g = ctx.createLinearGradient(0,0,0,H);
    g.addColorStop(0,THEME.sky[0]); g.addColorStop(.42,THEME.sky[1]); g.addColorStop(.72,THEME.sky[2]); g.addColorStop(1,THEME.sky[3]);
    ctx.fillStyle = g; ctx.fillRect(0,0,W,H);

    const sunX = 740 - camera.x*0.15;
    const sunG = ctx.createRadialGradient(sunX,100,6,sunX,100,220);
    sunG.addColorStop(0,'rgba(255,250,225,.85)'); sunG.addColorStop(.4,'rgba(255,224,150,.45)'); sunG.addColorStop(1,'rgba(255,224,150,0)');
    ctx.fillStyle = sunG; ctx.fillRect(0,0,W,H);

    ctx.fillStyle = 'rgba(255,255,255,.7)';
    for(const c of CLOUDS){
      const sx = c.x - camera.x*c.parallax;
      if(sx<-150||sx>W+150) continue;
      const s = c.scale;
      ctx.beginPath();
      ctx.ellipse(sx,c.y,24*s,14*s,0,0,Math.PI*2);
      ctx.ellipse(sx+19*s,c.y-6*s,17*s,12*s,0,0,Math.PI*2);
      ctx.ellipse(sx-19*s,c.y-3*s,15*s,11*s,0,0,Math.PI*2);
      ctx.fill();
    }

    const hillOffset = -(camera.x*0.3)%240;
    ctx.fillStyle = THEME.hillFar;
    for(let i=-1;i<6;i++){ ctx.beginPath(); ctx.ellipse(hillOffset+i*240,H-84,170,90,0,Math.PI,0); ctx.fill(); }
    ctx.fillStyle = THEME.hillNear;
    const hillOffset2 = -(camera.x*0.45)%200;
    for(let i=-1;i<7;i++){ ctx.beginPath(); ctx.ellipse(hillOffset2+i*200,H-70,140,74,0,Math.PI,0); ctx.fill(); }

    for(const a of AMBIENT){
      const sx = a.x - camera.x;
      if(sx<-20||sx>W+20) continue;
      const sy = a.y + Math.sin(t*a.speed + a.phase)*10;
      ctx.globalAlpha = .3+.3*Math.sin(t*a.speed+a.phase);
      ctx.fillStyle = 'rgba(255,244,210,.9)';
      ctx.beginPath(); ctx.arc(sx,sy,a.r,0,Math.PI*2); ctx.fill();
    }
    ctx.globalAlpha = 1;
  }

  function drawGround(){
    for(const seg of GROUND_SEGMENTS){
      const sx = seg.x0-camera.x, ex = seg.x1-camera.x;
      if(ex<0||sx>W) continue;
      const g = ctx.createLinearGradient(0,GROUND_Y,0,H);
      g.addColorStop(0,THEME.ground[0]); g.addColorStop(.16,THEME.ground[1]); g.addColorStop(1,THEME.ground[2]);
      ctx.fillStyle = g; ctx.fillRect(sx,GROUND_Y,ex-sx,H-GROUND_Y);
      ctx.fillStyle = THEME.ground[3]; ctx.fillRect(sx,GROUND_Y,ex-sx,6);
    }
    for(const gap of GAPS){
      const sx=gap.x0-camera.x, ex=gap.x1-camera.x;
      if(ex<0||sx>W) continue;
      ctx.fillStyle = 'rgba(111,180,224,.55)';
      ctx.fillRect(sx,GROUND_Y+20,ex-sx,H-GROUND_Y-20);
      ctx.strokeStyle = 'rgba(255,255,255,.5)'; ctx.lineWidth = 2;
      for(let i=0;i<3;i++){
        const yy = GROUND_Y+40+i*24;
        ctx.beginPath();
        for(let x=sx;x<ex;x+=6){
          const yOff = Math.sin(x*0.08 + performance.now()/400 + i)*3;
          x===sx ? ctx.moveTo(x,yy+yOff) : ctx.lineTo(x,yy+yOff);
        }
        ctx.stroke();
      }
    }
  }

  function drawPlatforms(){
    for(const p of PLATFORMS){
      const sx = p.x-camera.x;
      if(sx+p.w<0||sx>W) continue;
      ctx.fillStyle = '#8a7454'; ctx.fillRect(sx,p.y+6,p.w,p.h-6);
      ctx.fillStyle = THEME.accent; ctx.fillRect(sx,p.y,p.w,8);
    }
  }

  function drawObstacles(){
    for(const o of OBSTACLES){
      const sx = o.x-camera.x;
      if(sx+o.w<0||sx>W) continue;
      if(o.type==='static'){
        for(let i=0;i<o.w;i+=11){
          ctx.fillStyle = THEME.staticColor;
          ctx.beginPath();
          ctx.moveTo(sx+i,o.y+o.h); ctx.lineTo(sx+i+5.5,o.y); ctx.lineTo(sx+i+11,o.y+o.h);
          ctx.closePath(); ctx.fill();
          ctx.fillStyle = THEME.staticAccent; ctx.fillRect(sx+i+4,o.y+o.h-6,3,6);
        }
      } else if(THEME.patrolShape==='humanoid'){
        const bob = Math.sin(performance.now()/220)*2;
        ctx.fillStyle = THEME.patrolColor;
        ctx.beginPath();
        ctx.moveTo(sx+4,o.y+o.h); ctx.lineTo(sx,o.y+14+bob); ctx.lineTo(sx+o.w/2,o.y+bob);
        ctx.lineTo(sx+o.w,o.y+14+bob); ctx.lineTo(sx+o.w-4,o.y+o.h);
        ctx.closePath(); ctx.fill();
        ctx.fillStyle = '#e0b98a';
        ctx.beginPath(); ctx.arc(sx+o.w/2,o.y+bob-4,6,0,Math.PI*2); ctx.fill();
        ctx.strokeStyle = THEME.patrolAccent; ctx.lineWidth = 2.4;
        ctx.beginPath(); ctx.moveTo(sx+o.w+2,o.y+bob-6); ctx.lineTo(sx+o.w+2,o.y+o.h+6); ctx.stroke();
      } else {
        ctx.fillStyle = THEME.patrolColor;
        ctx.beginPath();
        ctx.ellipse(sx+o.w/2,o.y+o.h/2,o.w/2,o.h/2,0,0,Math.PI*2);
        ctx.fill();
        ctx.fillStyle = THEME.patrolAccent;
        ctx.beginPath(); ctx.arc(sx+o.w*0.7,o.y+o.h*0.4,2.4,0,Math.PI*2); ctx.fill();
      }
    }
  }

  function drawVerses(t){
    for(const v of VERSES){
      if(v.collected) continue;
      const sx = v.x-camera.x;
      if(sx<-20||sx>W+20) continue;
      const bob = Math.sin(t*2+v.id)*5;
      const glow = ctx.createRadialGradient(sx,v.y+bob,2,sx,v.y+bob,24);
      glow.addColorStop(0,'rgba(255,207,110,.65)'); glow.addColorStop(1,'rgba(255,207,110,0)');
      ctx.fillStyle = glow; ctx.fillRect(sx-24,v.y+bob-24,48,48);
      ctx.fillStyle = '#fff4d6'; ctx.fillRect(sx-8,v.y+bob-10,16,20);
      ctx.strokeStyle = '#c98a2e'; ctx.lineWidth = 1.5; ctx.strokeRect(sx-8,v.y+bob-10,16,20);
      ctx.strokeStyle = 'rgba(201,138,46,.5)';
      ctx.beginPath(); ctx.moveTo(sx-6,v.y+bob-4); ctx.lineTo(sx+6,v.y+bob-4);
      ctx.moveTo(sx-6,v.y+bob+1); ctx.lineTo(sx+6,v.y+bob+1); ctx.stroke();
    }
  }

  function drawHeartShape(cx,cy,size,color){
    ctx.save(); ctx.translate(cx,cy); ctx.scale(size,size);
    ctx.beginPath();
    ctx.moveTo(0,3);
    ctx.bezierCurveTo(0,1,-3,-2,-6,-2); ctx.bezierCurveTo(-10,-2,-10,3,-10,3);
    ctx.bezierCurveTo(-10,7,-6,10,0,15); ctx.bezierCurveTo(6,10,10,7,10,3);
    ctx.bezierCurveTo(10,3,10,-2,6,-2); ctx.bezierCurveTo(3,-2,0,1,0,3);
    ctx.closePath(); ctx.fillStyle = color; ctx.fill(); ctx.restore();
  }
  function drawHiddenHeart(t){
    if(HIDDEN_HEART.collected) return;
    const sx = HIDDEN_HEART.x-camera.x;
    if(sx<-30||sx>W+30) return;
    const bob = Math.sin(t*2.4)*6, pulse = 1+Math.sin(t*4)*0.14;
    const glow = ctx.createRadialGradient(sx,HIDDEN_HEART.y+bob,2,sx,HIDDEN_HEART.y+bob,30);
    glow.addColorStop(0,'rgba(255,110,140,.6)'); glow.addColorStop(1,'rgba(255,110,140,0)');
    ctx.fillStyle = glow; ctx.fillRect(sx-32,HIDDEN_HEART.y+bob-32,64,64);
    drawHeartShape(sx, HIDDEN_HEART.y+bob-6, 1.05*pulse, '#ff5f7d');
  }

  function drawLandmark(t){
    const sx = LANDMARK_X - camera.x;
    if(sx<-260||sx>W+260) return;
    const [c1,c2,c3] = THEME.motifColors;
    const g = ctx.createRadialGradient(sx,GROUND_Y-150,10,sx,GROUND_Y-150,250);
    g.addColorStop(0,c1.replace(')', ',.5)').replace('rgb','rgba').replace('#','#'));
    ctx.save();
    const glowGrad = ctx.createRadialGradient(sx,GROUND_Y-140,10,sx,GROUND_Y-140,240);
    glowGrad.addColorStop(0, hexA(c1,.5)); glowGrad.addColorStop(1, hexA(c1,0));
    ctx.fillStyle = glowGrad; ctx.fillRect(sx-240,GROUND_Y-380,480,460);

    const motif = THEME.motif;
    if(motif==='growth'){
      ctx.fillStyle = '#7a4b2a'; ctx.fillRect(sx-12,GROUND_Y-110,24,110);
      const rings=[c3,c2,c1];
      for(let i=0;i<3;i++){ ctx.fillStyle = rings[i]; ctx.beginPath(); ctx.ellipse(sx, GROUND_Y-140-i*44, 92-i*16, 56-i*9, 0,0,Math.PI*2); ctx.fill(); }
    } else if(motif==='fire'){
      for(let i=0;i<5;i++){
        const flick = Math.sin(t*7+i)*6; const yy = GROUND_Y-30-i*30;
        ctx.fillStyle = i%2===0?c2:c1;
        ctx.beginPath();
        ctx.moveTo(sx-22+flick*0.3, yy);
        ctx.quadraticCurveTo(sx+flick, yy-38, sx, yy-66);
        ctx.quadraticCurveTo(sx-flick, yy-38, sx+22-flick*0.3, yy);
        ctx.closePath(); ctx.fill();
      }
      ctx.fillStyle = hexA(c3,.9);
      for(let i=0;i<7;i++){ const ang=(i/7)*Math.PI*2+t*0.6; const rr=26+Math.sin(t*2+i)*8;
        ctx.beginPath(); ctx.arc(sx+Math.cos(ang)*rr, GROUND_Y-160+Math.sin(ang)*18-(t*18%36), 2,0,Math.PI*2); ctx.fill(); }
    } else if(motif==='structure'){
      ctx.fillStyle = c1; ctx.fillRect(sx-100,GROUND_Y-118,200,118);
      ctx.fillStyle = c2; for(let i=0;i<7;i++){ ctx.fillRect(sx-100+i*29,GROUND_Y-126,18,10); }
      ctx.fillRect(sx-118,GROUND_Y-156,24,156); ctx.fillRect(sx+94,GROUND_Y-156,24,156);
      ctx.fillStyle = c3; ctx.fillRect(sx-118,GROUND_Y-164,24,10); ctx.fillRect(sx+94,GROUND_Y-164,24,10);
    } else if(motif==='water'){
      ctx.fillStyle = hexA(c1,.55);
      ctx.beginPath(); ctx.ellipse(sx,GROUND_Y-30,130,26,0,0,Math.PI*2); ctx.fill();
      ctx.strokeStyle = hexA(c3,.7); ctx.lineWidth=2;
      for(let i=0;i<3;i++){ ctx.beginPath(); ctx.ellipse(sx,GROUND_Y-30,60+i*26,10+i*4+Math.sin(t*2+i)*2,0,0,Math.PI*2); ctx.stroke(); }
    } else if(motif==='mountain'){
      ctx.fillStyle = c1;
      ctx.beginPath(); ctx.moveTo(sx-140,GROUND_Y); ctx.lineTo(sx-40,GROUND_Y-190); ctx.lineTo(sx+30,GROUND_Y-100); ctx.lineTo(sx+140,GROUND_Y); ctx.closePath(); ctx.fill();
      ctx.fillStyle = c3;
      ctx.beginPath(); ctx.moveTo(sx-40,GROUND_Y-190); ctx.lineTo(sx-16,GROUND_Y-150); ctx.lineTo(sx-60,GROUND_Y-150); ctx.closePath(); ctx.fill();
    } else if(motif==='star'){
      ctx.fillStyle = hexA(c1,.9);
      const spikes=8, outer=26, inner=10;
      ctx.beginPath();
      for(let i=0;i<spikes*2;i++){ const r=i%2===0?outer:inner; const ang=(i/(spikes*2))*Math.PI*2 - Math.PI/2; const px=sx+Math.cos(ang)*r, py=(GROUND_Y-190)+Math.sin(ang)*r;
        i===0?ctx.moveTo(px,py):ctx.lineTo(px,py); }
      ctx.closePath(); ctx.fill();
      ctx.strokeStyle = hexA(c2,.7); ctx.lineWidth=1.5;
      for(let i=0;i<10;i++){ const ang=(i/10)*Math.PI*2+t*0.4; ctx.beginPath(); ctx.moveTo(sx,GROUND_Y-190); ctx.lineTo(sx+Math.cos(ang)*70,(GROUND_Y-190)+Math.sin(ang)*70); ctx.stroke(); }
    } else if(motif==='cross'){
      ctx.fillStyle = c1;
      ctx.fillRect(sx-9,GROUND_Y-200,18,200);
      ctx.fillRect(sx-52,GROUND_Y-160,104,18);
    } else if(motif==='creature' && THEME.creatureKind==='lion'){
      ctx.fillStyle = c1;
      ctx.beginPath(); ctx.ellipse(sx,GROUND_Y-56,54,30,0,0,Math.PI*2); ctx.fill();
      ctx.beginPath(); ctx.arc(sx+50,GROUND_Y-66,22,0,Math.PI*2); ctx.fill();
      ctx.fillStyle = c2;
      for(let i=0;i<10;i++){ const ang=(i/10)*Math.PI*2; ctx.beginPath(); ctx.arc(sx+50+Math.cos(ang)*24,GROUND_Y-66+Math.sin(ang)*24,7,0,Math.PI*2); ctx.fill(); }
    } else if(motif==='creature'){
      ctx.fillStyle = c1;
      ctx.beginPath();
      ctx.ellipse(sx,GROUND_Y-70,70,32,0,0,Math.PI*2); ctx.fill();
      ctx.beginPath(); ctx.moveTo(sx-70,GROUND_Y-70); ctx.lineTo(sx-100,GROUND_Y-90); ctx.lineTo(sx-100,GROUND_Y-50); ctx.closePath(); ctx.fill();
      ctx.fillStyle = c3; ctx.beginPath(); ctx.arc(sx+40,GROUND_Y-78,4,0,Math.PI*2); ctx.fill();
    }
    ctx.restore();
  }

  function hexA(hex, a){
    hex = hex.replace('#','');
    const r=parseInt(hex.substring(0,2),16), g=parseInt(hex.substring(2,4),16), b=parseInt(hex.substring(4,6),16);
    return `rgba(${r},${g},${b},${a})`;
  }

  function drawPlayer(t){
    const sx = player.x-camera.x, sy = player.y;
    const flashOff = player.invuln>0 && Math.floor(t*14)%2===0;
    if(flashOff) return;
    ctx.save();
    ctx.translate(sx+player.w/2, sy); ctx.scale(player.facing,1);
    const stride = player.onGround ? Math.sin(player.running*10)*10 : 0;
    ctx.strokeStyle = '#3c2a1a'; ctx.lineWidth = 5; ctx.lineCap = 'round';
    ctx.beginPath();
    ctx.moveTo(-4,player.h-16); ctx.lineTo(-4+stride*0.4, player.h);
    ctx.moveTo(4,player.h-16); ctx.lineTo(4-stride*0.4, player.h);
    ctx.stroke();
    ctx.fillStyle = '#fdf6e3';
    ctx.beginPath(); ctx.moveTo(-11,player.h-16); ctx.lineTo(11,player.h-16); ctx.lineTo(8,10); ctx.lineTo(-8,10); ctx.closePath(); ctx.fill();
    ctx.fillStyle = THEME.aqua; ctx.fillRect(-11,player.h-24,22,7);
    ctx.fillStyle = '#f0c397'; ctx.beginPath(); ctx.arc(0,0,9,0,Math.PI*2); ctx.fill();
    ctx.fillStyle = '#5a3c22'; ctx.beginPath(); ctx.arc(0,-4,9,Math.PI,0); ctx.fill();
    ctx.restore();
  }

  function render(t){
    const sxShake = shake>0 ? (Math.random()-0.5)*shake : 0;
    const syShake = shake>0 ? (Math.random()-0.5)*shake : 0;
    ctx.save(); ctx.translate(sxShake, syShake);
    drawSky(t); drawGround(); drawPlatforms(); drawLandmark(t); drawVerses(t); drawHiddenHeart(t);
    drawObstacles(); drawParticles(); drawPlayer(t);
    ctx.restore();
  }

  function loop(now){
    if(!lastT) lastT = now;
    let dt = (now-lastT)/1000; dt = Math.min(dt, 1/30); lastT = now;
    update(dt); render(now/1000);
    raf = requestAnimationFrame(loop);
  }
  updateHUD();
  raf = requestAnimationFrame(loop);
})();
</script>
'''

def esc(s):
    return s.replace('\\', '\\\\').replace("'", "\\'").replace('\n', ' ')

def js_str(s):
    return "'" + s.replace('\\', '\\\\').replace("'", "\\'") + "'"

def render_level(lv):
    html = TEMPLATE
    html = html.replace('@@TITLE@@', lv['title'])
    html = html.replace('@@SUBTITLE@@', lv['subtitle'])
    html = html.replace('@@ACCENT_SOFT@@', lv['accent_soft'])
    html = html.replace('@@ACCENT@@', lv['accent'])
    html = html.replace('@@DANGER@@', lv['danger'])
    html = html.replace('@@AQUA@@', lv['aqua'])
    html = html.replace('@@LEAF@@', lv['leaf'])
    html = html.replace('@@VOID2@@', lv['void2'])
    html = html.replace('@@VOID@@', lv['void'])
    html = html.replace('@@PANEL@@', lv['panel'])
    html = html.replace('@@ARCH1@@', lv['arch1'])
    html = html.replace('@@ARCH2@@', lv['arch2'])
    html = html.replace('@@CANVASBG@@', lv['canvasbg'])
    html = html.replace('@@START_TITLE@@', lv['start_title'])
    html = html.replace('@@START_BLURB@@', lv['start_blurb'])
    html = html.replace('@@DIALOGUE_REF@@', lv['dialogue_ref'])
    html = html.replace('@@DIALOGUE_TITLE@@', lv['dialogue_title'])
    html = html.replace('@@DIALOGUE_QUOTE@@', lv['dialogue_quote'])
    html = html.replace('@@DIALOGUE_REFLECTION@@', lv['reflection'])
    html = html.replace('@@COMPLETE_BLURB@@', lv['complete_blurb'])
    html = html.replace('@@GAMEOVER_TITLE@@', lv['gameover_title'])
    html = html.replace('@@GAMEOVER_BLURB@@', lv['gameover_blurb'])

    if lv['next_slug']:
        next_nav = (f'<div class="nav-links"><a href="faith-run-hub.html">← Accueil</a>'
                    f'<a href="faith-run-{lv["next_slug"]}.html">{lv["next_title"]} →</a></div>')
    else:
        next_nav = ('<div class="nav-links"><a href="faith-run-hub.html">← Accueil</a></div>'
                    '<p class="next-note">Dernier chapitre disponible — d\'autres arrivent bientôt.</p>')
    html = html.replace('@@NEXT_NAV@@', next_nav)

    # Fixed positions matching the proven, jump-tested layout (ground + platforms
    # are identical across every level). Verse 0 has no nearby platform, so it
    # sits low enough (y:300) to be reachable by a plain standing jump; the
    # others sit just above their neighboring platform.
    # verse 0 sits on clear ground well before the first obstacle (x:560) so a
    # plain standing jump collects it without threading a hazard.
    VERSE_POS = [(300,370), (1060,255), (1855,335), (2420,275), (3620,345)]
    verses_js = '[' + ','.join(
        '{x:%d,y:%d,ref:%s,text:%s}' % (VERSE_POS[i][0], VERSE_POS[i][1], js_str(ref), js_str(text))
        for i,(ref,text) in enumerate(lv['verses'])
    ) + ']'
    html = html.replace('@@VERSES_JS@@', verses_js)

    theme = dict(
        sky=list(lv['sky']), hillFar=lv['hillfar'], hillNear=lv['hillnear'],
        ground=list(lv['ground']), motif=lv['motif'], motifColors=list(lv['motif_colors']),
        patrolShape=lv['patrol_shape'], patrolColor=lv['patrol_color'], patrolAccent=lv['patrol_accent'],
        staticColor=lv['static_color'], staticAccent=lv['static_accent'],
        accent=lv['accent'], aqua=lv['aqua'], creatureKind=lv.get('creature_kind'),
    )
    def js_val(v):
        if v is None: return 'null'
        if isinstance(v, list): return '[' + ','.join(js_val(x) for x in v) + ']'
        return js_str(v)
    theme_js = '{' + ','.join(f'{k}:{js_val(v)}' for k,v in theme.items()) + '}'
    html = html.replace('@@THEME_JS@@', theme_js)

    def hex_a(hexcolor, alpha):
        hexcolor = hexcolor.lstrip('#')
        r = int(hexcolor[0:2], 16); g = int(hexcolor[2:4], 16); b = int(hexcolor[4:6], 16)
        return f'rgba({r},{g},{b},{alpha})'

    def replace_colormix(m):
        hexcolor, pct = m.group(1), int(m.group(2))
        return hex_a(hexcolor, round(pct/100, 3))
    html = re.sub(r'color-mix\(in srgb,\s*(#[0-9a-fA-F]{6})\s+(\d+)%,\s*transparent\)', replace_colormix, html)
    return html

if __name__ == '__main__':
    os.makedirs(OUT_DIR, exist_ok=True)
    for lv in LEVELS:
        out = render_level(lv)
        path = os.path.join(OUT_DIR, f'faith-run-{lv["slug"]}.html')
        with open(path, 'w', encoding='utf-8') as f:
            f.write(out)
        print('wrote', path, len(out), 'bytes')
