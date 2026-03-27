#!/usr/bin/env python3
from __future__ import annotations

import argparse
import os
import sys
from pathlib import Path

from huggingface_hub import HfApi, snapshot_download
from huggingface_hub.errors import GatedRepoError, HfHubHTTPError


DEFAULT_REPO_ID = "CohereLabs/cohere-transcribe-03-2026"


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(
        description="Download the gated Cohere Transcribe model to a local directory."
    )
    parser.add_argument(
        "--repo-id",
        default=DEFAULT_REPO_ID,
        help=f"Hugging Face repo id. Default: {DEFAULT_REPO_ID}",
    )
    parser.add_argument(
        "--local-dir",
        default=str(Path("models") / "cohere-transcribe-03-2026"),
        help="Directory where the model snapshot will be stored.",
    )
    parser.add_argument(
        "--token",
        default=os.environ.get("HF_TOKEN") or os.environ.get("HUGGINGFACE_HUB_TOKEN"),
        help="Hugging Face token. If omitted, HF_TOKEN or HUGGINGFACE_HUB_TOKEN is used.",
    )
    parser.add_argument(
        "--revision",
        default=None,
        help="Optional revision, tag, or commit SHA.",
    )
    parser.add_argument(
        "--force",
        action="store_true",
        help="Force a fresh snapshot check even if config.json already exists locally.",
    )
    return parser.parse_args()


def require_token(token: str | None) -> str:
    if token:
        return token

    print(
        "Missing Hugging Face token. Set HF_TOKEN or pass --token.\n"
        "You also need approved access to the gated repo:\n"
        f"https://huggingface.co/{DEFAULT_REPO_ID}",
        file=sys.stderr,
    )
    raise SystemExit(2)


def verify_access(repo_id: str, token: str, revision: str | None) -> None:
    api = HfApi(token=token)

    try:
        api.model_info(repo_id=repo_id, revision=revision, token=token)
    except GatedRepoError as exc:
        print(
            f"Access denied for gated repo {repo_id}.\n"
            "Make sure the account behind this token has been approved and logged in.\n"
            f"Repo URL: https://huggingface.co/{repo_id}\n"
            f"Original error: {exc}",
            file=sys.stderr,
        )
        raise SystemExit(3) from exc
    except HfHubHTTPError as exc:
        print(f"Failed to verify repo access: {exc}", file=sys.stderr)
        raise SystemExit(4) from exc


def should_skip_download(local_dir: Path, force: bool) -> bool:
    return not force and (local_dir / "config.json").exists()


def main() -> int:
    args = parse_args()
    token = require_token(args.token)
    local_dir = Path(args.local_dir).expanduser().resolve()

    if should_skip_download(local_dir, args.force):
        print(f"Model already present at {local_dir}")
        print("Use --force to re-check against Hugging Face.")
        return 0

    local_dir.mkdir(parents=True, exist_ok=True)
    verify_access(args.repo_id, token, args.revision)

    try:
        snapshot_path = snapshot_download(
            repo_id=args.repo_id,
            revision=args.revision,
            local_dir=str(local_dir),
            local_dir_use_symlinks=False,
            resume_download=True,
            token=token,
        )
    except GatedRepoError as exc:
        print(
            f"Gated repo access failed while downloading {args.repo_id}.\n"
            "The token is valid syntactically but the account still lacks repo access.\n"
            f"Original error: {exc}",
            file=sys.stderr,
        )
        return 5
    except HfHubHTTPError as exc:
        print(f"Download failed: {exc}", file=sys.stderr)
        return 6

    print(f"Downloaded model snapshot to: {snapshot_path}")
    print(f"Use this local path in your app: {local_dir}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
