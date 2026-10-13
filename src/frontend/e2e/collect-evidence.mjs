// Copia los vídeos de la última ejecución e2e a docs/evidencias/e2e con el código del caso en el nombre
// (S1-…, C1-…-2 cuando un caso abre dos navegadores) y escribe un índice en Markdown.
import { copyFileSync, existsSync, mkdirSync, readFileSync, readdirSync, rmSync, writeFileSync } from 'node:fs';
import { dirname, join, relative } from 'node:path';
import { fileURLToPath } from 'node:url';

const FRONTEND_DIRECTORY = join(dirname(fileURLToPath(import.meta.url)), '..');
const RESULTS_FILE = join(FRONTEND_DIRECTORY, 'e2e-results', 'results.json');
const EVIDENCE_DIRECTORY = join(FRONTEND_DIRECTORY, '..', '..', 'docs', 'evidencias', 'e2e');
const INDEX_FILE = join(EVIDENCE_DIRECTORY, 'README.md');
const CASE_TITLE_PATTERN = /^([A-Z]\d+) · (.+)$/;
const MAXIMUM_SLUG_LENGTH = 60;
// Mismo orden que la lista de casos: sesión, listados, diálogos, concurrencia, notificaciones, usuarios, errores,
// móvil y entrega.
const GROUP_ORDER = 'SLDCNUEMX';

function slugify(text) {
    return text
        .normalize('NFD')
        .replace(/\p{Diacritic}/gu, '')
        .toLowerCase()
        .replace(/[^a-z0-9]+/g, '-')
        .replace(/^-|-$/g, '')
        .slice(0, MAXIMUM_SLUG_LENGTH)
        .replace(/-$/, '');
}

function collectSpecs(suite, groupTitles = []) {
    const titles = suite.title && !suite.title.endsWith('.ts') ? [...groupTitles, suite.title] : groupTitles;
    const own = (suite.specs ?? []).map((spec) => ({ spec, group: titles[0] ?? '' }));
    return [...own, ...(suite.suites ?? []).flatMap((child) => collectSpecs(child, titles))];
}

if (!existsSync(RESULTS_FILE)) {
    console.error('No hay resultados: ejecute antes `npm run e2e`.');
    process.exit(1);
}

const report = JSON.parse(readFileSync(RESULTS_FILE, 'utf8'));
rmSync(EVIDENCE_DIRECTORY, { recursive: true, force: true });
mkdirSync(EVIDENCE_DIRECTORY, { recursive: true });

const rows = [];
for (const { spec, group } of report.suites.flatMap((suite) => collectSpecs(suite))) {
    const match = CASE_TITLE_PATTERN.exec(spec.title);
    const result = spec.tests[0]?.results.at(-1);
    if (!match || !result) {
        continue;
    }
    const [, caseId, description] = match;
    const videos = result.attachments.filter((attachment) => attachment.name === 'video' && attachment.path);
    const links = videos.map((video, index) => {
        const suffix = videos.length > 1 ? `-${index + 1}` : '';
        const fileName = `${caseId}-${slugify(description)}${suffix}.webm`;
        copyFileSync(video.path, join(EVIDENCE_DIRECTORY, fileName));
        return `[vídeo${suffix}](${fileName})`;
    });
    rows.push({ group, caseId, description, status: result.status === 'passed' ? '✅' : '❌', links });
}

rows.sort(
    (first, second) =>
        GROUP_ORDER.indexOf(first.caseId[0]) - GROUP_ORDER.indexOf(second.caseId[0]) ||
        Number(first.caseId.slice(1)) - Number(second.caseId.slice(1)),
);

const lines = [
    '# Evidencias e2e (Playwright)',
    '',
    'Vídeos de la última ejecución de `npm run e2e` contra el stack de Docker. Se regeneran con',
    '`npm run e2e:evidence` (ver `docs/evidencias.md`).',
    '',
    '| Grupo | Caso | Descripción | Resultado | Vídeo |',
    '|---|---|---|---|---|',
    ...rows.map(
        (row) => `| ${row.group} | ${row.caseId} | ${row.description} | ${row.status} | ${row.links.join(' · ')} |`,
    ),
    '',
];
writeFileSync(INDEX_FILE, lines.join('\n'));

const copied = readdirSync(EVIDENCE_DIRECTORY).filter((file) => file.endsWith('.webm')).length;
console.log(`${rows.length} casos, ${copied} vídeos en ${relative(FRONTEND_DIRECTORY, EVIDENCE_DIRECTORY)}`);
