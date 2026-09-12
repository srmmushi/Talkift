#!/bin/bash
echo "==================================="
echo "  Talkift Official Server"
echo "==================================="
echo ""

cd "$(dirname "$0")"

mkdir -p data/logs

echo "Starting official server..."
go run ./cmd/main.go
