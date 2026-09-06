#!/bin/bash
echo "==================================="
echo "  Talkift Login Server"
echo "==================================="
echo ""

cd "$(dirname "$0")"

mkdir -p data/logs

echo "Starting login server..."
go run ./cmd/main.go
