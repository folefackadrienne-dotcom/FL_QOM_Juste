#!/usr/bin/env python3
"""Regenerates the Faith Run hub (chapter-select screen) from the level roster:
the 3 hand-authored levels (Eden/Exodus/Canaan) plus the 17 generated ones."""
import os
import sys
sys.path.insert(0, os.path.dirname(__file__))
from build_levels import LEVELS

OUT = os.path.join(os.path.dirname(__file__), "..", "game", "faith-run-hub.html")

ICONS = {
  'growth': '<path d="M12 21V10"></path><path d="M12 10C12 10 6 10 6 5C6 5 12 4 12 10Z" fill="currentColor" fill-opacity=".25"></path><path d="M12 13C12 13 18 13 18 8C18 8 12 7 12 13Z" fill="currentColor" fill-opacity=".25"></path>',
  'fire': '<path d="M12 21C8 21 6 18 6 15C6 12 9 11 9 8C9 6 8 4 8 4C11 4 13 7 13 10C13 10 16 8 16 5C18 8 18 13 15 16C17 15 18 13 18 13C18 17 15 21 12 21Z" fill="currentColor" fill-opacity=".8" stroke="none"></path>',
  'structure': '<path d="M4 20V10H7V7H10V10H14V7H17V10H20V20H4Z" fill="currentColor" fill-opacity=".22"></path>',
  'water': '<path d="M3 10c2-2.5 4-2.5 6 0s4 2.5 6 0s4-2.5 6 0" fill="none"></path><path d="M3 15c2-2.5 4-2.5 6 0s4 2.5 6 0s4-2.5 6 0" fill="none"></path>',
  'mountain': '<path d="M3 19L9 7L12.5 13L16 6L21 19H3Z" fill="currentColor" fill-opacity=".22"></path>',
  'star': '<path d="M12 3l2.4 6.6H21l-5.5 4.2 2.2 6.4L12 16l-5.7 4.2 2.2-6.4L3 9.6h6.6L12 3Z" fill="currentColor" fill-opacity=".3"></path>',
  'cross': '<path d="M12 2v20M6 8h12" fill="none"></path>',
  'creature': '<path d="M12 20c-4.5 0-7.5-2.7-7.5-6.2c0-2 1.6-3.6 3.1-3.6c.6-2 2.2-3.7 4.4-3.7s3.8 1.7 4.4 3.7c1.5 0 3.1 1.6 3.1 3.6c0 3.5-3 6.2-7.5 6.2Z" fill="currentColor" fill-opacity=".22"></path>',
}

def station_html(n, slug, title, verse_tag, blurb, accent, motif, locked=False, href=None):
    icon_body = ICONS.get(motif, ICONS['star'])
    if locked:
        return f'''    <div class="station locked">
      <div class="node">
        <svg viewBox="0 0 24 24" fill="none" stroke="var(--locked)" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round">
          <rect x="6" y="11" width="12" height="9" rx="1.5"></rect>
          <path d="M9 11V8a3 3 0 0 1 6 0v3"></path>
        </svg>
      </div>
      <div class="card">
        <span class="eyebrow">Chapitre {n}</span>
        <h2>{title}</h2>
        <p>{blurb}</p>
        <div class="card-foot">
          <span class="locked-badge">
            <svg viewBox="0 0 24 24" width="12" height="12" fill="none" stroke="currentColor" stroke-width="2"><rect x="5" y="11" width="14" height="9" rx="1.5"></rect><path d="M8 11V8a4 4 0 0 1 8 0v3"></path></svg>
            Bientôt
          </span>
        </div>
      </div>
    </div>
'''
    return f'''    <div class="station" style="--accent:{accent};--accent-soft:{accent};">
      <div class="node">
        <svg viewBox="0 0 24 24" fill="none" stroke="{accent}" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round">
          {icon_body}
        </svg>
      </div>
      <div class="card">
        <span class="eyebrow">Chapitre {n}</span>
        <h2>{title}</h2>
        <p>{blurb}</p>
        <div class="card-foot">
          <span class="verse-tag">{verse_tag}</span>
          <a class="play-btn" href="{href}">Jouer →</a>
        </div>
      </div>
    </div>
'''

