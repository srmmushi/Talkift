#!/bin/bash
echo "==================================="
echo "  Talkift Chat Server"
echo "==================================="
echo ""

cd "$(dirname "$0")"

mkdir -p data

echo "Usage: ./run.sh [--port 8080]"
echo ""

go run ./cmd/server/main.go "$@"
