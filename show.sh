#!/bin/bash
find . -type f \( -name "*.cs" -o -name "*.tsx" -o -name "*.ts" \) | xargs grep -l "$1" 2>/dev/null | while read f; do
  echo "=== $f ==="
  cat "$f"
  echo ""
done
