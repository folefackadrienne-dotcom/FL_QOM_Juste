/* Données des versets bibliques, bilingues (français : Louis Segond 1910 ;
   anglais : King James Version — deux traductions du domaine public),
   organisées en "parcours" narratifs de 12 niveaux chacun. */

/* Symboles à thème biblique utilisés comme "bonbons" du jeu */
const TILE_SYMBOLS = ["✝️", "🕊️", "🐟", "⭐", "👑", "📖", "🍇"];

/* Difficulté progressive appliquée aux 12 niveaux de chaque parcours */
const LEVEL_TARGET = [400, 435, 470, 505, 540, 575, 610, 645, 680, 715, 750, 785];
const LEVEL_MOVES = [20, 20, 19, 19, 18, 18, 17, 17, 16, 16, 15, 15];
const levelTiles = (n) => (n <= 6 ? 6 : 5);

function buildLevels(rawVerses, lang) {
  return rawVerses.map((v, i) => {
    const n = i + 1;
    const l = v[lang];
    return {
      id: n,
      ref: l.ref,
      text: l.text,
      hide: l.hide,
      target: LEVEL_TARGET[i],
      moves: LEVEL_MOVES[i],
      tiles: levelTiles(n)
    };
  });
}

const PARCOURS = [
  {
    id: "creation",
    emoji: "🌱",
    fr: { title: "La Création", subtitle: "Les 7 jours où Dieu a tout créé" },
    en: { title: "Creation", subtitle: "The 7 days God made everything" },
    verses: [
      {
        fr: { ref: "Genèse 1.1", text: "Au commencement Dieu créa les cieux et la terre", hide: [1, 3, 5, 8] },
        en: { ref: "Genesis 1:1", text: "In the beginning God created the heaven and the earth", hide: [2, 4, 6, 9] }
      },
      {
        fr: { ref: "Genèse 1.3", text: "Dieu dit Que la lumière soit Et la lumière fut", hide: [4, 5, 8, 9] },
        en: { ref: "Genesis 1:3", text: "God said Let there be light and there was light", hide: [1, 5, 8, 9] }
      },
      {
        fr: { ref: "Genèse 1.5", text: "Dieu appela la lumière jour et les ténèbres nuit", hide: [1, 3, 7, 8] },
        en: { ref: "Genesis 1:5", text: "God called the light Day and the darkness Night", hide: [1, 3, 7, 8] }
      },
      {
        fr: { ref: "Genèse 1.16", text: "Dieu fit les deux grands luminaires et aussi les étoiles", hide: [1, 5, 9] },
        en: { ref: "Genesis 1:16", text: "God made the two great lights and the stars also", hide: [1, 5, 8] }
      },
      {
        fr: { ref: "Genèse 1.21", text: "Dieu créa les grands poissons et tous les oiseaux ailés", hide: [1, 4, 8, 9] },
        en: { ref: "Genesis 1:21", text: "God created the great whales and every winged fowl", hide: [1, 4, 7, 8] }
      },
      {
        fr: { ref: "Genèse 1.25", text: "Dieu fit les animaux de la terre selon leur espèce", hide: [1, 3, 6, 9] },
        en: { ref: "Genesis 1:25", text: "God made the beasts of the earth after their kind", hide: [1, 3, 6, 9] }
      },
      {
        fr: { ref: "Genèse 1.27", text: "Dieu créa l'homme à son image il créa l'homme et femme", hide: [1, 5, 7, 10] },
        en: { ref: "Genesis 1:27", text: "God created man in his own image male and female", hide: [1, 6, 7, 9] }
      },
      {
        fr: { ref: "Genèse 1.31", text: "Dieu vit tout ce qu'il avait fait et cela était très bon", hide: [1, 6, 10, 11] },
        en: { ref: "Genesis 1:31", text: "God saw everything that he had made and it was very good", hide: [1, 6, 10, 11] }
      },
      {
        fr: { ref: "Genèse 2.2", text: "Dieu acheva au septième jour son oeuvre et il se reposa", hide: [1, 3, 6, 10] },
        en: { ref: "Genesis 2:2", text: "On the seventh day God ended his work and he rested", hide: [2, 5, 7, 10] }
      },
      {
        fr: { ref: "Genèse 2.3", text: "Dieu bénit le septième jour et le sanctifia", hide: [1, 3, 7] },
        en: { ref: "Genesis 2:3", text: "God blessed the seventh day and sanctified it", hide: [1, 3, 6] }
      },
      {
        fr: { ref: "Genèse 2.7", text: "L'Éternel Dieu forma l'homme de la poussière de la terre", hide: [2, 3, 6, 9] },
        en: { ref: "Genesis 2:7", text: "The LORD God formed man of the dust of the ground", hide: [3, 4, 7, 10] }
      },
      {
        fr: { ref: "Genèse 2.18", text: "Il n'est pas bon que l'homme soit seul je lui ferai une aide", hide: [3, 5, 7, 12] },
        en: { ref: "Genesis 2:18", text: "It is not good that the man should be alone I will make him an help", hide: [3, 6, 9, 15] }
      }
    ]
  },
  {
    id: "abraham",
    emoji: "⛺",
    fr: { title: "La vie d'Abraham", subtitle: "L'ami de Dieu, père des croyants" },
    en: { title: "The life of Abraham", subtitle: "God's friend, father of believers" },
    verses: [
      {
        fr: { ref: "Genèse 12.1", text: "L'Éternel dit à Abram Va-t'en de ton pays vers le pays que je te montrerai", hide: [3, 4, 7, 14] },
        en: { ref: "Genesis 12:1", text: "The LORD said unto Abram Get thee unto a land that I will shew thee", hide: [4, 5, 9, 13] }
      },
      {
        fr: { ref: "Genèse 12.2", text: "Je ferai de toi une grande nation et je te bénirai", hide: [1, 5, 6, 10] },
        en: { ref: "Genesis 12:2", text: "I will make of thee a great nation and I will bless thee", hide: [2, 6, 7, 11] }
      },
      {
        fr: { ref: "Genèse 15.5", text: "Regarde vers le ciel et compte les étoiles telle sera ta postérité", hide: [0, 3, 7, 11] },
        en: { ref: "Genesis 15:5", text: "Look now toward heaven and count the stars so shall thy seed be", hide: [0, 3, 7, 11] }
      },
      {
        fr: { ref: "Genèse 15.6", text: "Abram eut confiance en l'Éternel qui le lui imputa à justice", hide: [2, 4, 8, 10] },
        en: { ref: "Genesis 15:6", text: "Abram believed in the LORD and he counted it to him for righteousness", hide: [1, 7, 12] }
      },
      {
        fr: { ref: "Genèse 17.5", text: "On ne t'appellera plus Abram mais ton nom sera Abraham", hide: [2, 4, 7, 9] },
        en: { ref: "Genesis 17:5", text: "Thy name shall no more be called Abram but thy name shall be Abraham", hide: [6, 7, 10, 13] }
      },
      {
        fr: { ref: "Genèse 18.14", text: "Y a-t-il rien qui soit étonnant de la part de l'Éternel", hide: [2, 5, 8, 10] },
        en: { ref: "Genesis 18:14", text: "Is any thing too hard for the LORD", hide: [1, 4, 7] }
      },
      {
        fr: { ref: "Genèse 21.1", text: "L'Éternel se souvint de ce qu'il avait dit à Sara", hide: [2, 6, 7, 9] },
        en: { ref: "Genesis 21:1", text: "The LORD visited Sarah as he had said", hide: [2, 3, 7] }
      },
      {
        fr: { ref: "Genèse 22.8", text: "Dieu se pourvoira lui-même de l'agneau pour l'holocauste mon fils", hide: [2, 5, 7, 9] },
        en: { ref: "Genesis 22:8", text: "My son God will provide himself a lamb for a burnt offering", hide: [4, 7, 10, 11] }
      },
      {
        fr: { ref: "Genèse 22.14", text: "Abraham donna à ce lieu le nom de l'Éternel pourvoira", hide: [1, 4, 6, 9] },
        en: { ref: "Genesis 22:14", text: "Abraham called that place The LORD will provide", hide: [1, 3, 7] }
      },
      {
        fr: { ref: "Genèse 22.17", text: "Je multiplierai ta postérité comme les étoiles du ciel", hide: [1, 3, 6, 8] },
        en: { ref: "Genesis 22:17", text: "I will multiply thy seed as the stars of heaven", hide: [2, 4, 7, 9] }
      },
      {
        fr: { ref: "Genèse 24.27", text: "Béni soit l'Éternel qui n'a pas renoncé à sa fidélité", hide: [0, 2, 6, 9] },
        en: { ref: "Genesis 24:27", text: "Blessed be the LORD who has not forsaken his mercy and his truth", hide: [0, 7, 9, 12] }
      },
      {
        fr: { ref: "Genèse 25.8", text: "Abraham expira et mourut âgé et rassasié de jours", hide: [1, 3, 6, 8] },
        en: { ref: "Genesis 25:8", text: "Abraham died in a good old age an old man and full of years", hide: [1, 6, 11, 13] }
      }
    ]
  },
  {
    id: "jacob",
    emoji: "🪜",
    fr: { title: "La vie de Jacob", subtitle: "L'échelle qui touche le ciel" },
    en: { title: "The life of Jacob", subtitle: "The ladder that touches heaven" },
    verses: [
      {
        fr: { ref: "Genèse 28.12", text: "Jacob eut un songe une échelle touchait le ciel et des anges y montaient", hide: [3, 5, 8, 11] },
        en: { ref: "Genesis 28:12", text: "Jacob dreamed of a ladder that reached to heaven and angels were going up it", hide: [1, 4, 8, 10] }
      },
      {
        fr: { ref: "Genèse 28.15", text: "Voici je suis avec toi je te garderai partout où tu iras", hide: [7, 8, 11] },
        en: { ref: "Genesis 28:15", text: "Behold I am with thee and will keep thee in all places whither thou goest", hide: [7, 11, 14] }
      },
      {
        fr: { ref: "Genèse 28.16", text: "Certainement l'Éternel est en ce lieu et je ne le savais pas", hide: [0, 1, 5, 10] },
        en: { ref: "Genesis 28:16", text: "Surely the LORD is in this place and I knew it not", hide: [0, 2, 6, 9] }
      },
      {
        fr: { ref: "Genèse 29.20", text: "Jacob servit sept années pour Rachel elles furent à ses yeux comme quelques jours", hide: [1, 2, 5, 13] },
        en: { ref: "Genesis 29:20", text: "Jacob served seven years for Rachel and they seemed to him but a few days", hide: [1, 2, 5, 14] }
      },
      {
        fr: { ref: "Genèse 31.3", text: "Retourne au pays de tes pères et je serai avec toi", hide: [0, 2, 5, 8] },
        en: { ref: "Genesis 31:3", text: "Return unto the land of thy fathers and I will be with thee", hide: [0, 3, 6, 10] }
      },
      {
        fr: { ref: "Genèse 32.26", text: "Je ne te laisserai point aller que tu ne m'aies béni", hide: [3, 5, 10] },
        en: { ref: "Genesis 32:26", text: "I will not let thee go except thou bless me", hide: [3, 5, 8] }
      },
      {
        fr: { ref: "Genèse 32.28", text: "On ne t'appellera plus Jacob mais Israël car tu as été vainqueur", hide: [2, 4, 6, 11] },
        en: { ref: "Genesis 32:28", text: "Thy name shall be called no more Jacob but Israel for thou hast prevailed", hide: [4, 7, 9, 13] }
      },
      {
        fr: { ref: "Genèse 33.4", text: "Ésaü courut à sa rencontre l'embrassa et ils pleurèrent", hide: [0, 4, 5, 8] },
        en: { ref: "Genesis 33:4", text: "Esau ran to meet him and embraced him and they wept", hide: [0, 3, 6, 10] }
      },
      {
        fr: { ref: "Genèse 35.3", text: "Levons-nous montons à Béthel je veux faire là un autel à Dieu", hide: [1, 3, 9, 11] },
        en: { ref: "Genesis 35:3", text: "Let us arise and go up to Bethel and I will make there an altar unto God", hide: [2, 7, 14, 16] }
      },
      {
        fr: { ref: "Genèse 25.23", text: "L'aîné sera assujetti au plus jeune", hide: [0, 2, 5] },
        en: { ref: "Genesis 25:23", text: "The elder shall serve the younger", hide: [1, 3, 5] }
      },
      {
        fr: { ref: "Genèse 30.22", text: "Dieu se souvint de Rachel il l'exauça et la rendit féconde", hide: [2, 4, 6, 10] },
        en: { ref: "Genesis 30:22", text: "God remembered Rachel and he heard her and opened her womb", hide: [1, 2, 8, 10] }
      },
      {
        fr: { ref: "Genèse 46.3", text: "Ne crains point de descendre en Égypte car là je te ferai devenir une grande nation", hide: [1, 4, 6, 14, 15] },
        en: { ref: "Genesis 46:3", text: "Fear not to go down into Egypt for I will there make of thee a great nation", hide: [0, 6, 15, 16] }
      }
    ]
  },
  {
    id: "joseph",
    emoji: "🌾",
    fr: { title: "La vie de Joseph", subtitle: "Du puits d'Égypte au palais de Pharaon" },
    en: { title: "The life of Joseph", subtitle: "From the pit to Pharaoh's palace" },
    verses: [
      {
        fr: { ref: "Genèse 37.3", text: "Israël aimait Joseph plus que tous ses fils il lui fit une tunique de plusieurs couleurs", hide: [1, 2, 12, 15] },
        en: { ref: "Genesis 37:3", text: "Israel loved Joseph more than all his children and made him a coat of many colours", hide: [1, 2, 12, 15] }
      },
      {
        fr: { ref: "Genèse 37.5", text: "Joseph eut un songe et il le raconta à ses frères", hide: [3, 7, 10] },
        en: { ref: "Genesis 37:5", text: "Joseph dreamed a dream and he told it to his brethren", hide: [1, 3, 6, 10] }
      },
      {
        fr: { ref: "Genèse 39.2", text: "L'Éternel fut avec Joseph et il prospéra", hide: [1, 3, 6] },
        en: { ref: "Genesis 39:2", text: "The LORD was with Joseph and he was a prosperous man", hide: [1, 4, 9] }
      },
      {
        fr: { ref: "Genèse 39.9", text: "Comment ferais-je un aussi grand mal et pécherais-je contre Dieu", hide: [1, 5, 7, 9] },
        en: { ref: "Genesis 39:9", text: "How can I do this great wickedness and sin against God", hide: [6, 8, 10] }
      },
      {
        fr: { ref: "Genèse 39.21", text: "L'Éternel fut avec Joseph et il étendit sur lui sa bonté", hide: [1, 6, 10] },
        en: { ref: "Genesis 39:21", text: "The LORD was with Joseph and shewed him mercy", hide: [1, 6, 8] }
      },
      {
        fr: { ref: "Genèse 41.16", text: "Ce n'est pas moi c'est Dieu qui donnera une réponse favorable à Pharaon", hide: [5, 7, 9, 12] },
        en: { ref: "Genesis 41:16", text: "It is not in me God shall give Pharaoh an answer of peace", hide: [5, 7, 8, 12] }
      },
      {
        fr: { ref: "Genèse 41.41", text: "Pharaon dit à Joseph je t'établis sur tout le pays d'Égypte", hide: [0, 5, 9, 10] },
        en: { ref: "Genesis 41:41", text: "Pharaoh said unto Joseph I have set thee over all the land of Egypt", hide: [0, 6, 11, 13] }
      },
      {
        fr: { ref: "Genèse 45.4", text: "Je suis Joseph votre frère que vous avez vendu", hide: [2, 4, 8] },
        en: { ref: "Genesis 45:4", text: "I am Joseph your brother whom ye sold", hide: [2, 4, 7] }
      },
      {
        fr: { ref: "Genèse 45.5", text: "C'est pour vous sauver la vie que Dieu m'a envoyé devant vous", hide: [3, 5, 7, 9] },
        en: { ref: "Genesis 45:5", text: "God did send me before you to preserve life", hide: [2, 7, 8] }
      },
      {
        fr: { ref: "Genèse 45.8", text: "Ce n'est donc pas vous qui m'avez envoyé ici mais c'est Dieu", hide: [6, 7, 8, 11] },
        en: { ref: "Genesis 45:8", text: "So now it was not you that sent me hither but God", hide: [7, 9, 11] }
      },
      {
        fr: { ref: "Genèse 50.20", text: "Vous aviez médité de me faire du mal Dieu l'a changé en bien", hide: [2, 7, 10, 12] },
        en: { ref: "Genesis 50:20", text: "Ye thought evil against me but God meant it unto good", hide: [1, 2, 7, 10] }
      },
      {
        fr: { ref: "Genèse 50.21", text: "Ne craignez point je vous entretiendrai vous et vos enfants", hide: [1, 5, 9] },
        en: { ref: "Genesis 50:21", text: "Fear ye not I will nourish you and your little ones", hide: [0, 5, 9, 10] }
      }
    ]
  },
  {
    id: "exode",
    emoji: "🌊",
    fr: { title: "L'Exode", subtitle: "La sortie d'Égypte vers la liberté" },
    en: { title: "The Exodus", subtitle: "Leaving Egypt for freedom" },
    verses: [
      {
        fr: { ref: "Exode 2.24", text: "Dieu entendit leurs gémissements et se souvint de son alliance avec Abraham", hide: [1, 3, 9, 11] },
        en: { ref: "Exodus 2:24", text: "God heard their groaning and remembered his covenant with Abraham", hide: [1, 3, 7, 9] }
      },
      {
        fr: { ref: "Exode 3.4", text: "Dieu l'appela du milieu du buisson Moïse Moïse Et il répondit me voici", hide: [1, 5, 6, 10] },
        en: { ref: "Exodus 3:4", text: "God called unto him out of the bush Moses Moses And he said Here am I", hide: [1, 7, 8, 12] }
      },
      {
        fr: { ref: "Exode 3.14", text: "Dieu dit à Moïse je suis celui qui suis", hide: [3, 5, 6] },
        en: { ref: "Exodus 3:14", text: "God said unto Moses I AM THAT I AM", hide: [3, 5, 6] }
      },
      {
        fr: { ref: "Exode 4.12", text: "Va je serai avec ta bouche et je t'enseignerai ce que tu diras", hide: [2, 5, 8, 12] },
        en: { ref: "Exodus 4:12", text: "Go and I will be with thy mouth and teach thee what thou shalt say", hide: [7, 9, 14] }
      },
      {
        fr: { ref: "Exode 8.1", text: "Laisse aller mon peuple afin qu'il me serve", hide: [0, 3, 7] },
        en: { ref: "Exodus 8:1", text: "Let my people go that they may serve me", hide: [0, 2, 7] }
      },
      {
        fr: { ref: "Exode 12.13", text: "Le sang vous servira de signe et il n'y aura point de plaie qui vous détruise", hide: [1, 5, 12, 15] },
        en: { ref: "Exodus 12:13", text: "The blood shall be to you for a token and the plague shall not destroy you", hide: [1, 8, 11, 14] }
      },
      {
        fr: { ref: "Exode 14.13", text: "Ne craignez rien restez en place et regardez la délivrance de l'Éternel", hide: [1, 7, 9, 11] },
        en: { ref: "Exodus 14:13", text: "Fear ye not stand still and see the salvation of the LORD", hide: [3, 6, 8, 11] }
      },
      {
        fr: { ref: "Exode 14.22", text: "Les eaux se fendirent et les enfants d'Israël entrèrent au milieu de la mer à sec", hide: [1, 3, 13, 15] },
        en: { ref: "Exodus 14:22", text: "The waters were divided and the children of Israel went into the sea on dry ground", hide: [1, 3, 12, 15] }
      },
      {
        fr: { ref: "Exode 15.2", text: "L'Éternel est ma force et le sujet de mes louanges c'est lui qui m'a sauvé", hide: [3, 9, 14] },
        en: { ref: "Exodus 15:2", text: "The LORD is my strength and song and he is become my salvation", hide: [4, 6, 12] }
      },
      {
        fr: { ref: "Exode 16.4", text: "Voici je vais faire pleuvoir pour vous du pain du haut des cieux", hide: [4, 8, 12] },
        en: { ref: "Exodus 16:4", text: "Behold I will rain bread from heaven for you", hide: [3, 4, 6] }
      },
      {
        fr: { ref: "Exode 20.2", text: "Je suis l'Éternel ton Dieu tu n'auras pas d'autres dieux devant ma face", hide: [2, 4, 6, 9] },
        en: { ref: "Exodus 20:2", text: "I am the LORD thy God thou shalt have no other gods before me", hide: [3, 5, 10, 11] }
      },
      {
        fr: { ref: "Exode 20.12", text: "Honore ton père et ta mère afin que tes jours se prolongent", hide: [0, 2, 5, 11] },
        en: { ref: "Exodus 20:12", text: "Honour thy father and thy mother that thy days may be long", hide: [0, 2, 5, 11] }
      }
    ]
  },
  {
    id: "david",
    emoji: "🪨",
    fr: { title: "La vie de David", subtitle: "Le petit berger devenu roi" },
    en: { title: "The life of David", subtitle: "The shepherd boy who became king" },
    verses: [
      {
        fr: { ref: "1 Samuel 16.7", text: "L'Éternel ne regarde pas à ce que l'homme regarde l'Éternel regarde au coeur", hide: [2, 7, 9, 12] },
        en: { ref: "1 Samuel 16:7", text: "Man looketh on the outward appearance but the LORD looketh on the heart", hide: [1, 5, 8, 12] }
      },
      {
        fr: { ref: "1 Samuel 16.13", text: "Samuel prit la corne d'huile et l'esprit de l'Éternel saisit David", hide: [0, 3, 6, 9] },
        en: { ref: "1 Samuel 16:13", text: "Samuel took the horn of oil and the Spirit of the LORD came upon David", hide: [0, 3, 8, 14] }
      },
      {
        fr: { ref: "1 Samuel 17.37", text: "L'Éternel qui m'a délivré de la griffe du lion me délivrera de ce Philistin", hide: [3, 6, 8, 13] },
        en: { ref: "1 Samuel 17:37", text: "The LORD that delivered me out of the paw of the lion will deliver me from this Philistine", hide: [3, 8, 11, 17] }
      },
      {
        fr: { ref: "1 Samuel 17.45", text: "Tu marches contre moi avec l'épée et moi je marche au nom de l'Éternel", hide: [1, 5, 9, 13] },
        en: { ref: "1 Samuel 17:45", text: "Thou comest to me with a sword but I come to thee in the name of the LORD", hide: [1, 6, 9, 17] }
      },
      {
        fr: { ref: "1 Samuel 18.1", text: "L'âme de Jonathan s'attacha à l'âme de David et Jonathan l'aima comme son âme", hide: [2, 3, 7, 10] },
        en: { ref: "1 Samuel 18:1", text: "The soul of Jonathan was knit with the soul of David and Jonathan loved him as his own soul", hide: [3, 5, 10, 13] }
      },
      {
        fr: { ref: "1 Samuel 24.6", text: "Que l'Éternel me garde de commettre une telle action contre mon seigneur", hide: [3, 5, 8, 11] },
        en: { ref: "1 Samuel 24:6", text: "The LORD forbid that I should do this thing unto my master", hide: [2, 8, 11] }
      },
      {
        fr: { ref: "2 Samuel 7.16", text: "Ta maison et ton règne seront pour toujours affermis", hide: [1, 4, 8] },
        en: { ref: "2 Samuel 7:16", text: "Thine house and thy kingdom shall be established for ever", hide: [1, 4, 7] }
      },
      {
        fr: { ref: "Psaume 23.1", text: "L'Éternel est mon berger je ne manquerai de rien", hide: [3, 6, 8] },
        en: { ref: "Psalm 23:1", text: "The LORD is my shepherd I shall not want", hide: [4, 6, 8] }
      },
      {
        fr: { ref: "Psaume 27.1", text: "L'Éternel est ma lumière et mon salut de qui aurais-je crainte", hide: [3, 6, 10] },
        en: { ref: "Psalm 27:1", text: "The LORD is my light and my salvation whom shall I fear", hide: [4, 7, 11] }
      },
      {
        fr: { ref: "Psaume 34.9", text: "Sentez et voyez combien l'Éternel est bon heureux l'homme qui cherche en lui son refuge", hide: [0, 2, 6, 14] },
        en: { ref: "Psalm 34:8", text: "O taste and see that the LORD is good blessed is the man that trusteth in him", hide: [1, 8, 9, 14] }
      },
      {
        fr: { ref: "Psaume 103.1", text: "Mon âme bénis l'Éternel que tout ce qui est en moi bénisse son saint nom", hide: [1, 2, 11, 14] },
        en: { ref: "Psalm 103:1", text: "Bless the LORD O my soul and all that is within me bless his holy name", hide: [0, 5, 10, 15] }
      },
      {
        fr: { ref: "1 Chroniques 29.11", text: "A toi Éternel la grandeur la force et la magnificence et la gloire", hide: [4, 6, 9, 12] },
        en: { ref: "1 Chronicles 29:11", text: "Thine O LORD is the greatness and the power and the glory", hide: [5, 8, 11] }
      }
    ]
  },
  {
    id: "jesus",
    emoji: "🌟",
    fr: { title: "La naissance de Jésus", subtitle: "Noël : Dieu vient habiter parmi nous" },
    en: { title: "The birth of Jesus", subtitle: "Christmas: God comes to live among us" },
    verses: [
      {
        fr: { ref: "Luc 1.31", text: "N'aie pas de crainte Marie tu enfanteras un fils et tu lui donneras le nom de Jésus", hide: [3, 4, 6, 16] },
        en: { ref: "Luke 1:31", text: "Fear not Mary thou shalt bring forth a son and shalt call his name Jesus", hide: [2, 5, 8, 14] }
      },
      {
        fr: { ref: "Luc 1.37", text: "Car rien n'est impossible à Dieu", hide: [1, 3, 5] },
        en: { ref: "Luke 1:37", text: "For with God nothing shall be impossible", hide: [2, 3, 6] }
      },
      {
        fr: { ref: "Luc 1.38", text: "Je suis la servante du Seigneur qu'il me soit fait selon ta parole", hide: [3, 5, 9, 12] },
        en: { ref: "Luke 1:38", text: "Behold the handmaid of the Lord be it unto me according to thy word", hide: [2, 5, 10, 13] }
      },
      {
        fr: { ref: "Matthieu 1.21", text: "Elle enfantera un fils c'est lui qui sauvera son peuple de ses péchés", hide: [1, 7, 9, 12] },
        en: { ref: "Matthew 1:21", text: "She shall bring forth a son and he shall save his people from their sins", hide: [2, 5, 9, 14] }
      },
      {
        fr: { ref: "Matthieu 1.23", text: "On lui donnera le nom d'Emmanuel ce qui signifie Dieu avec nous", hide: [2, 5, 8, 9] },
        en: { ref: "Matthew 1:23", text: "They shall call his name Emmanuel which being interpreted is God with us", hide: [2, 5, 8, 10] }
      },
      {
        fr: { ref: "Luc 2.4", text: "Joseph monta à Bethléhem pour se faire inscrire avec Marie sa fiancée", hide: [0, 3, 9, 11] },
        en: { ref: "Luke 2:4", text: "Joseph went up to Bethlehem to be taxed with Mary his espoused wife", hide: [0, 4, 9, 12] }
      },
      {
        fr: { ref: "Luc 2.7", text: "Elle enfanta son fils premier-né et le coucha dans une crèche", hide: [1, 4, 7, 10] },
        en: { ref: "Luke 2:7", text: "She brought forth her firstborn son and laid him in a manger", hide: [1, 4, 7, 11] }
      },
      {
        fr: { ref: "Luc 2.11", text: "Ne craignez point il vous est né un Sauveur qui est le Christ le Seigneur", hide: [6, 8, 12, 14] },
        en: { ref: "Luke 2:11", text: "Fear not for unto you is born a Saviour which is Christ the Lord", hide: [6, 8, 11, 13] }
      },
      {
        fr: { ref: "Luc 2.14", text: "Gloire à Dieu dans les lieux très hauts et paix sur la terre", hide: [0, 2, 9, 12] },
        en: { ref: "Luke 2:14", text: "Glory to God in the highest and on earth peace", hide: [0, 2, 8, 9] }
      },
      {
        fr: { ref: "Luc 2.20", text: "Les bergers s'en retournèrent glorifiant et louant Dieu pour tout ce qu'ils avaient vu", hide: [1, 4, 6, 13] },
        en: { ref: "Luke 2:20", text: "The shepherds returned glorifying and praising God for all that they had seen", hide: [1, 3, 5, 12] }
      },
      {
        fr: { ref: "Matthieu 2.2", text: "Nous avons vu son étoile en Orient et nous sommes venus pour l'adorer", hide: [2, 4, 6, 12] },
        en: { ref: "Matthew 2:2", text: "We have seen his star in the east and are come to worship him", hide: [2, 4, 7, 12] }
      },
      {
        fr: { ref: "Matthieu 2.11", text: "Ils virent le petit enfant avec Marie sa mère et se prosternant ils l'adorèrent", hide: [1, 4, 6, 13] },
        en: { ref: "Matthew 2:11", text: "They saw the young child with Mary his mother and fell down and worshipped him", hide: [1, 4, 6, 13] }
      }
    ]
  },
  {
    id: "paul",
    emoji: "📜",
    fr: { title: "La vie de Paul", subtitle: "De persécuteur à apôtre de Jésus" },
    en: { title: "The life of Paul", subtitle: "From persecutor to apostle of Jesus" },
    verses: [
      {
        fr: { ref: "Actes 9.3", text: "Tout à coup une lumière venant du ciel resplendit autour de Saul", hide: [4, 7, 8, 11] },
        en: { ref: "Acts 9:3", text: "Suddenly there shined round about him a light from heaven", hide: [0, 2, 7, 9] }
      },
      {
        fr: { ref: "Actes 9.4", text: "Saul Saul pourquoi me persécutes-tu", hide: [0, 2, 4] },
        en: { ref: "Acts 9:4", text: "Saul Saul why persecutest thou me", hide: [0, 2, 3] }
      },
      {
        fr: { ref: "Actes 9.15", text: "Va car cet homme est un instrument que j'ai choisi pour porter mon nom", hide: [6, 9, 11, 13] },
        en: { ref: "Acts 9:15", text: "Go thy way for he is a chosen vessel unto me to bear my name", hide: [7, 8, 12, 14] }
      },
      {
        fr: { ref: "Actes 9.18", text: "Il recouvra la vue il se leva et fut baptisé", hide: [1, 3, 6, 9] },
        en: { ref: "Acts 9:18", text: "He received sight forthwith and arose and was baptized", hide: [1, 2, 5, 8] }
      },
      {
        fr: { ref: "Actes 16.9", text: "Passe en Macédoine et viens à notre secours", hide: [0, 2, 7] },
        en: { ref: "Acts 16:9", text: "Come over into Macedonia and help us", hide: [0, 3, 5] }
      },
      {
        fr: { ref: "Actes 16.25", text: "Vers le milieu de la nuit Paul et Silas priaient et chantaient les louanges de Dieu", hide: [5, 6, 8, 9] },
        en: { ref: "Acts 16:25", text: "At midnight Paul and Silas prayed and sang praises unto God", hide: [1, 4, 5, 7] }
      },
      {
        fr: { ref: "Actes 17.11", text: "Ils recevaient la parole avec empressement et examinaient chaque jour les Écritures", hide: [1, 3, 7, 11] },
        en: { ref: "Acts 17:11", text: "They received the word with readiness of mind and searched the scriptures daily", hide: [1, 3, 9, 11] }
      },
      {
        fr: { ref: "Romains 1.16", text: "Je n'ai point honte de l'Évangile c'est une puissance de Dieu pour le salut", hide: [3, 5, 8, 13] },
        en: { ref: "Romans 1:16", text: "I am not ashamed of the gospel it is the power of God unto salvation", hide: [3, 6, 10, 14] }
      },
      {
        fr: { ref: "Romains 8.28", text: "Toutes choses concourent au bien de ceux qui aiment Dieu", hide: [2, 4, 8, 9] },
        en: { ref: "Romans 8:28", text: "All things work together for good to them that love God", hide: [2, 5, 9, 10] }
      },
      {
        fr: { ref: "Philippiens 4.13", text: "Je puis tout par celui qui me fortifie", hide: [1, 4, 7] },
        en: { ref: "Philippians 4:13", text: "I can do all things through Christ which strengtheneth me", hide: [1, 6, 8] }
      },
      {
        fr: { ref: "Galates 5.22", text: "Le fruit de l'Esprit c'est l'amour la joie la paix la patience la bonté", hide: [1, 5, 7, 11, 13] },
        en: { ref: "Galatians 5:22", text: "The fruit of the Spirit is love joy peace longsuffering gentleness goodness", hide: [1, 4, 7, 9, 11] }
      },
      {
        fr: { ref: "2 Timothée 4.7", text: "J'ai combattu le bon combat j'ai achevé la course j'ai gardé la foi", hide: [1, 4, 6, 8, 12] },
        en: { ref: "2 Timothy 4:7", text: "I have fought a good fight I have finished my course I have kept the faith", hide: [2, 5, 8, 10, 15] }
      }
    ]
  }
];
