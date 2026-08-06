/* Contrôleur principal : navigation entre écrans, progression, sauvegarde. */

const STORAGE_KEY = "douceParoleProgress";

const state = {
  unlocked: 1,
  stars: {},
  learned: {},
  currentLevelId: null,
  board: null,
  score: 0,
  movesLeft: 0,
  target: 0
};

function loadProgress() {
  try {
    const raw = localStorage.getItem(STORAGE_KEY);
    if (raw) {
      const data = JSON.parse(raw);
      state.unlocked = data.unlocked || 1;
      state.stars = data.stars || {};
      state.learned = data.learned || {};
    }
  } catch (e) {
    /* stockage indisponible : on continue avec l'état par défaut */
  }
}

function saveProgress() {
  try {
    localStorage.setItem(
      STORAGE_KEY,
      JSON.stringify({ unlocked: state.unlocked, stars: state.stars, learned: state.learned })
    );
  } catch (e) {
    /* stockage indisponible : progression non persistée */
  }
}

function showToast(msg) {
  const toast = document.getElementById("toast");
  toast.textContent = msg;
  toast.classList.add("show");
  setTimeout(() => toast.classList.remove("show"), 1800);
}

function navTo(id) {
  document.querySelectorAll(".screen").forEach((s) => s.classList.remove("active"));
  document.getElementById("screen-" + id).classList.add("active");
}

/* ---------- ACCUEIL ---------- */

function renderHome() {
  const learnedCount = Object.keys(state.learned).length;
  document.getElementById("home-stats").textContent =
    `📖 ${learnedCount} verset(s) appris sur ${LEVELS.length} • 🗺️ Niveau ${state.unlocked}`;
}

/* ---------- CARTE DES NIVEAUX ---------- */

function renderLevels() {
  const grid = document.getElementById("levels-grid");
  grid.innerHTML = "";
  LEVELS.forEach((level) => {
    const locked = level.id > state.unlocked;
    const btn = document.createElement("button");
    btn.className = "level-btn" + (locked ? " locked" : "");
    const starCount = state.stars[level.id] || 0;
    btn.innerHTML = `<div>${level.id}</div><div class="lv-stars">${locked ? "" : "⭐".repeat(starCount) + "☆".repeat(3 - starCount)}</div>`;
    if (!locked) {
      btn.addEventListener("click", () => startLevel(level.id));
    }
    grid.appendChild(btn);
  });
}

/* ---------- JEU ---------- */

function startLevel(levelId) {
  levelFinished = false;
  const level = LEVELS.find((l) => l.id === levelId);
  state.currentLevelId = levelId;
  state.score = 0;
  state.movesLeft = level.moves;
  state.target = level.target;

  document.getElementById("game-ref").textContent = level.ref;
  document.getElementById("verse-preview").textContent =
    "Gagne ce niveau pour mémoriser : « " + level.text + " »";
  updateHud();

  const container = document.getElementById("board");
  if (state.board) state.board.destroy();
  state.board = new Board(container, {
    rows: 8,
    cols: 8,
    tiles: level.tiles,
    onScore: handleScore,
    onMove: handleMove,
    onInvalid: () => {}
  });
  state.board.init();

  navTo("game");
}

function updateHud() {
  document.getElementById("hud-score").textContent = state.score;
  document.getElementById("hud-target").textContent = state.target;
  document.getElementById("hud-moves").textContent = state.movesLeft;
  document.getElementById("hud-stars").textContent = starsPreview();
}

function starsPreview() {
  const ratio = state.target > 0 ? state.score / state.target : 0;
  let n = 0;
  if (ratio >= 1) n = 1;
  if (ratio >= 1.3) n = 2;
  if (ratio >= 1.6) n = 3;
  return "⭐".repeat(n) + "☆".repeat(3 - n);
}

function handleScore(gained) {
  state.score += gained;
  updateHud();
  if (state.score >= state.target) {
    setTimeout(() => finishLevel(true), 350);
  }
}

function handleMove() {
  state.movesLeft -= 1;
  updateHud();
  if (state.movesLeft <= 0 && state.score < state.target) {
    setTimeout(() => finishLevel(false), 500);
  }
}

let levelFinished = false;

