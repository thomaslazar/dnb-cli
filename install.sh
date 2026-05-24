#!/bin/bash
set -euo pipefail

REPO="thomaslazar/dnb-cli"
INSTALL_DIR="${DNB_CLI_INSTALL_DIR:-$HOME/.local/bin}"
VERSION="${DNB_CLI_VERSION:-}"

OS="$(uname -s)"
case "$OS" in
  Linux)  OS_RID="linux" ;;
  Darwin) OS_RID="osx" ;;
  *)      echo "Error: Unsupported OS: $OS" >&2; exit 1 ;;
esac

ARCH="$(uname -m)"
case "$ARCH" in
  x86_64)  ARCH_RID="x64" ;;
  aarch64) ARCH_RID="arm64" ;;
  arm64)   ARCH_RID="arm64" ;;
  *)       echo "Error: Unsupported architecture: $ARCH" >&2; exit 1 ;;
esac

RID="${OS_RID}-${ARCH_RID}"

if [ -z "$VERSION" ]; then
  VERSION="$(curl -fsSL "https://api.github.com/repos/${REPO}/releases/latest" \
    | grep '"tag_name"' | sed -E 's/.*"([^"]+)".*/\1/')"
  if [ -z "$VERSION" ]; then
    echo "Error: Could not determine latest version" >&2
    exit 1
  fi
fi

echo "Installing dnb-cli ${VERSION} (${RID})..."

DOWNLOAD_URL="https://github.com/${REPO}/releases/download/${VERSION}/dnb-cli-${RID}"
mkdir -p "$INSTALL_DIR"
curl -fsSL "$DOWNLOAD_URL" -o "${INSTALL_DIR}/dnb-cli"
chmod +x "${INSTALL_DIR}/dnb-cli"

case ":${PATH}:" in
  *":${INSTALL_DIR}:"*) ;;
  *)
    echo ""
    echo "Warning: ${INSTALL_DIR} is not on your PATH." >&2
    echo "Add it to your shell profile:" >&2
    echo "  export PATH=\"${INSTALL_DIR}:\$PATH\"" >&2
    ;;
esac

"${INSTALL_DIR}/dnb-cli" --version
echo "dnb-cli installed to ${INSTALL_DIR}/dnb-cli"
