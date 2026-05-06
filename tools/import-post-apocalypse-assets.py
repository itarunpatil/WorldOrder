#!/usr/bin/env python3
"""Refresh the integrated PostApocalypse art folder from a locally owned zip.

This is a maintenance helper for private repositories. It extracts the supplied
asset pack into GameAssets/PostApocalypse so Windows and Android builds package
real art instead of relying on the procedural safety fallback.
"""
from __future__ import annotations

import argparse
import shutil
import sys
import zipfile
from pathlib import Path


def main() -> int:
    root = Path(__file__).resolve().parents[1]
    parser = argparse.ArgumentParser()
    parser.add_argument("asset_zip", type=Path, help="Path to PostApocalypse_AssetPack_v1.1.2.zip")
    parser.add_argument("--clean", action="store_true", help="Delete the existing integrated asset folder before extracting")
    args = parser.parse_args()
    if not args.asset_zip.exists():
        print(f"Missing asset zip: {args.asset_zip}", file=sys.stderr)
        return 2

    out = root / "GameAssets" / "PostApocalypse"
    if args.clean and out.exists():
        shutil.rmtree(out)
    out.mkdir(parents=True, exist_ok=True)

    with zipfile.ZipFile(args.asset_zip) as zf:
        zf.extractall(out)

    total = sum(1 for p in out.rglob("*") if p.is_file())
    print(f"Integrated {total} files into {out}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
