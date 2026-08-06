/* Contrôleur principal : navigation entre écrans, progression, sauvegarde. */

const STORAGE_KEY = "douceParoleProgressV2";
const TOTAL_LEVELS = PARCOURS.reduce((s, p) => s + p.levels.length, 0);

const state = {
  progress: {}, // { [parcoursId]: { unlocked, stars: {levelId:n}, learned: {levelId:true} } }
  currentParcoursId: null,
  currentLevelId: null,
  board: null,
  score: 0,
  movesLeft: 0,
  target: 0
};

function emptyParcoursProgress() {
  return { unlocked: 1, stars: {}, learned: {} };
}

function getParcours(id) {
  return PARCOURS.find((p) => p.id === id);
}

function getProgress(parcoursId) {
  if (!state.progress[parcoursId]) state.progress[parcoursId] = emptyParcoursProgress();
  return state.progress[parcoursId];
}

function currentParcours() {
  return getParcours(state.currentParcoursId);
}

function currentLevel() {
  return currentParcours().levels.find((l) => l.id === state.currentLevelId);
}

function loadProgress() {
  try {
    const raw = localStorage.getItem(STORAGE_KEY);
    if (raw) {
      const data = JSON.parse(raw);
      if (data && data.progress) state.progress = data.progress;
    }
  } catch (e) {
    /* stockage indisponible : on continue avec l'état par défaut */
  }
}

function saveProgress() {
  try {
    localStorage.setItem(STORAGE_KEY, JSON.stringify({ progress: state.progress }));
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
  let learnedCount = 0;
  PARCOURS.forEach((p) => {
    const prog = getProgress(p.id);
    learnedCount += Object.keys(prog.learned).length;
  });
  document.getElementById("home-stats").textContent =
    `📖 ${learnedCount} verset(s) appris sur ${TOTAL_LEVELS} • 🗺️ ${PARCOURS.length} parcours`;
}

/* ---------- CHOIX DU PARCOURS ---------- */

function renderParcoursList() {
  const list = document.getElementById("parcours-list");
  list.innerHTML = "";
  PARCOURS.forEach((p) => {
    const prog = getProgress(p.id);
    const learnedCount = Object.keys(prog.learned).length;
    const pct = Math.round((learnedCount / p.levels.length) * 100);
    const done = learnedCount === p.levels.length;

    const card = document.createElement("button");
    card.className = "parcours-card";
    card.innerHTML = `
      <div class="parcours-emoji">${p.emoji}</div>
      <div class="parcours-info">
        <div class="p-title">${p.title}</div>
        <div class="p-subtitle">${p.subtitle}</div>
        <div class="p-progress"><div class="p-progress-bar" style="width:${pct}%"></div></div>
        <div class="p-progress-text">${learnedCount} / ${p.levels.length} versets appris</div>
      </div>
      <div class="parcours-check">${done ? "🏆" : "▶️"}</div>
    `;
    card.addEventListener("click", () => {
      state.currentParcoursId = p.id;
      renderLevels();
      navTo("levels");
    });
    list.appendChild(card);
  });
}

/* ---------- CARTE DES NIVEAUX ---------- */

function renderLevels() {
  const p = currentParcours();
  const prog = getProgress(p.id);
  document.getElementById("levels-title").textContent = `${p.emoji} ${p.title}`;

  const grid = document.getElementById("levels-grid");
  grid.innerHTML = "";
  p.levels.forEach((level) => {
    const locked = level.id > prog.unlocked;
    const btn = document.createElement("button");
    btn.className = "level-btn" + (locked ? " locked" : "");
    const starCount = prog.stars[level.id] || 0;
    btn.innerHTML = `<div>${level.id}</div><div class="lv-stars">${locked ? "" : "⭐".repeat(starCount) + "☆".repeat(3 - starCount)}</div>`;
    if (!locked) {
      btn.addEventListener("click", () => startLevel(level.id));
    }
    grid.appendChild(btn);
  });
}

/* ---------- JEU ---------- */

let levelFinished = false;

function startLevel(levelId) {
  levelFinished = false;
  const p = currentParcours();
  const level = p.levels.find((l) => l.id === levelId);
  state.currentLevelId = levelId;
  state.score = 0;
  state.movesLeft = level.moves;
  state.target = level.target;

  document.getElementById("game-ref").textContent = `${p.emoji} ${level.ref}`;
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

function finishLevel(win) {
  if (levelFinished) return;
  levelFinished = true;

  const p = currentParcours();
  const prog = getProgress(p.id);
  const level = currentLevel();

  if (win) {
    const ratio = state.score / state.target;
    let stars = 1;
    if (ratio >= 1.6) stars = 3;
    else if (ratio >= 1.3) stars = 2;

    prog.stars[level.id] = Math.max(prog.stars[level.id] || 0, stars);
    if (level.id === prog.unlocked && level.id < p.levels.length) {
      prog.unlocked = level.id + 1;
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
  const level = currentLevel();
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
  const prog = getProgress(state.currentParcoursId);
  prog.learned[levelId] = true;
  saveProgress();
}

/* ---------- BIBLIOTHÈQUE ---------- */

function renderLibrary() {
  const list = document.getElementById("library-list");
  list.innerHTML = "";

  const sections = PARCOURS.map((p) => {
    const prog = getProgress(p.id);
    const learnedIds = Object.keys(prog.learned).map(Number).sort((a, b) => a - b);
    return { p, learnedIds };
  }).filter((s) => s.learnedIds.length > 0);

  if (sections.length === 0) {
    list.innerHTML = '<p class="library-empty">Tu n\'as encore appris aucun verset.<br>Gagne un niveau pour commencer ! 🍭</p>';
    return;
  }

  sections.forEach(({ p, learnedIds }) => {
    const heading = document.createElement("h3");
    heading.className = "library-section";
    heading.textContent = `${p.emoji} ${p.title}`;
    list.appendChild(heading);

    learnedIds.forEach((id) => {
      const level = p.levels.find((l) => l.id === id);
      const div = document.createElement("div");
      div.className = "library-item";
      div.innerHTML = `<div class="ref">${level.ref}</div><div class="txt">${level.text}.</div>`;
      list.appendChild(div);
    });
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
      if (target === "parcours") renderParcoursList();
      if (target === "levels") renderLevels();
      navTo(target);
    });
  });

  document.getElementById("btn-play").addEventListener("click", () => {
    renderParcoursList();
    navTo("parcours");
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
    const p = currentParcours();
    const prog = getProgress(p.id);
    const nextId = state.currentLevelId + 1;
    if (nextId <= p.levels.length && nextId <= prog.unlocked) {
      startLevel(nextId);
    } else {
      renderLevels();
      navTo("levels");
    }
  });

  navTo("home");
});