STATIONS = []
STATIONS.append(station_html(1, 'eden', 'Éden', 'Genèse 1–2 · Jean 15',
    "Recueille les versets semés dans le Jardin, évite les ronces et le serpent, et rejoins l'Arbre de Vie.",
    '#5fcf7f', 'growth', href='faith-run-eden.html'))
STATIONS.append(station_html(2, 'exodus', "L'Exode", 'Exode 3, 13, 14, 20',
    "Fuis l'Égypte, traverse le Nil et la mer Rouge, et suis la colonne de feu jusqu'au Sinaï.",
    '#ff9a4a', 'fire', href='faith-run-exodus.html'))
STATIONS.append(station_html(3, 'canaan', 'La Terre promise', 'Josué · Nombres 13',
    "Franchis le Jourdain, affronte lances et géants, et vois tomber les murailles de Jéricho.",
    '#a8d17a', 'structure', href='faith-run-canaan.html'))

for lv in LEVELS:
    verse_refs = ' · '.join(sorted(set(r.split(' ')[0] if not r[0].isdigit() else ' '.join(r.split(' ')[:2]) for r,_ in lv['verses']), key=lambda x: lv['verses'][0][0]))[:40]
    first_book = lv['verses'][0][0].rsplit(' ',1)[0]
    STATIONS.append(station_html(lv['n'], lv['slug'], lv['title'], first_book, lv['start_blurb'],
        lv['accent'], lv['motif'], href=f'faith-run-{lv["slug"]}.html'))

