/* Données des versets bibliques (Louis Segond 1910, domaine public),
   organisées en "parcours" narratifs de 12 niveaux chacun. */

/* Symboles à thème biblique utilisés comme "bonbons" du jeu */
const TILE_SYMBOLS = ["✝️", "🕊️", "🐟", "⭐", "👑", "📖", "🍇"];

/* Difficulté progressive appliquée aux 12 niveaux de chaque parcours */
const LEVEL_TARGET = [400, 435, 470, 505, 540, 575, 610, 645, 680, 715, 750, 785];
const LEVEL_MOVES = [20, 20, 19, 19, 18, 18, 17, 17, 16, 16, 15, 15];
const levelTiles = (n) => (n <= 6 ? 6 : 5);

function buildLevels(rawVerses) {
  return rawVerses.map((v, i) => {
    const n = i + 1;
    return {
      id: n,
      ref: v.ref,
      text: v.text,
      hide: v.hide,
      target: LEVEL_TARGET[i],
      moves: LEVEL_MOVES[i],
      tiles: levelTiles(n)
    };
  });
}

const PARCOURS_RAW = [
  {
    id: "creation",
    title: "La Création",
    emoji: "🌱",
    subtitle: "Les 7 jours où Dieu a tout créé",
    verses: [
      { ref: "Genèse 1.1", text: "Au commencement Dieu créa les cieux et la terre", hide: [1, 3, 5, 8] },
      { ref: "Genèse 1.3", text: "Dieu dit Que la lumière soit Et la lumière fut", hide: [4, 5, 8, 9] },
      { ref: "Genèse 1.5", text: "Dieu appela la lumière jour et les ténèbres nuit", hide: [1, 3, 7, 8] },
      { ref: "Genèse 1.16", text: "Dieu fit les deux grands luminaires et aussi les étoiles", hide: [1, 5, 9] },
      { ref: "Genèse 1.21", text: "Dieu créa les grands poissons et tous les oiseaux ailés", hide: [1, 4, 8, 9] },
      { ref: "Genèse 1.25", text: "Dieu fit les animaux de la terre selon leur espèce", hide: [1, 3, 6, 9] },
      { ref: "Genèse 1.27", text: "Dieu créa l'homme à son image il créa l'homme et femme", hide: [1, 5, 7, 10] },
      { ref: "Genèse 1.31", text: "Dieu vit tout ce qu'il avait fait et cela était très bon", hide: [1, 6, 10, 11] },
      { ref: "Genèse 2.2", text: "Dieu acheva au septième jour son oeuvre et il se reposa", hide: [1, 3, 6, 10] },
      { ref: "Genèse 2.3", text: "Dieu bénit le septième jour et le sanctifia", hide: [1, 3, 7] },
      { ref: "Genèse 2.7", text: "L'Éternel Dieu forma l'homme de la poussière de la terre", hide: [2, 3, 6, 9] },
      { ref: "Genèse 2.18", text: "Il n'est pas bon que l'homme soit seul je lui ferai une aide", hide: [3, 5, 7, 12] }
    ]
  },
  {
    id: "abraham",
    title: "La vie d'Abraham",
    emoji: "⛺",
    subtitle: "L'ami de Dieu, père des croyants",
    verses: [
      { ref: "Genèse 12.1", text: "L'Éternel dit à Abram Va-t'en de ton pays vers le pays que je te montrerai", hide: [3, 4, 7, 14] },
      { ref: "Genèse 12.2", text: "Je ferai de toi une grande nation et je te bénirai", hide: [1, 5, 6, 10] },
      { ref: "Genèse 15.5", text: "Regarde vers le ciel et compte les étoiles telle sera ta postérité", hide: [0, 3, 7, 11] },
      { ref: "Genèse 15.6", text: "Abram eut confiance en l'Éternel qui le lui imputa à justice", hide: [2, 4, 8, 10] },
      { ref: "Genèse 17.5", text: "On ne t'appellera plus Abram mais ton nom sera Abraham", hide: [2, 4, 7, 9] },
      { ref: "Genèse 18.14", text: "Y a-t-il rien qui soit étonnant de la part de l'Éternel", hide: [2, 5, 8, 10] },
      { ref: "Genèse 21.1", text: "L'Éternel se souvint de ce qu'il avait dit à Sara", hide: [2, 6, 7, 9] },
      { ref: "Genèse 22.8", text: "Dieu se pourvoira lui-même de l'agneau pour l'holocauste mon fils", hide: [2, 5, 7, 9] },
      { ref: "Genèse 22.14", text: "Abraham donna à ce lieu le nom de l'Éternel pourvoira", hide: [1, 4, 6, 9] },
      { ref: "Genèse 22.17", text: "Je multiplierai ta postérité comme les étoiles du ciel", hide: [1, 3, 6, 8] },
      { ref: "Genèse 24.27", text: "Béni soit l'Éternel qui n'a pas renoncé à sa fidélité", hide: [0, 2, 6, 9] },
      { ref: "Genèse 25.8", text: "Abraham expira et mourut âgé et rassasié de jours", hide: [1, 3, 6, 8] }
    ]
  },
  {
    id: "jacob",
    title: "La vie de Jacob",
    emoji: "🪜",
    subtitle: "L'échelle qui touche le ciel",
    verses: [
      { ref: "Genèse 28.12", text: "Jacob eut un songe une échelle touchait le ciel et des anges y montaient", hide: [3, 5, 8, 11] },
      { ref: "Genèse 28.15", text: "Voici je suis avec toi je te garderai partout où tu iras", hide: [7, 8, 11] },
      { ref: "Genèse 28.16", text: "Certainement l'Éternel est en ce lieu et je ne le savais pas", hide: [0, 1, 5, 10] },
      { ref: "Genèse 29.20", text: "Jacob servit sept années pour Rachel elles furent à ses yeux comme quelques jours", hide: [1, 2, 5, 13] },
      { ref: "Genèse 31.3", text: "Retourne au pays de tes pères et je serai avec toi", hide: [0, 2, 5, 8] },
      { ref: "Genèse 32.26", text: "Je ne te laisserai point aller que tu ne m'aies béni", hide: [3, 5, 10] },
      { ref: "Genèse 32.28", text: "On ne t'appellera plus Jacob mais Israël car tu as été vainqueur", hide: [2, 4, 6, 11] },
      { ref: "Genèse 33.4", text: "Ésaü courut à sa rencontre l'embrassa et ils pleurèrent", hide: [0, 4, 5, 8] },
      { ref: "Genèse 35.3", text: "Levons-nous montons à Béthel je veux faire là un autel à Dieu", hide: [1, 3, 9, 11] },
      { ref: "Genèse 25.23", text: "L'aîné sera assujetti au plus jeune", hide: [0, 2, 5] },
      { ref: "Genèse 30.22", text: "Dieu se souvint de Rachel il l'exauça et la rendit féconde", hide: [2, 4, 6, 10] },
      { ref: "Genèse 46.3", text: "Ne crains point de descendre en Égypte car là je te ferai devenir une grande nation", hide: [1, 4, 6, 14, 15] }
    ]
  },
  {
    id: "joseph",
    title: "La vie de Joseph",
    emoji: "🌾",
    subtitle: "Du puits d'Égypte au palais de Pharaon",
    verses: [
      { ref: "Genèse 37.3", text: "Israël aimait Joseph plus que tous ses fils il lui fit une tunique de plusieurs couleurs", hide: [1, 2, 12, 15] },
      { ref: "Genèse 37.5", text: "Joseph eut un songe et il le raconta à ses frères", hide: [3, 7, 10] },
      { ref: "Genèse 39.2", text: "L'Éternel fut avec Joseph et il prospéra", hide: [1, 3, 6] },
      { ref: "Genèse 39.9", text: "Comment ferais-je un aussi grand mal et pécherais-je contre Dieu", hide: [1, 5, 7, 9] },
      { ref: "Genèse 39.21", text: "L'Éternel fut avec Joseph et il étendit sur lui sa bonté", hide: [1, 6, 10] },
      { ref: "Genèse 41.16", text: "Ce n'est pas moi c'est Dieu qui donnera une réponse favorable à Pharaon", hide: [5, 7, 9, 12] },
      { ref: "Genèse 41.41", text: "Pharaon dit à Joseph je t'établis sur tout le pays d'Égypte", hide: [0, 5, 9, 10] },
      { ref: "Genèse 45.4", text: "Je suis Joseph votre frère que vous avez vendu", hide: [2, 4, 8] },
      { ref: "Genèse 45.5", text: "C'est pour vous sauver la vie que Dieu m'a envoyé devant vous", hide: [3, 5, 7, 9] },
      { ref: "Genèse 45.8", text: "Ce n'est donc pas vous qui m'avez envoyé ici mais c'est Dieu", hide: [6, 7, 8, 11] },
      { ref: "Genèse 50.20", text: "Vous aviez médité de me faire du mal Dieu l'a changé en bien", hide: [2, 7, 10, 12] },
      { ref: "Genèse 50.21", text: "Ne craignez point je vous entretiendrai vous et vos enfants", hide: [1, 5, 9] }
    ]
  },
  {
    id: "exode",
    title: "L'Exode",
    emoji: "🌊",
    subtitle: "La sortie d'Égypte vers la liberté",
    verses: [
      { ref: "Exode 2.24", text: "Dieu entendit leurs gémissements et se souvint de son alliance avec Abraham", hide: [1, 3, 9, 11] },
      { ref: "Exode 3.4", text: "Dieu l'appela du milieu du buisson Moïse Moïse Et il répondit me voici", hide: [1, 5, 6, 10] },
      { ref: "Exode 3.14", text: "Dieu dit à Moïse je suis celui qui suis", hide: [3, 5, 6] },
      { ref: "Exode 4.12", text: "Va je serai avec ta bouche et je t'enseignerai ce que tu diras", hide: [2, 5, 8, 12] },
      { ref: "Exode 5.1", text: "Laisse aller mon peuple afin qu'il me serve", hide: [0, 3, 7] },
      { ref: "Exode 12.13", text: "Le sang vous servira de signe et il n'y aura point de plaie qui vous détruise", hide: [1, 5, 12, 15] },
      { ref: "Exode 14.13", text: "Ne craignez rien restez en place et regardez la délivrance de l'Éternel", hide: [1, 7, 9, 11] },
      { ref: "Exode 14.22", text: "Les eaux se fendirent et les enfants d'Israël entrèrent au milieu de la mer à sec", hide: [1, 3, 13, 15] },
      { ref: "Exode 15.2", text: "L'Éternel est ma force et le sujet de mes louanges c'est lui qui m'a sauvé", hide: [3, 9, 14] },
      { ref: "Exode 16.4", text: "Voici je vais faire pleuvoir pour vous du pain du haut des cieux", hide: [4, 8, 12] },
      { ref: "Exode 20.2", text: "Je suis l'Éternel ton Dieu tu n'auras pas d'autres dieux devant ma face", hide: [2, 4, 6, 9] },
      { ref: "Exode 20.12", text: "Honore ton père et ta mère afin que tes jours se prolongent", hide: [0, 2, 5, 11] }
    ]
  },
  {
    id: "david",
    title: "La vie de David",
    emoji: "🪨",
    subtitle: "Le petit berger devenu roi",
    verses: [
      { ref: "1 Samuel 16.7", text: "L'Éternel ne regarde pas à ce que l'homme regarde l'Éternel regarde au coeur", hide: [2, 7, 9, 12] },
      { ref: "1 Samuel 16.13", text: "Samuel prit la corne d'huile et l'esprit de l'Éternel saisit David", hide: [0, 3, 6, 9] },
      { ref: "1 Samuel 17.37", text: "L'Éternel qui m'a délivré de la griffe du lion me délivrera de ce Philistin", hide: [3, 6, 8, 13] },
      { ref: "1 Samuel 17.45", text: "Tu marches contre moi avec l'épée et moi je marche au nom de l'Éternel", hide: [1, 5, 9, 13] },
      { ref: "1 Samuel 18.1", text: "L'âme de Jonathan s'attacha à l'âme de David et Jonathan l'aima comme son âme", hide: [2, 3, 7, 10] },
      { ref: "1 Samuel 24.6", text: "Que l'Éternel me garde de commettre une telle action contre mon seigneur", hide: [3, 5, 8, 11] },
      { ref: "2 Samuel 7.16", text: "Ta maison et ton règne seront pour toujours affermis", hide: [1, 4, 8] },
      { ref: "Psaume 23.1", text: "L'Éternel est mon berger je ne manquerai de rien", hide: [3, 6, 8] },
      { ref: "Psaume 27.1", text: "L'Éternel est ma lumière et mon salut de qui aurais-je crainte", hide: [3, 6, 10] },
      { ref: "Psaume 34.9", text: "Sentez et voyez combien l'Éternel est bon heureux l'homme qui cherche en lui son refuge", hide: [0, 2, 6, 14] },
      { ref: "Psaume 103.1", text: "Mon âme bénis l'Éternel que tout ce qui est en moi bénisse son saint nom", hide: [1, 2, 11, 14] },
      { ref: "1 Chroniques 29.11", text: "A toi Éternel la grandeur la force et la magnificence et la gloire", hide: [4, 6, 9, 12] }
    ]
  },
  {
    id: "jesus",
    title: "La naissance de Jésus",
    emoji: "🌟",
    subtitle: "Noël : Dieu vient habiter parmi nous",
    verses: [
      { ref: "Luc 1.31", text: "N'aie pas de crainte Marie tu enfanteras un fils et tu lui donneras le nom de Jésus", hide: [3, 4, 6, 16] },
      { ref: "Luc 1.37", text: "Car rien n'est impossible à Dieu", hide: [1, 3, 5] },
      { ref: "Luc 1.38", text: "Je suis la servante du Seigneur qu'il me soit fait selon ta parole", hide: [3, 5, 9, 12] },
      { ref: "Matthieu 1.21", text: "Elle enfantera un fils c'est lui qui sauvera son peuple de ses péchés", hide: [1, 7, 9, 12] },
      { ref: "Matthieu 1.23", text: "On lui donnera le nom d'Emmanuel ce qui signifie Dieu avec nous", hide: [2, 5, 8, 9] },
      { ref: "Luc 2.4", text: "Joseph monta à Bethléhem pour se faire inscrire avec Marie sa fiancée", hide: [0, 3, 9, 11] },
      { ref: "Luc 2.7", text: "Elle enfanta son fils premier-né et le coucha dans une crèche", hide: [1, 4, 7, 10] },
      { ref: "Luc 2.11", text: "Ne craignez point il vous est né un Sauveur qui est le Christ le Seigneur", hide: [6, 8, 12, 14] },
      { ref: "Luc 2.14", text: "Gloire à Dieu dans les lieux très hauts et paix sur la terre", hide: [0, 2, 9, 12] },
      { ref: "Luc 2.20", text: "Les bergers s'en retournèrent glorifiant et louant Dieu pour tout ce qu'ils avaient vu", hide: [1, 4, 6, 13] },
      { ref: "Matthieu 2.2", text: "Nous avons vu son étoile en Orient et nous sommes venus pour l'adorer", hide: [2, 4, 6, 12] },
      { ref: "Matthieu 2.11", text: "Ils virent le petit enfant avec Marie sa mère et se prosternant ils l'adorèrent", hide: [1, 4, 6, 13] }
    ]
  },
  {
    id: "paul",
    title: "La vie de Paul",
    emoji: "📜",
    subtitle: "De persécuteur à apôtre de Jésus",
    verses: [
      { ref: "Actes 9.3", text: "Tout à coup une lumière venant du ciel resplendit autour de Saul", hide: [4, 7, 8, 11] },
      { ref: "Actes 9.4", text: "Saul Saul pourquoi me persécutes-tu", hide: [0, 2, 4] },
      { ref: "Actes 9.15", text: "Va car cet homme est un instrument que j'ai choisi pour porter mon nom", hide: [6, 9, 11, 13] },
      { ref: "Actes 9.18", text: "Il recouvra la vue il se leva et fut baptisé", hide: [1, 3, 6, 9] },
      { ref: "Actes 16.9", text: "Passe en Macédoine et viens à notre secours", hide: [0, 2, 7] },
      { ref: "Actes 16.25", text: "Vers le milieu de la nuit Paul et Silas priaient et chantaient les louanges de Dieu", hide: [5, 6, 8, 9] },
      { ref: "Actes 17.11", text: "Ils recevaient la parole avec empressement et examinaient chaque jour les Écritures", hide: [1, 3, 7, 11] },
      { ref: "Romains 1.16", text: "Je n'ai point honte de l'Évangile c'est une puissance de Dieu pour le salut", hide: [3, 5, 8, 13] },
      { ref: "Romains 8.28", text: "Toutes choses concourent au bien de ceux qui aiment Dieu", hide: [2, 4, 8, 9] },
      { ref: "Philippiens 4.13", text: "Je puis tout par celui qui me fortifie", hide: [1, 4, 7] },
      { ref: "Galates 5.22", text: "Le fruit de l'Esprit c'est l'amour la joie la paix la patience la bonté", hide: [1, 5, 7, 11, 13] },
      { ref: "2 Timothée 4.7", text: "J'ai combattu le bon combat j'ai achevé la course j'ai gardé la foi", hide: [1, 4, 6, 8, 12] }
    ]
  }
];

const PARCOURS = PARCOURS_RAW.map((p) => ({
  id: p.id,
  title: p.title,
  emoji: p.emoji,
  subtitle: p.subtitle,
  levels: buildLevels(p.verses)
}));
