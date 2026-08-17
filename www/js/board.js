/* Moteur de jeu "match-3" façon Candy Crush, à thème biblique. */

const DEFAULT_TILE_SYMBOLS = ["✝️", "🕊️", "🐟", "⭐", "👑", "📖"];

class Board {
  constructor(container, options) {
    this.container = container;
    this.rows = options.rows || 8;
    this.cols = options.cols || 8;
    this.numTypes = options.tiles || 6;
    this.symbols = options.symbols || DEFAULT_TILE_SYMBOLS;
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
        row.push(type);
      }
      this.grid.push(row);
    }
  }

  // Vérifie si placer "type" en (r,c) créerait immédiatement un alignement,
  // en ne regardant que ce qui est déjà posé (gauche et haut).
  createsMatchAt(currentRowInProgress, r, c, type) {
    if (c >= 2 && currentRowInProgress[c - 1] === type && currentRowInProgress[c - 2] === type) {
      return true;
    }
    if (r >= 2 && this.grid[r - 1][c] === type && this.grid[r - 2][c] === type) {
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
        el.textContent = this.symbols[this.grid[r][c]];
        el.addEventListener("click", () => this.handleClick(r, c));
        this.container.appendChild(el);
        rowEls.push(el);
      }
      this.cellEls.push(rowEls);
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
    this.updateCell(r1, c1);
    this.updateCell(r2, c2);
    await this.wait(120);

    const matches = this.findMatches();
    if (matches.length === 0) {
      // échange invalide : on annule
      this.swapValues(r1, c1, r2, c2);
      this.updateCell(r1, c1);
      this.updateCell(r2, c2);
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

  updateCell(r, c) {
    const el = this.cellEls[r][c];
    const type = this.grid[r][c];
    el.textContent = type === null ? "" : this.symbols[type];
  }

  flashInvalid(r1, c1, r2, c2) {
    [this.cellEls[r1][c1], this.cellEls[r2][c2]].forEach((el) => {
      el.classList.add("invalid");
      setTimeout(() => el.classList.remove("invalid"), 300);
    });
  }

  findMatches() {
    const matched = new Set();

    for (let r = 0; r < this.rows; r++) {
      let runStart = 0;
      for (let c = 1; c <= this.cols; c++) {
        const same = c < this.cols && this.grid[r][c] === this.grid[r][runStart] && this.grid[r][runStart] !== null;
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
        const same = r < this.rows && this.grid[r][c] === this.grid[runStart][c] && this.grid[runStart][c] !== null;
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

  async resolveMatches(cascadeLevel) {
    const matches = this.findMatches();
    if (matches.length === 0) return;

    matches.forEach(({ r, c }) => {
      this.cellEls[r][c].classList.add("matched");
    });
    SFX.match();

    const gained = matches.length * 10 * cascadeLevel;
    this.onScore(gained, matches.length, cascadeLevel);

    await this.wait(220);

    matches.forEach(({ r, c }) => {
      this.grid[r][c] = null;
    });

    this.collapseAndRefill();
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
        this.grid[r][c] = this.randomType();
      }
    }

    for (let r = 0; r < this.rows; r++) {
      for (let c = 0; c < this.cols; c++) {
        const el = this.cellEls[r][c];
        el.classList.remove("matched");
        el.classList.add("dropping");
        el.textContent = this.symbols[this.grid[r][c]];
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
