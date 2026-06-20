# AgenticFlow

<p align="center">
  <img src="assets/logo.png" alt="AgenticFlow logo" width="128" height="128">
</p>

AgenticFlow is a MetaGPT-style multi-agent orchestration app for autonomous software development. It coordinates PM Research, Architecture, Planning, parallel Engineers, and QA with a native desktop dashboard for ticket management and live progress.

## What It Does

- Takes a PRD or ticket and executes it through a five-phase loop: PM Analysis -> Architecture -> Planning -> Parallel Execution -> QA Review.
- Supports multiple PM Research agents and Engineer workers, capped by `--max-workers`.
- Gives each Engineer its own role context, feature focus, git worktree, and branch.
- Runs QA before integrating a batch back into trunk.
- Provides a native Kanban dashboard with live updates.
- Saves per-ticket run snapshots so a ticket can be paused, resumed, or restarted after the app restarts.

## Requirements

- Python 3.10+
- Git
- At least one AI runner:
  - `kimi`
  - `claude`
  - `cursor-agent` (or `agent` on Windows)
  - `codex`
  - `copilot` or `gh` (GitHub Copilot CLI)
  - `OPENAI_API_KEY` for OpenAI-compatible API mode

## Quick Start: Download the Desktop App

Pre-built desktop apps are attached to every [GitHub Release](https://github.com/fabricciotc/agenticflow/releases). Download the bundle for your platform and open it:

- **macOS**: `AgenticFlow_aarch64.app.zip` or `AgenticFlow_x86_64.app.zip`. Right-click the app and choose **Open** the first time.
- **Windows**: the `.exe` installer from the `windows-x86_64` artifact.
- **Linux**: `.deb` or `.AppImage` from the `linux-x86_64` artifact.

The desktop app bundles the Python engine as a sidecar, so no Python or web setup is required.

## Install from Source

If you prefer to install the CLI and run the native app from a local clone:

```bash
git clone https://github.com/fabricciotc/agenticflow.git
cd agenticflow
./install.sh
```

On Windows, use PowerShell:

```powershell
git clone https://github.com/fabricciotc/agenticflow.git
cd agenticflow
.\install.ps1
```

The installer:

1. Creates a Python virtual environment for the backend in `dashboard/.venv`.
2. Installs `dashboard/requirements.txt`.
3. Creates `agenticflow` and `meta-ralph` commands in your PATH.
4. Reports which AI backends are available.

Restart your terminal or run `source ~/.zshrc` / `source ~/.bashrc` so the commands are available.

Then open the native app:

```bash
agenticflow start
```

If the native bundle has not been built yet, the launcher prints build instructions (see [Build the Desktop App Locally](#build-the-desktop-app-locally) below).

## Run from Source for Development

This is the fastest way to iterate on the code. It runs the Python backend and the Tauri desktop shell separately.

### 1. Clone the repository

```bash
git clone https://github.com/fabricciotc/agenticflow.git
cd agenticflow
```

### 2. Install Python dependencies

Using `venv`:

```bash
cd dashboard
python3 -m venv .venv
source .venv/bin/activate
pip install -r requirements.txt
```

Or using `uv` (recommended):

```bash
cd dashboard
uv venv .venv
source .venv/bin/activate
uv pip install -r requirements.txt
```

### 3. Start the backend server

```bash
cd dashboard
python server.py
```

By default the dashboard is available at `http://localhost:5050`. You can change the port:

```bash
python server.py --port 8080
```

### 4. Start the desktop shell in dev mode

In a second terminal, from the repository root:

```bash
npm install
npm run tauri dev
```

This compiles the Rust Tauri app and opens a native window pointing at `http://localhost:5050`. The window auto-reloads when the Rust or frontend code changes. Restart `python server.py` manually when you change Python code.

### 5. Link an AI assistant

The first time the dashboard connects to the local engine it detects which AI CLIs are installed and asks you to pick one. The choice is saved in the application data directory (`config.json`).

Supported options:

- `kimi` — Kimi Code CLI
- `claude` — Claude Code CLI
- `cursor-agent` / `agent` — Cursor agent CLI
- `codex` — Codex CLI
- `copilot` / `gh` — GitHub Copilot CLI
- `OPENAI_API_KEY` — OpenAI-compatible API backend

### 6. Create a ticket and run the factory

Create a ticket and use **Pick folder** to select the project directory with the native file picker. The full absolute path is used so the local engine can run `git`, builds, and AI CLIs on that folder.

Move the ticket to **Ready for Work** and the five-phase loop starts.

## Legacy CLI

The original `meta-ralph` CLI is still available as a legacy alias:

```bash
meta-ralph init      # create the app state directory
meta-ralph run       # start the multi-agent loop
meta-ralph dashboard # open the native desktop app
meta-ralph status    # show active worker state
meta-ralph stop      # stop active workers
```

## Backend Selection

By default, the backend tries available backends in this order:

```bash
AGENTICFLOW_BACKENDS="kimi claude cursor codex copilot openai_api"
```

Force a backend:

```bash
AGENTICFLOW_BACKEND=claude agenticflow start
AGENTICFLOW_BACKEND=codex agenticflow start
AGENTICFLOW_BACKEND=cursor agenticflow start
AGENTICFLOW_BACKEND=kimi agenticflow start
AGENTICFLOW_BACKEND=copilot agenticflow start
```

Use a custom runner:

```bash
META_RALPH_BACKEND=custom \
META_RALPH_RUNNER_COMMAND='my-agent --prompt-file "$META_RALPH_PROMPT_FILE"' \
agenticflow start
```

## Application Data Directory

AgenticFlow stores config, board, run-state, snapshots, worktrees, and logs in the OS-native application data directory:

- **macOS**: `~/Library/Application Support/AgenticFlow`
- **Linux**: `~/.local/share/AgenticFlow` (or `$XDG_DATA_HOME/AgenticFlow`)
- **Windows**: `%LOCALAPPDATA%/AgenticFlow`

Override this location with the `AGENTICFLOW_DATA_DIR` environment variable.

## Parallel Agents

The default maximum number of parallel agents is **10**. You can change it via:

- The `maxWorkers` field in `config.json`.
- The `AGENTICFLOW_MAX_WORKERS` environment variable.

```bash
AGENTICFLOW_MAX_WORKERS=5 agenticflow start
```

## Skill Recognition

Assistants that support skills should load `SKILL.md` when the user asks for:

- "meta ralph"
- "multi-agent loop"
- "parallel team"
- "MetaGPT workflow"
- "autonomous PRD execution"

If the assistant does not support native skill discovery, use CLI mode.

## Build the Desktop App Locally

The desktop app is a [Tauri v2](https://v2.tauri.app/) wrapper around the existing Python dashboard. In release mode it spawns the Python engine as a sidecar on port `5051` and loads `http://127.0.0.1:5051` in a native window.

### Prerequisites

- [Rust](https://rustup.rs/)
- [Node.js](https://nodejs.org/) 22+
- Python 3.10+ and `dashboard/requirements.txt` installed
- `npm install` run once from the repo root

### Build the release bundle

1. Build the Python sidecar (a single executable that Tauri bundles):

   ```bash
   scripts/build-sidecar.sh      # macOS / Linux
   scripts/build-sidecar.ps1     # Windows
   ```

2. Build and package the desktop app:

   ```bash
   npm install
   npx tauri build
   ```

3. Open the produced bundle:

   - **macOS**: `src-tauri/target/release/bundle/macos/AgenticFlow.app`
   - **Windows**: `src-tauri/target/release/bundle/nsis/AgenticFlow.exe`
   - **Linux**: `src-tauri/target/release/bundle/appimage/AgenticFlow.AppImage`

### Automatic releases

Pushing a tag that starts with `v` triggers `.github/workflows/release.yml`, which builds and uploads desktop bundles for macOS (ARM + x86_64), Windows, and Linux to a GitHub Release.

```bash
git tag v0.6.0
git push origin v0.6.0
```

## Project Layout

```text
agenticflow/
├── SKILL.md                    # Assistant-facing skill definition
├── README.md                   # This file
├── install.sh                  # Desktop/CLI installer
├── .github/workflows/          # CI/CD
│   └── release.yml             # Tauri release workflow
├── frontend/                   # Minimal Tauri frontend placeholder
│   └── index.html
├── scripts/
│   ├── agenticflow             # Main CLI launcher
│   ├── meta-ralph              # Legacy CLI launcher
│   ├── build-sidecar.sh        # PyInstaller sidecar builder
│   ├── build-sidecar.ps1
│   ├── create-worktree.sh
│   ├── dispatch-workers.sh
│   └── ...
├── src-tauri/                  # Tauri v2 desktop app
│   ├── src/main.rs             # Sidecar spawn / cleanup
│   ├── tauri.conf.json
│   └── icons/
└── dashboard/
    ├── server.py               # Flask + SocketIO backend
    ├── static/                 # Kanban UI
    └── requirements.txt
```

## License

MIT
