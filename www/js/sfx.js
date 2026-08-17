/* Effets sonores discrets, générés directement (Web Audio API, aucun fichier).
   Volontairement sobres : même son de match à chaque fois (pas d'escalade
   de ton/volume sur les combos, contrairement aux jeux conçus pour être
   compulsifs). Un enfant peut couper le son à tout moment. */

const SFX = (function () {
  let ctx = null;
  let enabled = true;

  function ensureContext() {
    if (!ctx) {
      const AC = window.AudioContext || window.webkitAudioContext;
      if (!AC) return null;
      ctx = new AC();
    }
    if (ctx.state === "suspended") ctx.resume();
    return ctx;
  }

  function tone(freq, duration, opts) {
    opts = opts || {};
    if (!enabled) return;
    const c = ensureContext();
    if (!c) return;
    const t0 = c.currentTime + (opts.delay || 0);
    const osc = c.createOscillator();
    const gain = c.createGain();
    osc.type = opts.type || "sine";
    osc.frequency.setValueAtTime(freq, t0);
    if (opts.slideTo) {
      osc.frequency.exponentialRampToValueAtTime(opts.slideTo, t0 + duration);
    }
    const peak = opts.volume || 0.08;
    gain.gain.setValueAtTime(0.0001, t0);
    gain.gain.exponentialRampToValueAtTime(peak, t0 + 0.015);
    gain.gain.exponentialRampToValueAtTime(0.0001, t0 + duration);
    osc.connect(gain);
    gain.connect(c.destination);
    osc.start(t0);
    osc.stop(t0 + duration + 0.03);
  }

  function setEnabled(v) {
    enabled = v;
  }

  function isEnabled() {
    return enabled;
  }

  return {
    setEnabled,
    isEnabled,
    select: () => tone(720, 0.05, { volume: 0.045 }),
    swap: () => tone(480, 0.06, { volume: 0.05 }),
    invalid: () => tone(200, 0.11, { volume: 0.05, slideTo: 150 }),
    // Toujours le même son, quel que soit l'enchaînement de combos.
    match: () => tone(880, 0.12, { type: "triangle", volume: 0.07, slideTo: 1040 }),
    wordPlaced: () => tone(660, 0.05, { volume: 0.045 }),
    levelWin: () => {
      [523.25, 659.25, 783.99].forEach((f, i) => tone(f, 0.3, { volume: 0.06, delay: i * 0.07 }));
    },
    levelLose: () => tone(280, 0.22, { volume: 0.045, slideTo: 210 }),
    verseComplete: () => {
      [523.25, 659.25, 783.99, 1046.5].forEach((f, i) => tone(f, 0.26, { volume: 0.07, delay: i * 0.09 }));
    }
  };
})();
