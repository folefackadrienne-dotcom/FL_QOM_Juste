/* Moteur de jeu "match-3" façon Candy Crush, à thème biblique.

   Chaque case de la grille est soit `null` (vide, en cours de chute), soit un
   objet { type, special } où `type` est l'index du symbole et `special` vaut
   null | "row" | "col" | "bomb" :
   - Aligner 4 pions identiques transforme l'un d'eux en pion spécial "ligne"
     ou "colonne" (selon l'orientation de l'alignement) : rassemblé dans un
     match plus tard, il casse toute sa ligne/colonne.
   - Aligner 5 pions (ou plus) crée un pion "bombe" : il casse un bloc de 3x3
     autour de lui. Ça libère plus d'espace d'un coup, comme demandé. */

const DEFAULT_TILE_SYMBOLS = ["✝️", "🕊️", "🐟", "⭐", "👑", "📖"];
const DEFAULT_SPECIAL_SYMBOLS = { row: "↔️", col: "↕️", bomb: "💥" };

class Board {
  constructor(container, options) {
    this.container = container;
    this.rows = options.rows || 8;
    this.cols = options.cols || 8;
    this.numTypes = options.tiles || 6;
    this.symbols = options.symbols || DEFAULT_TILE_SYMBOLS;
    this.specialSymbols = options.specialSymbols || DEFAULT_SPECIAL_SYMBOLS;
    this.onScore = options.onScore || function () {};
    this.onMove = options.onMove || function () {};
    this.onInvalid = options.onInvalid || function () {};
    this.locked = false;
    this.selected = null;
    this.grid = [];
    this.cellEls = [];
  }

  randomType() {
    return Math.floor(Math.random() * this.numTypes);
  }

  buildInitialGrid() {
    this.grid = [];
    for (let r = 0; r < this.rows; r++) {
      const row = [];
      for (let c = 0; c < this.cols; c++) {
        let type;
        do {
          type = this.randomType();
        } while (this.createsMatchAt(row, r, c, type));
        row.push({ type, special: null });
      }
      this.grid.push(row);
    }
  }

  // Vérifie si placer "type" en (r,c) créerait immédiatement un alignement,
  // en ne regardant que ce qui est déjà posé (gauche et haut).
  createsMatchAt(currentRowInProgress, r, c, type) {
    if (
      c >= 2 &&
      currentRowInProgress[c - 1].type === type &&
      currentRowInProgress[c - 2].type === type
    ) {
      return true;
    }
    if (r >= 2 && this.grid[r - 1][c].type === type && this.grid[r - 2][c].type === type) {
      return true;
    }
    return false;
  }

  init() {
    this.buildInitialGrid();
    this.renderAll();
  }

  renderAll() {
    this.container.innerHTML = "";
    this.container.style.gridTemplateColumns = `repeat(${this.cols}, 1fr)`;
    this.container.style.gridTemplateRows = `repeat(${this.rows}, 1fr)`;
    this.cellEls = [];
    for (let r = 0; r < this.rows; r++) {
      const rowEls = [];
      for (let c = 0; c < this.cols; c++) {
        const el = document.createElement("div");
        el.className = "tile";
        el.dataset.r = r;
        el.dataset.c = c;
        el.addEventListener("click", () => this.handleClick(r, c));
        this.container.appendChild(el);
        rowEls.push(el);
      }
      this.cellEls.push(rowEls);
    }
    for (let r = 0; r < this.rows; r++) {
      for (let c = 0; c < this.cols; c++) {
        this.paintCell(r, c);
      }
    }
  }

  handleClick(r, c) {
    if (this.locked) return;

    if (!this.selected) {
      this.selected = { r, c };
      this.cellEls[r][c].classList.add("selected");
      SFX.select();
      return;
    }

    const { r: sr, c: sc } = this.selected;
    if (sr === r && sc === c) {
      this.cellEls[r][c].classList.remove("selected");
      this.selected = null;
      return;
    }

    const isAdjacent = Math.abs(sr - r) + Math.abs(sc - c) === 1;
    this.cellEls[sr][sc].classList.remove("selected");

    if (!isAdjacent) {
      this.selected = { r, c };
      this.cellEls[r][c].classList.add("selected");
      return;
    }

    this.selected = null;
    this.trySwap(sr, sc, r, c);
  }

  async trySwap(r1, c1, r2, c2) {
    this.locked = true;
    this.swapValues(r1, c1, r2, c2);
    this.paintCell(r1, c1);
    this.paintCell(r2, c2);
    await this.wait(120);

    const matches = this.findMatches();
    if (matches.length === 0) {
      // échange invalide : on annule
      this.swapValues(r1, c1, r2, c2);
      this.paintCell(r1, c1);
      this.paintCell(r2, c2);
      this.flashInvalid(r1, c1, r2, c2);
      SFX.invalid();
      this.onInvalid();
      await this.wait(250);
      this.locked = false;
      return;
    }

    SFX.swap();
    this.onMove();
    await this.resolveMatches(1);
    this.locked = false;
  }

