#!/bin/bash

HTML_DIR="../www/"
OUTPUT_CSS="${HTML_DIR}/styles.css"
INPUT_CSS="${HTML_DIR}/input.css"

echo Installing dependencies
pnpm install -D tailwindcss
echo "Building Tailwind CSS..."
npx tailwindcss -i "$INPUT_CSS" -o "$OUTPUT_CSS" --watch

