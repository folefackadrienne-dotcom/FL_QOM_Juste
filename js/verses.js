/* Données des versets bibliques (Louis Segond 1910, domaine public).
   Chaque niveau associe un objectif de jeu à un verset à mémoriser.
   "hide" liste les mots (index dans le tableau "words") à faire deviner. */

const LEVELS = [
  {
    id: 1,
    ref: "Psaume 23.1",
    text: "L'Éternel est mon berger je ne manquerai de rien",
    hide: [1, 3, 6],
    target: 400,
    moves: 18,
    tiles: 6
  },
  {
    id: 2,
    ref: "Jean 3.16",
    text: "Dieu a tant aimé le monde qu'il a donné son Fils unique",
    hide: [0, 2, 8, 10],
    target: 500,
    moves: 18,
    tiles: 6
  },
  {
    id: 3,
    ref: "Philippiens 4.13",
    text: "Je puis tout par celui qui me fortifie",
    hide: [1, 4, 6],
    target: 500,
    moves: 17,
    tiles: 6
  },
  {
    id: 4,
    ref: "Proverbes 3.5",
    text: "Confie-toi en l'Éternel de tout ton cœur",
    hide: [0, 2, 5, 6],
    target: 550,
    moves: 17,
    tiles: 6
  },
  {
    id: 5,
    ref: "Josué 1.9",
    text: "Fortifie-toi et prends courage car l'Éternel ton Dieu est avec toi",
    hide: [0, 3, 7, 9],
    target: 600,
    moves: 18,
    tiles: 6
  },
  {
    id: 6,
    ref: "Matthieu 5.16",
    text: "Que votre lumière luise ainsi devant les hommes",
    hide: [2, 3, 6],
    target: 600,
    moves: 16,
    tiles: 6
  },
  {
    id: 7,
    ref: "Éphésiens 4.32",
    text: "Soyez bons les uns envers les autres et compatissants",
    hide: [1, 4, 6, 8],
    target: 650,
    moves: 16,
    tiles: 5
  },
  {
    id: 8,
    ref: "Psaume 119.105",
    text: "Ta parole est une lampe à mes pieds et une lumière sur mon sentier",
    hide: [1, 4, 6, 12],
    target: 700,
    moves: 17,
    tiles: 5
  },
  {
    id: 9,
    ref: "1 Jean 4.19",
    text: "Nous l'aimons parce qu'il nous a aimés le premier",
    hide: [1, 4, 7, 9],
    target: 700,
    moves: 16,
    tiles: 5
  },
  {
    id: 10,
    ref: "Marc 10.14",
    text: "Laissez venir à moi les petits enfants et ne les en empêchez pas",
    hide: [0, 3, 5, 8],
    target: 750,
    moves: 17,
    tiles: 5
  },
  {
    id: 11,
    ref: "Galates 5.22",
    text: "Le fruit de l'Esprit c'est l'amour la joie la paix la patience la bonté",
    hide: [0, 5, 8, 11, 14],
    target: 800,
    moves: 18,
    tiles: 5
  },
  {
    id: 12,
    ref: "Romains 8.28",
    text: "Toutes choses concourent au bien de ceux qui aiment Dieu",
    hide: [0, 3, 6, 9],
    target: 800,
    moves: 16,
    tiles: 5
  }
];

/* Symboles à thème biblique utilisés comme "bonbons" du jeu */
const TILE_SYMBOLS = ["✝️", "🕊️", "🐟", "⭐", "👑", "📖", "🍇"];