  swapValues(r1, c1, r2, c2) {
    const tmp = this.grid[r1][c1];
    this.grid[r1][c1] = this.grid[r2][c2];
    this.grid[r2][c2] = tmp;
  }

  // Affiche le symbole (+ badge spécial éventuel) de la case (r,c).
  paintCell(r, c) {
    const el = this.cellEls[r][c];
    const cell = this.grid[r][c];
    el.classList.remove("special-row", "special-col", "special-bomb");
    if (!cell) {
      el.innerHTML = "";
      return;
    }
    el.innerHTML = `<span class="tile-symbol">${this.symbols[cell.type]}</span>`;
    if (cell.special) {
      el.classList.add("special-" + cell.special);
      const badge = document.createElement("span");
      badge.className = "special-badge";
      badge.textContent = this.specialSymbols[cell.special];
      el.appendChild(badge);
    }
  }

  flashInvalid(r1, c1, r2, c2) {
    [this.cellEls[r1][c1], this.cellEls[r2][c2]].forEach((el) => {
      el.classList.add("invalid");
      setTimeout(() => el.classList.remove("invalid"), 300);
    });
  }

  spawnSparkles(tileEl) {
    const boardRect = this.container.getBoundingClientRect();
    const tileRect = tileEl.getBoundingClientRect();
    const cx = tileRect.left - boardRect.left + tileRect.width / 2;
    const cy = tileRect.top - boardRect.top + tileRect.height / 2;
    const count = 3;
    for (let i = 0; i < count; i++) {
      const s = document.createElement("span");
      s.className = "sparkle";
      s.textContent = "✨";
      const angle = (Math.PI * 2 * i) / count + (Math.random() * 0.8 - 0.4);
      const dist = 16 + Math.random() * 12;
      s.style.left = cx + "px";
      s.style.top = cy + "px";
      s.style.setProperty("--dx", `${Math.cos(angle) * dist}px`);
      s.style.setProperty("--dy", `${Math.sin(angle) * dist}px`);
      this.container.appendChild(s);
      setTimeout(() => s.remove(), 520);
    }
  }

  // Retourne la liste plate des cases alignées (>=3), tous alignements confondus.
  // Sert uniquement à savoir si un échange est valide.
  findMatches() {
    const matched = new Set();

    for (let r = 0; r < this.rows; r++) {
      let runStart = 0;
      for (let c = 1; c <= this.cols; c++) {
        const a = c < this.cols ? this.grid[r][c] : null;
        const b = this.grid[r][runStart];
        const same = a && b && a.type === b.type;
        if (!same) {
          if (c - runStart >= 3) {
            for (let k = runStart; k < c; k++) matched.add(`${r},${k}`);
          }
          runStart = c;
        }
      }
    }

    for (let c = 0; c < this.cols; c++) {
      let runStart = 0;
      for (let r = 1; r <= this.rows; r++) {
        const a = r < this.rows ? this.grid[r][c] : null;
        const b = this.grid[runStart][c];
        const same = a && b && a.type === b.type;
        if (!same) {
          if (r - runStart >= 3) {
            for (let k = runStart; k < r; k++) matched.add(`${k},${c}`);
          }
          runStart = r;
        }
      }
    }

    return Array.from(matched).map((s) => {
      const [r, c] = s.split(",").map(Number);
      return { r, c };
    });
  }

  // Comme findMatches, mais garde les alignements groupés (avec leur
  // orientation et longueur) pour savoir où créer un pion spécial.
  findMatchRuns() {
    const runs = [];

    for (let r = 0; r < this.rows; r++) {
      let runStart = 0;
      for (let c = 1; c <= this.cols; c++) {
        const a = c < this.cols ? this.grid[r][c] : null;
        const b = this.grid[r][runStart];
        const same = a && b && a.type === b.type;
        if (!same) {
          if (c - runStart >= 3) {
            const cells = [];
            for (let k = runStart; k < c; k++) cells.push({ r, c: k });
            runs.push({ cells, orientation: "row" });
          }
          runStart = c;
        }
      }
    }

    for (let c = 0; c < this.cols; c++) {
      let runStart = 0;
      for (let r = 1; r <= this.rows; r++) {
        const a = r < this.rows ? this.grid[r][c] : null;
        const b = this.grid[runStart][c];
        const same = a && b && a.type === b.type;
        if (!same) {
          if (r - runStart >= 3) {
            const cells = [];
            for (let k = runStart; k < r; k++) cells.push({ r: k, c });
            runs.push({ cells, orientation: "col" });
          }
          runStart = r;
        }
      }
    }

    return runs;
  }

  // Calcule le plan complet d'un match : quelles cases sont alignées, et
  // lesquelles doivent devenir des pions spéciaux plutôt que d'être retirées.
  computeMatchPlan() {
    const runs = this.findMatchRuns();
    if (runs.length === 0) return null;

    const matchedKeys = new Set();
    runs.forEach((run) => run.cells.forEach(({ r, c }) => matchedKeys.add(`${r},${c}`)));

    const specialCells = new Map(); // "r,c" -> { r, c, special }
    runs.forEach((run) => {
      if (run.cells.length < 4) return;
      const kind = run.cells.length >= 5 ? "bomb" : run.orientation === "row" ? "row" : "col";
      const pick = run.cells[Math.floor(run.cells.length / 2)];
      const key = `${pick.r},${pick.c}`;
      const existing = specialCells.get(key);
      if (!existing || (kind === "bomb" && existing.special !== "bomb")) {
        specialCells.set(key, { r: pick.r, c: pick.c, special: kind });
      }
    });

    return { matchedKeys, specialCells };
  }

