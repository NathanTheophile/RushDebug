'use strict';

const DEFAULT_EMOJI_MAP = new Map([
  ['features', '🔥'],
  ['feature', '🔥'],
  ['bug fixes', '🔧'],
  ['bug fix', '🔧'],
  ['additions', '➕'],
  ['addition', '➕'],
  ['performance', '🚀'],
  ['refactorings', '✂️'],
  ['refactoring', '✂️'],
  ['chores', '🧹'],
  ['chore', '🧹'],
  ['docs', '📚'],
  ['documentation', '📚'],
  ['tests', '✅'],
  ['test', '✅'],
  ['ci', '🤖'],
]);

function normalizeSection(section) {
  if (!section) return '';
  return section.toString().trim().toLowerCase().replace(/[:]/g, '');
}
function applyEmojiToLine(line, emoji) {
  if (!emoji) return line;
  const m = /^(\s*\*)\s*/.exec(line);
  if (!m) return line;
  const prefix = `${m[1]} `;
  let rest = line.slice(m[0].length).trimStart();
  while (rest.startsWith(emoji)) rest = rest.slice(emoji.length).trimStart();
  if (!rest) return `${prefix}${emoji}`;
  return `${prefix}${emoji} ${rest}`;
}
function applyEmojiToHeading(line, emoji) {
  if (!emoji) return line;
  const m = /^(###\s+)(.+?)\s*$/.exec(line);
  if (!m) return line;
  let title = m[2].trimStart();
  while (title.startsWith(emoji)) title = title.slice(emoji.length).trimStart();
  if (!title) return `${m[1]}${emoji}`;
  return `${m[1]}${emoji} ${title}`;
}
function applyCommitEmojis(notes, customMap) {
  if (!notes) return notes;
  const emojiMap = new Map(DEFAULT_EMOJI_MAP);
  if (customMap && typeof customMap === 'object') {
    for (const [k, v] of Object.entries(customMap)) if (v) emojiMap.set(k.toLowerCase(), v);
  }
  const lines = notes.split(/\r?\n/);
  let current = '';
  for (let i = 0; i < lines.length; i++) {
    const line = lines[i];
    const h = /^###\s+(.+?)\s*$/.exec(line);
    if (h) {
      current = normalizeSection(h[1]);
      const emojiHead = emojiMap.get(current);
      if (emojiHead) lines[i] = applyEmojiToHeading(line, emojiHead);
      continue;
    }
    if (!/^\s*\*/.test(line)) continue;
    const emoji = emojiMap.get(current);
    if (!emoji) continue;
    lines[i] = applyEmojiToLine(line, emoji);
  }
  return lines.join('\n');
}

module.exports = { applyCommitEmojis, DEFAULT_EMOJI_MAP };
