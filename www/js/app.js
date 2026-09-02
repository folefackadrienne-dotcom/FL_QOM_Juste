/* Contrôleur principal : navigation entre écrans, progression, sauvegarde, langue. */

const STORAGE_KEY = "croqueVersetsProgressV1";
const TOTAL_LEVELS = PARCOURS.reduce((s, p) => s + p.verses.length, 0);

const state = {
  lang: "fr",
  sound: true,
  progress: {}, // { [parcoursId]: { unlocked, stars: {levelId:n}, learned: {levelId:true} } }
  currentParcoursId: null,
  currentLevelId: null,
  lastWin: false,
  board: null,
  score: 0,
  movesLeft: 0,
  timeLeft: 0,
  target: 0,
  collected: {},
  collectGoal: null,
  timerId: null,
  paused: false
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

function parcoursText(p) {
  return p[state.lang];
}

function currentParcours() {
  return getParcours(state.currentParcoursId);
}

function currentLevels() {
  return buildLevels(currentParcours().verses, state.lang);
}

function currentLevel() {
  return currentLevels().find((l) => l.id === state.currentLevelId);
}

function loadProgress() {
  try {
    const raw = localStorage.getItem(STORAGE_KEY);
    if (raw) {
      const data = JSON.parse(raw);
      if (data && data.progress) state.progress = data.progress;
      if (data && (data.lang === "fr" || data.lang === "en")) state.lang = data.lang;
      if (data && typeof data.sound === "boolean") state.sound = data.sound;
    }
  } catch (e) {
    /* stockage indisponible : on continue avec l'état par défaut */
  }
  SFX.setEnabled(state.sound);
}

function saveProgress() {
  try {
    localStorage.setItem(STORAGE_KEY, JSON.stringify({ progress: state.progress, lang: state.lang, sound: state.sound }));
  } catch (e) {
    /* stockage indisponible : progression non persistée */
  }
}

/* Partie en cours (pas juste les étoiles gagnées) : permet de fermer
   l'appli en pleine partie et de reprendre exactement où on en était. */
const INPROGRESS_KEY = "croqueVersetsInProgressV1";

function saveInProgress() {
  if (!state.board || !state.currentParcoursId || !state.currentLevelId || levelFinished) return;
  try {
    localStorage.setItem(
      INPROGRESS_KEY,
      JSON.stringify({
        parcoursId: state.currentParcoursId,
        levelId: state.currentLevelId,
        score: state.score,
        movesLeft: state.movesLeft,
        timeLeft: state.timeLeft,
        collected: state.collected,
        grid: state.board.grid,
        revealed: state.board.revealed
      })
    );
  } catch (e) {
    /* stockage indisponible : reprise impossible, sans gravité */
  }
}

function loadInProgress() {
  try {
    const raw = localStorage.getItem(INPROGRESS_KEY);
    return raw ? JSON.parse(raw) : null;
  } catch (e) {
    return null;
  }
}

function clearInProgress() {
  try {
    localStorage.removeItem(INPROGRESS_KEY);
  } catch (e) {
    /* rien à faire */
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

const SHARE_URL = "https://play.google.com/store/apps/details?id=com.croqueversets.app";

function shareApp() {
  const text = t("share_text");
  if (navigator.share) {
    navigator.share({ title: "Croque-Versets", text, url: SHARE_URL }).catch(() => {});
    return;
  }
  if (navigator.clipboard && navigator.clipboard.writeText) {
    navigator.clipboard.writeText(`${text} ${SHARE_URL}`).then(() => showToast(t("share_copied")));
  }
}

/* ---------- TEXTES STATIQUES / LANGUE ---------- */

function applyStaticI18n() {
  document.documentElement.lang = state.lang;
  document.title = t("doc_title");
  document.getElementById("lang-toggle").textContent = "🌐 " + t("lang_switch_label");
  document.getElementById("sound-toggle").textContent = state.sound ? "🔊" : "🔇";

  document.getElementById("home-subtitle").textContent = t("home_subtitle");
  document.getElementById("btn-play").textContent = t("btn_play");
  document.getElementById("btn-library").textContent = t("btn_library");
  document.getElementById("btn-help").textContent = t("btn_help");
  document.getElementById("btn-share").textContent = t("btn_share");
  document.getElementById("btn-activate").textContent = t("btn_activate");
  document.getElementById("btn-activate-link").textContent = t("btn_activate_link");
  document.getElementById("activate-title").textContent = t("activate_title");
  document.getElementById("activate-desc").textContent = t("activate_desc");
  document.getElementById("activate-code-input").placeholder = t("activate_placeholder");
  document.getElementById("btn-activate-submit").textContent = t("btn_activate_submit");
  document.getElementById("parcours-header").textContent = t("parcours_header");
  document.getElementById("hud-moves-suffix").textContent = t("hud_moves_suffix");
  document.getElementById("btn-to-verse").textContent = t("btn_to_verse");
  document.getElementById("btn-retry").textContent = t("btn_retry");
  document.getElementById("btn-back-levels").textContent = t("btn_back_levels");
  document.getElementById("btn-check-verse").textContent = t("btn_check_verse");
  document.getElementById("btn-next-level").textContent = t("btn_next_level");
  document.getElementById("library-header").textContent = t("library_header");
  document.getElementById("help-header").textContent = t("help_header");
  document.getElementById("pause-title").textContent = t("pause_title");
  document.getElementById("btn-resume-pause").textContent = t("btn_resume_pause");
  document.getElementById("btn-quit-pause").textContent = t("btn_quit_pause");
  document.getElementById("btn-restore-top").textContent = t("btn_restore");
  document.getElementById("btn-restore-unlock").textContent = t("btn_restore");
  document.getElementById("unlock-desc").textContent = t("unlock_desc");

  const helpLines = document.getElementById("help-lines");
  helpLines.innerHTML = t("help_lines").map((line) => `<p>${line}</p>`).join("");
  document.getElementById("help-quote").textContent = t("help_quote");
}

function refreshActiveScreen() {
  const active = document.querySelector(".screen.active");
  if (!active) return;
  switch (active.id) {
    case "screen-home":
      renderHome();
      break;
    case "screen-parcours":
      renderParcoursList();
      break;
    case "screen-levels":
      renderLevels();
      break;
    case "screen-library":
      renderLibrary();
      break;
    case "screen-game":
      refreshGameTexts();
      break;
    case "screen-result":
      renderResultTexts();
      renderSymbolExplain();
      break;
    case "screen-verse":
      openVerseGameForCurrentLevel();
      break;
    case "screen-unlock":
      renderUnlockScreen(state.currentUnlockParcoursId);
      break;
  }
}

/* ---------- ACCUEIL ---------- */

function renderHome() {
  let learnedCount = 0;
  PARCOURS.forEach((p) => {
    const prog = getProgress(p.id);
    learnedCount += Object.keys(prog.learned).length;
  });
  document.getElementById("home-stats").textContent = t("home_stats", learnedCount, TOTAL_LEVELS, PARCOURS.length);

  const resumeBtn = document.getElementById("btn-resume");
  const saved = loadInProgress();
  const savedParcours = saved ? getParcours(saved.parcoursId) : null;
  if (savedParcours) {
    resumeBtn.textContent = t("btn_resume", parcoursText(savedParcours).title, saved.levelId);
    resumeBtn.classList.remove("hidden");
  } else {
    resumeBtn.classList.add("hidden");
  }
}

/* ---------- CHOIX DU PARCOURS ---------- */

function renderParcoursList() {
  const list = document.getElementById("parcours-list");
  list.innerHTML = "";
  PARCOURS.forEach((p) => {
    const prog = getProgress(p.id);
    const learnedCount = Object.keys(prog.learned).length;
    const pct = Math.round((learnedCount / p.verses.length) * 100);
    const done = learnedCount === p.verses.length;
    const txt = parcoursText(p);
    const unlocked = Billing.isUnlocked(p.id);

    let badge;
    if (!unlocked) {
      const price = Billing.priceFor(p.id);
      badge = `<div class="parcours-price">🔒${price ? " " + price : ""}</div>`;
    } else {
      badge = `<div class="parcours-check">${done ? "🏆" : "▶️"}</div>`;
    }

    const card = document.createElement("button");
    card.className = "parcours-card";
    card.innerHTML = `
      <div class="parcours-emoji">${p.emoji}</div>
      <div class="parcours-info">
        <div class="p-title">${txt.title}</div>
        <div class="p-subtitle">${txt.subtitle}</div>
        <div class="p-progress"><div class="p-progress-bar" style="width:${pct}%"></div></div>
        <div class="p-progress-text">${t("progress_text", learnedCount, p.verses.length)}</div>
      </div>
      ${badge}
    `;
    card.addEventListener("click", () => {
      // Les 2 premiers niveaux de chaque parcours sont toujours jouables :
      // on ouvre donc toujours la carte des niveaux, jamais directement
      // l'écran d'achat (celui-ci n'apparaît qu'en touchant un niveau payant).
      state.currentParcoursId = p.id;
      renderLevels();
      navTo("levels");
    });
    list.appendChild(card);
  });
}

/* ---------- DÉBLOCAGE (ACHATS) ---------- */

function renderUnlockScreen(parcoursId) {
  const p = getParcours(parcoursId);
  const txt = parcoursText(p);
  document.getElementById("unlock-title").textContent = `${p.emoji} ${txt.title}`;
  document.getElementById("unlock-feedback").textContent = "";
  document.getElementById("unlock-feedback").className = "verse-feedback";
  document.getElementById("btn-unlock-one").textContent = t("btn_unlock_one", Billing.priceFor(parcoursId));
  document.getElementById("btn-unlock-all").textContent = t("btn_unlock_all", Billing.bundlePrice());
}

function setUnlockFeedback(msg, ok) {
  const el = document.getElementById("unlock-feedback");
  el.textContent = msg;
  el.className = "verse-feedback " + (ok ? "ok" : "ko");
}

function handlePurchase(purchaseFn, parcoursId) {
  if (!Billing.available()) {
    setUnlockFeedback(t("purchase_unavailable"), false);
    return;
  }
  setUnlockFeedback(t("purchase_pending"), true);
  purchaseFn()
    .then(() => {
      if (Billing.isUnlocked(parcoursId)) {
        showToast(t("toast_unlocked"));
        state.currentParcoursId = parcoursId;
        renderLevels();
        navTo("levels");
      }
    })
    .catch(() => setUnlockFeedback(t("purchase_error"), false));
}

function handleRestore() {
  if (!Billing.available()) {
    showToast(t("purchase_unavailable"));
    return;
  }
  Billing.restore().then(() => {
    showToast(t("restore_done"));
    refreshActiveScreen();
  });
}

/* ---------- ACTIVATION PAR CODE ---------- */

const ACTIVATE_ERROR_KEYS = {
  empty: "activate_err_empty",
  unavailable: "activate_err_unavailable",
  "not-found": "activate_err_notfound",
  inactive: "activate_err_inactive",
  full: "activate_err_full",
  error: "activate_err_generic"
};

function setActivateFeedback(msg, ok) {
  const el = document.getElementById("activate-feedback");
  el.textContent = msg;
  el.className = "verse-feedback " + (ok ? "ok" : "ko");
}

function handleActivateSubmit() {
  const input = document.getElementById("activate-code-input");
  const code = input.value;
  setActivateFeedback(t("activate_pending"), true);
  Activation.activate(code)
    .then(() => {
      setActivateFeedback(t("activate_success"), true);
      showToast(t("toast_unlocked"));
      refreshActiveScreen();
    })
    .catch((err) => {
      const errCode = (err && err.code) || "error";
      const key = ACTIVATE_ERROR_KEYS[errCode] || ACTIVATE_ERROR_KEYS.error;
      const msg = t(key) + (err && err.debug ? " [debug: " + err.debug + "]" : "");
      setActivateFeedback(msg, false);
    });
}

/* ---------- CARTE DES NIVEAUX ---------- */

function renderLevels() {
  const p = currentParcours();
  const prog = getProgress(p.id);
  document.getElementById("levels-title").textContent = `${p.emoji} ${parcoursText(p).title}`;

  const grid = document.getElementById("levels-grid");
  grid.innerHTML = "";
  currentLevels().forEach((level) => {
    const progLocked = level.id > prog.unlocked;
    const paywalled = !progLocked && !Billing.isLevelUnlocked(p.id, level.id);
    const btn = document.createElement("button");
    btn.className = "level-btn" + (progLocked ? " locked" : "") + (paywalled ? " paywalled" : "");
    const starCount = prog.stars[level.id] || 0;
    btn.innerHTML = `<div>${level.id}</div><div class="lv-stars">${progLocked ? "" : "⭐".repeat(starCount) + "☆".repeat(3 - starCount)}</div>`;
    if (!progLocked) {
      if (paywalled) {
        btn.addEventListener("click", () => {
          state.currentUnlockParcoursId = p.id;
          renderUnlockScreen(p.id);
          navTo("unlock");
        });
      } else {
        btn.addEventListener("click", () => startLevel(level.id));
      }
    }
    grid.appendChild(btn);
  });
}

/* ---------- JEU ---------- */

let levelFinished = false;

function startLevel(levelId) {
  levelFinished = false;
  state.paused = false;
  state.currentLevelId = levelId;
  state.score = 0;
  state.collected = {};

  const level = currentLevel();
  state.movesLeft = level.moves;
  state.timeLeft = level.time;
  state.target = level.target;
  state.collectGoal = level.collectGoal;

  refreshGameTexts();
  updateHud();
  startTimer();

  const container = document.getElementById("board");
  if (state.board) state.board.destroy();
  state.board = new Board(container, {
    rows: 8,
    cols: 8,
    tiles: level.tiles,
    symbols: currentParcours().tileSymbols,
    specialSymbols: currentParcours().specialSymbols,
    illustrationSvg: currentParcours().illustrationSvg,
    onScore: handleScore,
    onMove: handleMove,
    onInvalid: () => {}
  });
  state.board.init();

  document.getElementById("pause-overlay").classList.add("hidden");
  navTo("game");
}

// Restaure une partie sauvegardée (fermeture de l'appli en pleine partie) :
// on reconstruit le plateau tel qu'il était, sans en tirer un nouveau.
function resumeLevel(saved) {
  const p = getParcours(saved.parcoursId);
  state.currentParcoursId = saved.parcoursId;
  state.currentLevelId = saved.levelId;
  const level = currentLevel();
  if (!p || !level) return false;

  levelFinished = false;
  state.paused = false;
  state.score = saved.score;
  state.collected = saved.collected || {};
  state.movesLeft = saved.movesLeft;
  state.timeLeft = saved.timeLeft;
  state.target = level.target;
  state.collectGoal = level.collectGoal;

  refreshGameTexts();
  updateHud();
  startTimer();

  const container = document.getElementById("board");
  if (state.board) state.board.destroy();
  state.board = new Board(container, {
    rows: 8,
    cols: 8,
    tiles: level.tiles,
    symbols: p.tileSymbols,
    specialSymbols: p.specialSymbols,
    illustrationSvg: p.illustrationSvg,
    onScore: handleScore,
    onMove: handleMove,
    onInvalid: () => {}
  });
  state.board.grid = saved.grid;
  state.board.revealed = saved.revealed;
  state.board.renderAll();

  document.getElementById("pause-overlay").classList.add("hidden");
  navTo("game");
  return true;
}

function pauseGame() {
  if (levelFinished || state.paused) return;
  state.paused = true;
  stopTimer();
  if (state.board) state.board.locked = true;
  saveInProgress();
  document.getElementById("pause-overlay").classList.remove("hidden");
}

function resumeGame() {
  if (!state.paused) return;
  state.paused = false;
  document.getElementById("pause-overlay").classList.add("hidden");
  if (state.board) state.board.locked = false;
  startTimer();
}

function quitGame() {
  stopTimer();
  state.paused = false;
  document.getElementById("pause-overlay").classList.add("hidden");
  clearInProgress();
  if (state.board) state.board.destroy();
  renderLevels();
  navTo("levels");
}

function startTimer() {
  stopTimer();
  state.timerId = setInterval(() => {
    state.timeLeft -= 1;
    updateHud();
    if (state.timeLeft <= 0) {
      stopTimer();
      finishLevel(levelWon());
    }
  }, 1000);
}

function stopTimer() {
  if (state.timerId) {
    clearInterval(state.timerId);
    state.timerId = null;
  }
}

function refreshGameTexts() {
  const p = currentParcours();
  const level = currentLevel();
  document.getElementById("game-ref").textContent = `${p.emoji} ${level.ref}`;
  document.getElementById("verse-preview").textContent = t("verse_preview", level.text);
}

function updateHud() {
  document.getElementById("hud-score").textContent = state.score;
  document.getElementById("hud-target").textContent = state.target;
  document.getElementById("hud-moves").textContent = state.movesLeft;
  document.getElementById("hud-timer").textContent = Math.max(0, state.timeLeft);
  document.getElementById("hud-timer-item").classList.toggle("low", state.timeLeft <= 10);
  document.getElementById("hud-stars").textContent = starsPreview();

  if (state.collectGoal) {
    const p = currentParcours();
    const idx = state.collectGoal.symbolIndex;
    const symbol = p.tileSymbols[idx];
    const got = state.collected[idx] || 0;
    // Une mission par niveau (pas par symbole) : le texte varie à chaque
    // répétition du même symbole au fil des 12 niveaux d'un parcours.
    document.getElementById("hud-goal-story").textContent = p.objectiveText[state.currentLevelId - 1][state.lang];
    document.getElementById("hud-goal-icon").textContent = symbol;
    document.getElementById("hud-goal-count-num").textContent = got;
    document.getElementById("hud-goal-target").textContent = state.collectGoal.count;
    document.getElementById("hud-goal-item").classList.toggle("reached", got >= state.collectGoal.count);
  }
}

function goalReached() {
  if (!state.collectGoal) return true;
  const got = state.collected[state.collectGoal.symbolIndex] || 0;
  return got >= state.collectGoal.count;
}

function levelWon() {
  return state.score >= state.target && goalReached();
}

function starsPreview() {
  const ratio = state.target > 0 ? state.score / state.target : 0;
  let n = 0;
  if (ratio >= 1) n = 1;
  if (ratio >= 1.3) n = 2;
  if (ratio >= 1.6) n = 3;
  return "⭐".repeat(n) + "☆".repeat(3 - n);
}

function renderStarsAnimated(elementId, filledCount) {
  const el = document.getElementById(elementId);
  el.innerHTML = "";
  for (let i = 0; i < 3; i++) {
    const span = document.createElement("span");
    span.className = "star";
    span.style.animationDelay = i * 0.12 + "s";
    span.textContent = i < filledCount ? "⭐" : "☆";
    el.appendChild(span);
  }
}

function handleScore(gained, matchedCount, cascadeLevel, typeCounts) {
  state.score += gained;
  if (typeCounts) {
    Object.keys(typeCounts).forEach((type) => {
      state.collected[type] = (state.collected[type] || 0) + typeCounts[type];
    });
  }
  updateHud();
}

function handleMove() {
  state.movesLeft -= 1;
  updateHud();
  if (state.movesLeft <= 0) {
    stopTimer();
    setTimeout(() => finishLevel(levelWon()), 500);
  } else {
    saveInProgress();
  }
}

function finishLevel(win) {
  if (levelFinished) return;
  levelFinished = true;
  state.lastWin = win;
  stopTimer();
  clearInProgress();

  const p = currentParcours();
  const prog = getProgress(p.id);
  const level = currentLevel();

  if (win) {
    const ratio = state.score / state.target;
    let stars = 1;
    if (ratio >= 1.6) stars = 3;
    else if (ratio >= 1.3) stars = 2;

    prog.stars[level.id] = Math.max(prog.stars[level.id] || 0, stars);
    if (level.id === prog.unlocked && level.id < currentLevels().length) {
      prog.unlocked = level.id + 1;
    }
    saveProgress();
    SFX.levelWin();
  } else {
    SFX.levelLose();
  }

  renderResultTexts();
  renderSymbolExplain();
  navTo("result");
}

function renderSymbolExplain() {
  const el = document.getElementById("symbol-explain");
  if (!state.collectGoal) {
    el.innerHTML = "";
    return;
  }
  const p = currentParcours();
  const idx = state.collectGoal.symbolIndex;
  const symbol = p.tileSymbols[idx];
  const meaning = p.symbolMeanings[idx][state.lang];
  el.innerHTML = `<div class="symbol-explain-label">${t("symbol_explain_label")}</div><div class="symbol-explain-body"><span class="symbol-explain-icon">${symbol}</span> ${meaning}</div>`;
}

function renderResultTexts() {
  const win = state.lastWin;
  if (win) {
    const ratio = state.target > 0 ? state.score / state.target : 0;
    let stars = 1;
    if (ratio >= 1.6) stars = 3;
    else if (ratio >= 1.3) stars = 2;

    document.getElementById("result-emoji").textContent = "🎉";
    document.getElementById("result-title").textContent = t("result_win_title");
    renderStarsAnimated("result-stars", stars);
    document.getElementById("result-text").textContent = t("result_win_text", state.score, state.target);
    document.getElementById("btn-to-verse").classList.remove("hidden");
    document.getElementById("result-illustration").innerHTML = currentParcours().illustrationSvg || "";
    VFX.confetti();
  } else {
    document.getElementById("result-emoji").textContent = "🙏";
    document.getElementById("result-title").textContent = t("result_lose_title");
    document.getElementById("result-stars").textContent = "";
    document.getElementById("result-illustration").innerHTML = "";
    const got = state.collectGoal ? state.collected[state.collectGoal.symbolIndex] || 0 : 0;
    const need = state.collectGoal ? state.collectGoal.count : 0;
    document.getElementById("result-text").textContent = t("result_lose_text", state.score, state.target, got, need);
    document.getElementById("btn-to-verse").classList.add("hidden");
  }
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
    list.innerHTML = `<p class="library-empty">${t("library_empty")}</p>`;
    return;
  }

  sections.forEach(({ p, learnedIds }) => {
    const levels = buildLevels(p.verses, state.lang);
    const heading = document.createElement("h3");
    heading.className = "library-section";
    heading.textContent = `${p.emoji} ${parcoursText(p).title}`;
    list.appendChild(heading);

    learnedIds.forEach((id) => {
      const level = levels.find((l) => l.id === id);
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
  applyStaticI18n();
  renderHome();

  Billing.init(() => refreshActiveScreen());

  document.getElementById("btn-unlock-one").addEventListener("click", () => {
    handlePurchase(() => Billing.purchase(state.currentUnlockParcoursId), state.currentUnlockParcoursId);
  });

  document.getElementById("btn-unlock-all").addEventListener("click", () => {
    handlePurchase(() => Billing.purchaseAll(), state.currentUnlockParcoursId);
  });

  document.getElementById("btn-restore-top").addEventListener("click", handleRestore);
  document.getElementById("btn-restore-unlock").addEventListener("click", handleRestore);

  document.getElementById("btn-activate").addEventListener("click", () => {
    document.getElementById("activate-code-input").value = "";
    setActivateFeedback("", true);
    navTo("activate");
  });
  document.getElementById("btn-activate-link").addEventListener("click", () => {
    document.getElementById("activate-code-input").value = "";
    setActivateFeedback("", true);
    navTo("activate");
  });
  document.getElementById("btn-activate-submit").addEventListener("click", handleActivateSubmit);

  document.getElementById("lang-toggle").addEventListener("click", () => {
    state.lang = state.lang === "fr" ? "en" : "fr";
    saveProgress();
    applyStaticI18n();
    refreshActiveScreen();
  });

  document.getElementById("sound-toggle").addEventListener("click", () => {
    state.sound = !state.sound;
    SFX.setEnabled(state.sound);
    saveProgress();
    applyStaticI18n();
  });

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

  document.getElementById("btn-share").addEventListener("click", shareApp);

  document.getElementById("btn-quit-game").addEventListener("click", quitGame);
  document.getElementById("btn-quit-pause").addEventListener("click", quitGame);
  document.getElementById("btn-pause").addEventListener("click", pauseGame);
  document.getElementById("btn-resume-pause").addEventListener("click", resumeGame);

  document.getElementById("btn-resume").addEventListener("click", () => {
    const saved = loadInProgress();
    if (saved) resumeLevel(saved);
  });

  document.addEventListener("visibilitychange", () => {
    if (!document.hidden) return;
    const active = document.querySelector(".screen.active");
    if (active && active.id === "screen-game" && !levelFinished && !state.paused) {
      pauseGame();
    }
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
      showToast(t("toast_learned"));
    }
  });

  document.getElementById("btn-next-level").addEventListener("click", () => {
    const prog = getProgress(state.currentParcoursId);
    const nextId = state.currentLevelId + 1;
    if (nextId <= currentLevels().length && nextId <= prog.unlocked) {
      startLevel(nextId);
    } else {
      renderLevels();
      navTo("levels");
    }
  });

  navTo("home");
});
