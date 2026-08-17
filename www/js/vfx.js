/* Petits effets visuels ponctuels (confettis), réservés aux vraies réussites
   (fin de niveau, verset mémorisé) — jamais sur une simple action anodine. */

const VFX = (function () {
  const COLORS = ["#ff8a5c", "#ff6f91", "#6c4ab6", "#ffc93c", "#4caf50", "#a672e0"];

  function confetti(count) {
    count = count || 26;
    for (let i = 0; i < count; i++) {
      const el = document.createElement("div");
      el.className = "confetti-piece";
      el.style.left = Math.random() * 100 + "vw";
      el.style.background = COLORS[Math.floor(Math.random() * COLORS.length)];
      const duration = 1.4 + Math.random() * 0.9;
      el.style.animationDuration = duration + "s";
      el.style.animationDelay = Math.random() * 0.25 + "s";
      document.body.appendChild(el);
      setTimeout(() => el.remove(), (duration + 0.5) * 1000);
    }
  }

  return { confetti };
})();