function finishLevel(win) {
  if (levelFinished) return;
  levelFinished = true;

  const level = LEVELS.find((l) => l.id === state.currentLevelId);

  if (win) {
    const ratio = state.score / state.target;
    let stars = 1;
    if (ratio >= 1.6) stars = 3;
    else if (ratio >= 1.3) stars = 2;

    state.stars[level.id] = Math.max(state.stars[level.id] || 0, stars);
    if (level.id === state.unlocked && level.id < LEVELS.length) {
      state.unlocked = level.id + 1;
    } else if (level.id === state.unlocked && level.id === LEVELS.length) {
      state.unlocked = level.id; // dernier niveau
    }
    saveProgress();

    document.getElementById("result-emoji").textContent = "🎉";
    document.getElementById("result-title").textContent = "Niveau réussi !";
    document.getElementById("result-stars").textContent = "⭐".repeat(stars) + "☆".repeat(3 - stars);
    document.getElementById("result-text").textContent =
      `Score : ${state.score} / ${state.target}. Tu peux maintenant apprendre le verset de ce niveau.`;
    document.getElementById("btn-to-verse").classList.remove("hidden");
  } else {
    document.getElementById("result-emoji").textContent = "🙏";
    document.getElementById("result-title").textContent = "Essaie encore !";
    document.getElementById("result-stars").textContent = "";
    document.getElementById("result-text").textContent =
      `Tu n'as pas atteint le score demandé (${state.score} / ${state.target}). Courage, réessaie !`;
    document.getElementById("btn-to-verse").classList.add("hidden");
  }

  navTo("result");
}

/* ---------- VERSET ---------- */

function openVerseGameForCurrentLevel() {
  const level = LEVELS.find((l) => l.id === state.currentLevelId);
  VerseGame.start(level, {
    title: document.getElementById("verse-ref-title"),
    sentence: document.getElementById("verse-sentence"),
    bank: document.getElementById("word-bank"),
    feedback: document.getElementById("verse-feedback"),
    checkBtn: document.getElementById("btn-check-verse"),
    nextBtn: document.getElementById("btn-next-level")
  });
  navTo("verse");
}

function markVerseLearned(levelId) {
  state.learned[levelId] = true;
  saveProgress();
}

/* ---------- BIBLIOTHÈQUE ---------- */

function renderLibrary() {
  const list = document.getElementById("library-list");
  list.innerHTML = "";
  const learnedIds = Object.keys(state.learned).map(Number).sort((a, b) => a - b);

  if (learnedIds.length === 0) {
    list.innerHTML = '<p class="library-empty">Tu n\'as encore appris aucun verset.<br>Gagne un niveau pour commencer ! 🍭</p>';
    return;
  }

  learnedIds.forEach((id) => {
    const level = LEVELS.find((l) => l.id === id);
    const div = document.createElement("div");
    div.className = "library-item";
    div.innerHTML = `<div class="ref">${level.ref}</div><div class="txt">${level.text}.</div>`;
    list.appendChild(div);
  });
}

/* ---------- ÉVÉNEMENTS ---------- */

document.addEventListener("DOMContentLoaded", () => {
  loadProgress();
  renderHome();

  document.querySelectorAll("[data-nav]").forEach((btn) => {
    btn.addEventListener("click", () => {
      const target = btn.dataset.nav;
      if (target === "home") renderHome();
      if (target === "levels") renderLevels();
      navTo(target);
    });
  });

  document.getElementById("btn-play").addEventListener("click", () => {
    renderLevels();
    navTo("levels");
  });

  document.getElementById("btn-library").addEventListener("click", () => {
    renderLibrary();
    navTo("library");
  });

  document.getElementById("btn-help").addEventListener("click", () => navTo("help"));

  document.getElementById("btn-quit-game").addEventListener("click", () => {
    if (state.board) state.board.destroy();
    renderLevels();
    navTo("levels");
  });

  document.getElementById("btn-to-verse").addEventListener("click", () => {
    openVerseGameForCurrentLevel();
  });

  document.getElementById("btn-retry").addEventListener("click", () => {
    levelFinished = false;
    startLevel(state.currentLevelId);
  });

  document.getElementById("btn-back-levels").addEventListener("click", () => {
    renderLevels();
    navTo("levels");
  });

  document.getElementById("btn-check-verse").addEventListener("click", () => {
    const success = VerseGame.check();
    if (success) {
      markVerseLearned(state.currentLevelId);
      showToast("Verset mémorisé ! 🌟");
    }
  });

  document.getElementById("btn-next-level").addEventListener("click", () => {
    const nextId = state.currentLevelId + 1;
    renderLevels();
    navTo("levels");
    if (nextId <= LEVELS.length && nextId <= state.unlocked) {
      levelFinished = false;
      startLevel(nextId);
    }
  });

  navTo("home");
});
