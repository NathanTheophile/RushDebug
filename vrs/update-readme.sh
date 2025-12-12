#!/usr/bin/env bash
set -euo pipefail

node vrs/apply-commit-emojis.js CHANGELOG.md

tmp="$(mktemp)"
awk '
  BEGIN{in_block=0}
  /^## \[/{
    if(in_block==1){exit}
    in_block=1
  }
  { if(in_block==1) print }
' CHANGELOG.md > "$tmp"

awk -v repl="$(sed -e 's/[\\/&]/\\&/g' "$tmp")" '
  BEGIN{in_block=0}
  /<!-- CHANGELOG:START -->/ { print; print repl; in_block=1; next }
  /<!-- CHANGELOG:END -->/   { in_block=0 }
  { if(!in_block) print }
' README.md > README.md.new

mv README.md.new README.md
rm -f "$tmp"
