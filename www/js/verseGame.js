/* Mini-jeu de mémorisation du verset : la phrase apparaît à trous,
   l'enfant doit cliquer les mots dans le bon ordre pour la compléter. */

const VerseGame = (function () {
  let currentLevel = null;
  let words = [];
  let hideSet = new Set();
  let blanks = []; // { index, word, filled }
  let bank = [];
  let els = {};

  function shuffle(arr) {
    const a = arr.slice();
    for (let i = a.length - 1; i > 0; i--) {
      const j = Math.floor(Math.random() * (i + 1));
      [a[i], a[j]] = [a[j], a[i]];
    }
    return a;
  }

  function pool() {
    const set = new Set();
    PARCOURS.forEach((p) => {
      p.verses.forEach((v) => {
        v[state.lang].text.split(" ").forEach((w) => set.add(w));
      });
    });
    return Array.from(set);
  }

  function buildBank() {
    const correct = blanks.map((b) => b.word);
    const others = pool().filter((w) => !correct.includes(w));
    const distractors = shuffle(others).slice(0, Math.min(3, others.length));
    bank = shuffle(correct.concat(distractors));
  }

  function start(level, domEls) {
    currentLevel = level;
    els = domEls;
    words = level.text.split(" ");
    hideSet = new Set(level.hide);
    blanks = level.hide.map((idx) => ({ index: idx, word: words[idx], filled: false }));
    buildBank();
    render();
  }

  function render() {
    els.title.textContent = currentLevel.ref;
    els.feedback.textContent = "";
    els.feedback.className = "verse-feedback";
    els.nextBtn.classList.add("hidden");
    els.checkBtn.classList.remove("hidden");

    els.sentence.innerHTML = "";
    words.forEach((w, i) => {
      if (hideSet.has(i)) {
        const span = document.createElement("span");
        span.className = "blank";
        span.dataset.index = i;
        span.textContent = "＿＿＿";
        els.sentence.appendChild(span);
      } else {
        els.sentence.appendChild(document.createTextNode(w));
      }
      els.sentence.appendChild(document.createTextNode(" "));
    });

    els.bank.innerHTML = "";
    bank.forEach((word, i) => {
      const btn = document.createElement("button");
      btn.className = "word-chip";
      btn.textContent = word;
      btn.dataset.word = word;
      btn.addEventListener("click", () => pickWord(word, btn));
      els.bank.appendChild(btn);
    });
  }

  function nextEmptyBlank() {
    return blanks.find((b) => !b.filled);
  }

  function pickWord(word, btnEl) {
    const target = nextEmptyBlank();
    if (!target) return;
    target.filled = true;
    target.chosen = word;

    const span = els.sentence.querySelector(`.blank[data-index="${target.index}"]`);
    span.textContent = word;
    span.classList.add("filled");
    btnEl.classList.add("used");
    SFX.wordPlaced();

    els.feedback.textContent = "";
  }

  function reset() {
    blanks.forEach((b) => {
      b.filled = false;
      b.chosen = null;
    });
    buildBank();
    render();
  }

  function check() {
    const allFilled = blanks.every((b) => b.filled);
    if (!allFilled) {
      els.feedback.textContent = t("verse_missing");
      els.feedback.className = "verse-feedback ko";
      return false;
    }
    const allCorrect = blanks.every((b) => b.chosen === b.word);
    if (allCorrect) {
      els.feedback.textContent = t("verse_success");
      els.feedback.className = "verse-feedback ok";
      els.checkBtn.classList.add("hidden");
      els.nextBtn.classList.remove("hidden");
      SFX.verseComplete();
      return true;
    } else {
      els.feedback.textContent = t("verse_retry");
      els.feedback.className = "verse-feedback ko";
      reset();
      return false;
    }
  }

  return { start, check, reset };
})();
