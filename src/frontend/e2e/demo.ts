import { test as base } from '@playwright/test';
import type { Page, TestInfo } from '@playwright/test';

// Modo demostración (E2E_DEMO=1): cursor visible, ondas en los clics, teclas en pantalla y un respiro al final de
// cada caso, para grabar vídeos que se entiendan. Registra los instantes de cada clic y tecla para el montaje.
export const DEMO = process.env.E2E_DEMO === '1';
const HOLD_AT_END_MILLISECONDS = 2000;
const EVENT_PREFIX = '__demo:';

interface DemoEvent {
  type: 'click' | 'key' | 'input' | 'navigation';
  at: number;
  x?: number;
  y?: number;
  key?: string;
  text?: string;
}

// Solo estilos por CSSOM y Web Animations: la CSP de la app (style-src 'self') bloquea un <style> inyectado.
const CURSOR_SCRIPT = `(() => {
  const POSITION_KEY = 'demo-cursor';
  const ACCENT = '#d9480f';
  const report = (event) => console.log('${EVENT_PREFIX}' + JSON.stringify({ ...event, at: Date.now() }));
  const saved = JSON.parse(sessionStorage.getItem(POSITION_KEY) || 'null') || { x: innerWidth / 2, y: innerHeight / 2 };
  const styled = (element, styles) => { Object.assign(element.style, styles); return element; };
  const mount = () => {
    if (document.getElementById('demo-overlay')) return;
    const cursor = styled(document.createElement('div'), {
      position: 'fixed', left: '0', top: '0', width: '0', height: '0', zIndex: '2147483647', pointerEvents: 'none',
      transition: 'transform 160ms cubic-bezier(.2,.8,.2,1)', transform: 'translate(' + saved.x + 'px,' + saved.y + 'px)',
    });
    cursor.id = 'demo-cursor';
    const halo = styled(document.createElement('div'), {
      position: 'absolute', left: '-18px', top: '-18px', width: '36px', height: '36px', borderRadius: '50%',
      background: 'rgba(217,72,15,.22)', boxShadow: '0 0 0 2px rgba(217,72,15,.6)',
    });
    const arrow = document.createElementNS('http://www.w3.org/2000/svg', 'svg');
    arrow.setAttribute('width', '26');
    arrow.setAttribute('height', '32');
    arrow.setAttribute('viewBox', '0 0 24 30');
    styled(arrow, { position: 'absolute', left: '-3px', top: '-2px', filter: 'drop-shadow(0 2px 3px rgba(0,0,0,.35))' });
    const path = document.createElementNS('http://www.w3.org/2000/svg', 'path');
    path.setAttribute('d', 'M2 2 L2 24 L8 18 L12 28 L16 26 L12 16 L20 16 Z');
    path.setAttribute('fill', '#fff');
    path.setAttribute('stroke', '#0b2a40');
    path.setAttribute('stroke-width', '2');
    path.setAttribute('stroke-linejoin', 'round');
    arrow.appendChild(path);
    cursor.append(halo, arrow);
    const keys = styled(document.createElement('div'), {
      position: 'fixed', left: '50%', bottom: '28px', transform: 'translateX(-50%)', zIndex: '2147483647',
      pointerEvents: 'none', padding: '10px 18px', borderRadius: '12px', background: 'rgba(13,48,72,.94)',
      color: '#fff', font: '600 18px Saira, system-ui, sans-serif', letterSpacing: '.02em',
      boxShadow: '0 8px 24px rgba(0,0,0,.25)', opacity: '0', transition: 'opacity 200ms', whiteSpace: 'nowrap',
      maxWidth: '80vw', overflow: 'hidden', textOverflow: 'ellipsis',
    });
    // Shadow root cerrado: los localizadores de Playwright no lo atraviesan y el overlay no interfiere con los tests.
    const host = document.createElement('div');
    host.id = 'demo-overlay';
    const shadow = host.attachShadow({ mode: 'closed' });
    shadow.append(cursor, keys);
    document.body.appendChild(host);
    let keysTimer = 0;
    const showKeys = (text) => {
      keys.textContent = text;
      keys.style.opacity = '1';
      clearTimeout(keysTimer);
      keysTimer = setTimeout(() => { keys.style.opacity = '0'; }, 1400);
    };
    addEventListener('mousemove', (event) => {
      cursor.style.transform = 'translate(' + event.clientX + 'px,' + event.clientY + 'px)';
      sessionStorage.setItem(POSITION_KEY, JSON.stringify({ x: event.clientX, y: event.clientY }));
    }, true);
    addEventListener('mousedown', (event) => {
      const ripple = styled(document.createElement('div'), {
        position: 'fixed', zIndex: '2147483646', pointerEvents: 'none', width: '14px', height: '14px',
        marginLeft: '-7px', marginTop: '-7px', borderRadius: '50%', border: '3px solid ' + ACCENT,
        left: event.clientX + 'px', top: event.clientY + 'px',
      });
      shadow.appendChild(ripple);
      ripple.animate([{ transform: 'scale(1)', opacity: 1 }, { transform: 'scale(6)', opacity: 0 }], { duration: 650, easing: 'ease-out' })
        .finished.then(() => ripple.remove());
      halo.animate([{ transform: 'scale(1)' }, { transform: 'scale(.7)' }, { transform: 'scale(1)' }], { duration: 260 });
      report({ type: 'click', x: event.clientX, y: event.clientY });
    }, true);
    addEventListener('keydown', (event) => {
      if (event.key.length === 1 || event.key === 'Shift') return;
      const combination = (event.shiftKey ? 'Shift + ' : '') + event.key;
      showKeys('⌨  ' + combination);
      report({ type: 'key', key: combination });
    }, true);
    addEventListener('input', (event) => {
      const target = event.target;
      if (!(target instanceof HTMLInputElement || target instanceof HTMLTextAreaElement)) return;
      const previousOutline = target.style.outline;
      target.style.outline = '3px solid rgba(217,72,15,.75)';
      target.style.outlineOffset = '2px';
      setTimeout(() => { target.style.outline = previousOutline; }, 900);
      const shown = target.type === 'password' ? '••••••••' : target.value;
      showKeys('⌨  ' + shown);
      const box = target.getBoundingClientRect();
      report({ type: 'input', x: box.left + box.width / 2, y: box.top + box.height / 2, text: shown });
    }, true);
    let lastUrl = '';
    const reportUrl = () => {
      const url = location.pathname + location.search;
      if (url !== lastUrl) {
        lastUrl = url;
        report({ type: 'navigation', text: url });
      }
    };
    reportUrl();
    setInterval(reportUrl, 150);
  };
  if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', mount);
  else mount();
})();`;

// Prepara una página para grabar: cursor visible y registro de eventos. Devuelve cómo adjuntar los tiempos.
export async function instrumentPage(page: Page, label: string): Promise<(testInfo: TestInfo) => Promise<void>> {
  const startedAt = Date.now();
  const events: DemoEvent[] = [];
  await page.addInitScript(CURSOR_SCRIPT);
  page.on('console', (message) => {
    const text = message.text();
    if (text.startsWith(EVENT_PREFIX)) {
      events.push(JSON.parse(text.slice(EVENT_PREFIX.length)) as DemoEvent);
    }
  });
  return async (testInfo) => {
    await testInfo.attach(`demo-timing-${label}`, {
      body: JSON.stringify({ label, startedAt, viewport: page.viewportSize(), events }),
      contentType: 'application/json',
    });
  };
}

export const test = base.extend({
  page: async ({ page }, provide, testInfo) => {
    if (!DEMO) {
      await provide(page);
      return;
    }
    const attachTiming = await instrumentPage(page, 'principal');
    await provide(page);
    if (!page.isClosed()) {
      await page.waitForTimeout(HOLD_AT_END_MILLISECONDS);
    }
    await attachTiming(testInfo);
  },
});

export { expect } from '@playwright/test';
