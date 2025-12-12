#!/usr/bin/env node
'use strict';

const { env, exit } = process;
const { applyCommitEmojis } = require('./release-notes-utils');

const FALLBACK_RELEASE_NOTES = 'Aucun changelog fourni.';
const DEFAULT_USERNAME = 'Rush Release';
const DEFAULT_AVATAR_URL = 'https://i.imgur.com/N85eXU5.png';
const DEFAULT_RELEASE_IMAGE_URL = 'https://i.imgur.com/j449uON.png';

const MARKDOWN_IMAGE_REGEX = /!\[[^\]]*\]\(\s*<?([^\s)]+[^)>]*)>?\s*\)/i;
const HTML_IMAGE_REGEX = /<img\s+[^>]*src=["']([^"']+)["'][^>]*>/i;

function toDirectImageUrl(url) {
  if (typeof url !== 'string') return undefined;
  const trimmed = url.trim();
  if (!trimmed) return undefined;
  try {
    const parsed = new URL(trimmed);
    const hostname = parsed.hostname.toLowerCase();
    if (hostname === 'imgur.com' || hostname === 'www.imgur.com') {
      const id = parsed.pathname.replace(/^\//, '');
      if (id) return `https://i.imgur.com/${id.includes('.') ? id : `${id}.png`}`;
    }
    return parsed.href;
  } catch { return trimmed; }
}

function extractImageFromNotes(notes) {
  if (typeof notes !== 'string' || notes.length === 0) return { cleanedNotes: notes, imageUrl: undefined };
  let match = notes.match(MARKDOWN_IMAGE_REGEX) || notes.match(HTML_IMAGE_REGEX);
  if (!match) return { cleanedNotes: notes, imageUrl: undefined };
  const [, rawUrl] = match;
  const directUrl = toDirectImageUrl(rawUrl);
  if (!directUrl) return { cleanedNotes: notes, imageUrl: undefined };
  const cleanedNotes = notes.replace(match[0], '').trim();
  return { cleanedNotes, imageUrl: directUrl };
}

function decodeReleaseNotes(rawNotes) {
  if (typeof rawNotes !== 'string' || rawNotes.length === 0) return rawNotes;
  const candidates = [rawNotes, rawNotes.replace(/\+/g, ' ')];
  for (const c of candidates) {
    try { return decodeURIComponent(c); } catch {}
    if (typeof unescape === 'function') { try { return unescape(c); } catch {} }
  }
  return rawNotes;
}

function normalizeNotes(rawNotes) {
  if (!rawNotes) return '';
  const trimmed = rawNotes.trim();
  if (!trimmed) return '';
  return trimmed.replace(/^#+\s+changelog\s*/i, '').trim();
}

function stripTrailingCommitReferences(notes) {
  if (!notes) return notes;
  const commitLinkRegex = /\s*\(\[[0-9a-f]{7,}\]\([^)]*\)\)\s*$/i;
  return notes.split(/\r?\n/).map((l) => l.replace(commitLinkRegex, '').trimEnd()).join('\n');
}

function truncate(text, limit) {
  if (typeof text !== 'string' || text.length <= limit) return text;
  return `${text.slice(0, limit - 1)}…`;
}

function buildReleaseUrl(version) {
  if (!version) return env.RELEASE_URL || undefined;
  if (env.RELEASE_URL) return env.RELEASE_URL;
  const repo = env.GITHUB_REPOSITORY;
  if (!repo) return undefined;
  const serverUrl = (env.GITHUB_SERVER_URL || 'https://github.com').replace(/\/$/, '');
  return `${serverUrl}/${repo}/releases/tag/${encodeURIComponent(version)}`;
}

async function main() {
  const webhookUrl = (env.DISCORD_CUSTOM_WEBHOOK_URL || env.DISCORD_WEBHOOK_URL || '').trim();
  if (!webhookUrl) { console.warn('[post-discord] Aucun webhook Discord configuré.'); return; }

  const version = env.RELEASE_VERSION || '';
  const releaseName = env.RELEASE_NAME || (version ? `📦 Release ${version}` : '📦 Nouvelle release');
  const releaseUrl = buildReleaseUrl(version);
  const rawNotes = env.RELEASE_NOTES || '';
  const decodedNotes = decodeReleaseNotes(rawNotes);
  const normalizedNotes = normalizeNotes(decodedNotes);
  const notesWithoutCommitRefs = stripTrailingCommitReferences(normalizedNotes);
  const { cleanedNotes, imageUrl } = extractImageFromNotes(notesWithoutCommitRefs);
  const notesWithEmoji = applyCommitEmojis(cleanedNotes);
  const embedDescription = truncate(notesWithEmoji || FALLBACK_RELEASE_NOTES, 4096);

  const payload = {
    username: env.DISCORD_BOT_USERNAME || DEFAULT_USERNAME,
    avatar_url: env.DISCORD_BOT_AVATAR_URL || DEFAULT_AVATAR_URL,
    embeds: [
      { title: releaseName, description: embedDescription, color: 0x5865f2, timestamp: new Date().toISOString() }
    ],
    allowed_mentions: { parse: [] }
  };
  if (releaseUrl) payload.embeds[0].url = releaseUrl;
  const repo = env.GITHUB_REPOSITORY;
  if (repo) {
    const parts = repo.split('/').filter(Boolean);
    payload.embeds[0].footer = { text: parts.length ? parts[parts.length - 1] : repo };
  }
  const releaseImageUrl = imageUrl || env.RELEASE_IMAGE_URL || DEFAULT_RELEASE_IMAGE_URL;
  if (releaseImageUrl) payload.embeds[0].image = { url: releaseImageUrl };

  try {
    const res = await fetch(webhookUrl, { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(payload) });
    if (!res.ok) { const body = await res.text(); console.error(`[post-discord] ${res.status}: ${body}`); exit(1); }
    console.log('[post-discord] Notification envoyée.');
  } catch (e) { console.error('[post-discord] Échec.'); console.error(e); exit(1); }
}

main().catch((e) => { console.error('[post-discord] Erreur.'); console.error(e); exit(1); });
