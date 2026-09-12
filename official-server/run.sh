#!/bin/bash
echo "==================================="
echo "  Talkift Official Server"
echo "==================================="
echo ""

cd "$(dirname "$0")"

mkdir -p data/user data/logs data/version data/config

echo "Usage: ./run.sh [--port 8081]"
echo ""

go run ./cmd/main.go "$@"