  // Cases supplémentaires cassées par le déclenchement d'un pion spécial.
  specialBonusCells(r, c, special) {
    const extra = [];
    if (special === "row") {
      for (let cc = 0; cc < this.cols; cc++) extra.push({ r, c: cc });
    } else if (special === "col") {
      for (let rr = 0; rr < this.rows; rr++) extra.push({ r: rr, c });
    } else if (special === "bomb") {
      for (let rr = r - 1; rr <= r + 1; rr++) {
        for (let cc = c - 1; cc <= c + 1; cc++) {
          if (rr >= 0 && rr < this.rows && cc >= 0 && cc < this.cols) extra.push({ r: rr, c: cc });
        }
      }
    }
    return extra;
  }

  async resolveMatches(cascadeLevel) {
    const plan = this.computeMatchPlan();
    if (!plan) return;

    const { matchedKeys, specialCells } = plan;
    const toClear = new Set(matchedKeys);
    const triggered = new Set();

    // Si un pion déjà spécial (créé lors d'un tour précédent) se retrouve
    // dans ce match, on déclenche son effet et on étend la casse en chaîne.
    let frontier = Array.from(toClear);
    let guard = 0;
    while (frontier.length > 0 && guard < 60) {
      guard++;
      const next = [];
      frontier.forEach((key) => {
        if (specialCells.has(key) || triggered.has(key)) return;
        const [r, c] = key.split(",").map(Number);
        const cell = this.grid[r][c];
        if (!cell || !cell.special) return;
        triggered.add(key);
        this.specialBonusCells(r, c, cell.special).forEach(({ r: er, c: ec }) => {
          const ekey = `${er},${ec}`;
          if (!toClear.has(ekey)) {
            toClear.add(ekey);
            next.push(ekey);
          }
        });
      });
      frontier = next;
    }

    const clearCells = Array.from(toClear)
      .map((k) => {
        const [r, c] = k.split(",").map(Number);
        return { r, c };
      })
      .filter(({ r, c }) => !specialCells.has(`${r},${c}`));

    clearCells.forEach(({ r, c }) => {
      this.cellEls[r][c].classList.add("matched");
      this.spawnSparkles(this.cellEls[r][c]);
    });
    specialCells.forEach(({ r, c }) => {
      this.cellEls[r][c].classList.add("upgrading");
      this.spawnSparkles(this.cellEls[r][c]);
    });

    SFX.match();
    this.container.classList.remove("pulse");
    // force reflow so the animation restarts even on rapid consecutive matches
    void this.container.offsetWidth;
    this.container.classList.add("pulse");

    const typeCounts = {};
    clearCells.forEach(({ r, c }) => {
      const cell = this.grid[r][c];
      if (cell) typeCounts[cell.type] = (typeCounts[cell.type] || 0) + 1;
    });

    const gained = clearCells.length * 10 * cascadeLevel;
    this.onScore(gained, clearCells.length, cascadeLevel, typeCounts);

    await this.wait(220);

    clearCells.forEach(({ r, c }) => {
      this.grid[r][c] = null;
    });
    specialCells.forEach(({ r, c, special }) => {
      const existingType = this.grid[r][c] ? this.grid[r][c].type : this.randomType();
      this.grid[r][c] = { type: existingType, special };
      this.cellEls[r][c].classList.remove("upgrading");
      this.paintCell(r, c);
    });

    this.collapseAndRefill();
    SFX.drop();
    await this.wait(220);

    await this.resolveMatches(cascadeLevel + 1);
  }

  collapseAndRefill() {
    for (let c = 0; c < this.cols; c++) {
      let writeRow = this.rows - 1;
      for (let r = this.rows - 1; r >= 0; r--) {
        if (this.grid[r][c] !== null) {
          this.grid[writeRow][c] = this.grid[r][c];
          if (writeRow !== r) this.grid[r][c] = null;
          writeRow--;
        }
      }
      for (let r = writeRow; r >= 0; r--) {
        this.grid[r][c] = { type: this.randomType(), special: null };
      }
    }

    for (let r = 0; r < this.rows; r++) {
      for (let c = 0; c < this.cols; c++) {
        const el = this.cellEls[r][c];
        el.classList.remove("matched");
        el.classList.add("dropping");
        this.paintCell(r, c);
        setTimeout(() => el.classList.remove("dropping"), 260);
      }
    }
  }

  wait(ms) {
    return new Promise((resolve) => setTimeout(resolve, ms));
  }

  destroy() {
    this.container.innerHTML = "";
    this.selected = null;
    this.locked = false;
  }
}
