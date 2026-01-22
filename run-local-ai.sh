#!/bin/bash

# Script to run Ollama and Claude Code locally

set -e

echo "Starting local AI services..."

# Check if Ollama is installed
if ! command -v ollama &> /dev/null; then
    echo "Error: Ollama is not installed."
    echo "Install it from: https://ollama.ai"
    exit 1
fi

# Check if Claude Code is installed
if ! command -v claude &> /dev/null; then
    echo "Error: Claude Code is not installed."
    echo "Install it with: npm install -g @anthropic-ai/claude-code"
    exit 1
fi

# Set Ollama context size to 32,000 tokens
export OLLAMA_NUM_CTX=32000

# Start Ollama server in a separate terminal if not already running
if ! pgrep -x "ollama" > /dev/null; then
    echo "Starting Ollama server in a new terminal (context size: $OLLAMA_NUM_CTX)..."
    osascript -e "tell application \"Terminal\"
        activate
        do script \"export OLLAMA_NUM_CTX=$OLLAMA_NUM_CTX && ollama serve\"
    end tell"
    sleep 3
    echo "Ollama server started in separate terminal"
else
    echo "Ollama server is already running"
fi

# Optional: Pull a default model if specified
MODEL="${1:-gpt-oss:20b}"
echo "Ensuring model '$MODEL' is available..."
ollama pull "$MODEL" 2>/dev/null || echo "Model '$MODEL' already available or pull in progress"

echo ""
echo "==================================="
echo "Local AI services are ready!"
echo "==================================="
echo ""
echo "Ollama API: http://localhost:11434"
echo "Model: $MODEL"
echo "Context: $OLLAMA_NUM_CTX tokens"
echo ""
echo "Starting Claude Code..."
echo ""

# Set Ollama variables for Claude Code
export ANTHROPIC_AUTH_TOKEN=ollama
export ANTHROPIC_BASE_URL=http://localhost:11434
# Run Claude Code
claude --model "$MODEL"

echo ""
echo "Note: Ollama server is still running in the separate terminal."
echo "Close that terminal window to stop Ollama."
