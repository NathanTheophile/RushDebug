#!/usr/bin/env node
'use strict';

const { readFileSync, writeFileSync } = require('node:fs');
const { applyCommitEmojis } = require('./release-notes-utils');

function processFile(path) {
  const original = readFileSync(path, 'utf8');
  const transformed = applyCommitEmojis(original);
  if (transformed !== original) writeFileSync(path, transformed);
}
function main() {
  const files = process.argv.slice(2);
  if (files.length === 0) {
    process.stderr.write('Usage: apply-commit-emojis.js <file> [...files]\n');
    process.exit(1);
  }
  for (const f of files) processFile(f);
}
main();