HEAD = '''<title>Faith Run</title>
<meta name="viewport" content="width=device-width, initial-scale=1" />

<style>
  :root{
    --font-display:Georgia,'Iowan Old Style','Palatino Linotype','Times New Roman',serif;
    --font-body:-apple-system,BlinkMacSystemFont,'Segoe UI',Roboto,'Helvetica Neue',Arial,sans-serif;
    --font-mono:ui-monospace,'SF Mono','Cascadia Mono','Consolas','Liberation Mono',monospace;
    --void:#17111a;
    --void-2:#211823;
    --panel:#2b202c;
    --gold:#f2c66b;
    --gold-soft:#ffdf9e;
    --parchment:#f6ecd8;
    --parchment-dim:#cdbfa8;
    --ink:#1c1410;
    --line:rgba(242,198,107,.22);
    --locked:#8a8090;
  }

  *{box-sizing:border-box;}
  html,body{margin:0;padding:0;}

  body{
    background:
      radial-gradient(ellipse 60% 40% at 15% -8%, rgba(95,207,127,.10) 0%, transparent 60%),
      radial-gradient(ellipse 55% 40% at 85% 18%, rgba(255,154,74,.10) 0%, transparent 60%),
      radial-gradient(ellipse 55% 40% at 50% 85%, rgba(168,209,122,.08) 0%, transparent 60%),
      var(--void);
    color:var(--parchment);
    font-family:var(--font-body);
    min-height:100vh;
    display:flex;
    justify-content:center;
    padding:clamp(28px,5vw,56px) 16px 60px;
  }

  .page{width:100%;max-width:680px;display:flex;flex-direction:column;align-items:center;}
  header{text-align:center;display:flex;flex-direction:column;align-items:center;gap:8px;margin-bottom:8px;}
  .kicker{font-weight:700;font-size:.72rem;letter-spacing:.28em;text-transform:uppercase;color:var(--gold-soft);opacity:.85;}
  h1{font-family:var(--font-display);font-weight:700;font-size:clamp(2.4rem,7vw,4rem);letter-spacing:.03em;margin:0;text-wrap:balance;color:var(--parchment);text-shadow:0 0 34px rgba(242,198,107,.3);}
  .tagline{font-size:.92rem;color:var(--parchment-dim);max-width:38ch;line-height:1.5;margin:2px 0 0;}
  .progress-line{font-family:var(--font-mono);font-size:.76rem;letter-spacing:.04em;color:var(--gold-soft);margin-top:6px;}

  .path{position:relative;width:100%;margin-top:34px;display:flex;flex-direction:column;gap:0;}
  .path::before{content:'';position:absolute;left:27px;top:14px;bottom:14px;width:2px;background:repeating-linear-gradient(to bottom, var(--line) 0 6px, transparent 6px 13px);}
  .station{position:relative;display:flex;align-items:flex-start;gap:20px;padding:18px 0;}
  .node{flex-shrink:0;width:56px;height:56px;border-radius:50%;display:flex;align-items:center;justify-content:center;background:linear-gradient(180deg,var(--panel),var(--void-2));border:1.5px solid var(--accent, var(--line));box-shadow:0 0 0 5px var(--void), 0 8px 20px -8px rgba(0,0,0,.6);z-index:1;}
  .node svg{width:26px;height:26px;}
  .station.locked .node{border-color:rgba(255,255,255,.12);opacity:.55;}
  .card{flex:1;background:linear-gradient(180deg,var(--panel),var(--void-2));border:1px solid var(--line);border-left:3px solid var(--accent, var(--line));border-radius:12px;padding:16px 18px;display:flex;flex-direction:column;gap:6px;box-shadow:0 12px 26px -18px rgba(0,0,0,.7);}
  .station.locked .card{border-left-color:rgba(255,255,255,.14);opacity:.62;}
  .eyebrow{font-family:var(--font-mono);font-size:.68rem;letter-spacing:.1em;text-transform:uppercase;color:var(--accent, var(--parchment-dim));}
  .card h2{font-family:var(--font-display);font-weight:600;font-size:1.25rem;margin:0;color:var(--parchment);}
  .card p{font-size:.86rem;line-height:1.5;color:var(--parchment-dim);margin:0;}
  .card-foot{display:flex;align-items:center;justify-content:space-between;gap:12px;margin-top:6px;flex-wrap:wrap;}
  .play-btn{font-family:var(--font-body);font-weight:700;font-size:.82rem;letter-spacing:.02em;color:var(--ink);background:linear-gradient(180deg, var(--accent-soft, var(--gold-soft)), var(--accent, var(--gold)));border:none;padding:8px 18px;border-radius:7px;cursor:pointer;text-decoration:none;display:inline-flex;align-items:center;gap:6px;box-shadow:0 8px 18px -8px rgba(0,0,0,.5);transition:transform .15s ease;}
  .play-btn:hover{transform:translateY(-1px);}
  .play-btn:focus-visible{outline:2px solid var(--parchment);outline-offset:3px;}
  .locked-badge{font-family:var(--font-mono);font-size:.7rem;letter-spacing:.06em;text-transform:uppercase;color:var(--locked);display:inline-flex;align-items:center;gap:6px;}
  .verse-tag{font-family:var(--font-mono);font-size:.72rem;color:var(--parchment-dim);opacity:.85;}
  footer{margin-top:36px;font-size:.72rem;color:var(--parchment-dim);opacity:.6;text-align:center;max-width:480px;}
  @media (max-width:520px){.station{gap:14px;}.node{width:46px;height:46px;}.node svg{width:22px;height:22px;}.path::before{left:22px;}}
</style>

<div class="page">
  <header>
    <span class="kicker">Faith Run</span>
    <h1>Choisis ton chapitre</h1>
    <p class="tagline">Une traversée de l'histoire biblique, chapitre après chapitre — versets à recueillir, épreuves à franchir, un enseignement de discipolat à l'arrivée de chacun.</p>
    <span class="progress-line">__PROGRESS__</span>
  </header>

  <div class="path">
__STATIONS__
  </div>

  <footer>Faith Run — prototype. Textes bibliques : version Louis Segond (1910, domaine public).</footer>
</div>
'''

total = len(STATIONS)
html = HEAD.replace('__PROGRESS__', f'{total} / {total} chapitres disponibles')
html = html.replace('__STATIONS__', '\n'.join(STATIONS))

with open(OUT, 'w', encoding='utf-8') as f:
    f.write(html)
print('wrote', OUT, len(html), 'bytes,', total, 'stations')
