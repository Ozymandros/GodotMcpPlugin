#!/usr/bin/env bash
set -euo pipefail

TASK="all"
SOLUTION=""
NO_RESTORE=0
FIX_FORMAT=0
CONFIGURATION="Release"

while [[ $# -gt 0 ]]; do
  case "$1" in
    --task|-t)
      TASK="${2:?missing value for $1}"
      shift 2
      ;;
    --solution|-s)
      SOLUTION="${2:?missing value for $1}"
      shift 2
      ;;
    --no-restore)
      NO_RESTORE=1
      shift
      ;;
    --fix-format)
      FIX_FORMAT=1
      shift
      ;;
    --help|-h)
      cat <<'EOF'
Usage: scripts/tasks.sh [options]

Options:
  -t, --task <name>       all|restore|lint|typecheck|format|build|test|verify
  -s, --solution <path>   Solution path (.slnx or .sln)
      --no-restore        Skip dotnet restore
      --fix-format        Apply formatting instead of verify-only
  -h, --help              Show this help
EOF
      exit 0
      ;;
    *)
      echo "Unknown argument: $1" >&2
      exit 2
      ;;
  esac
done

resolve_solution() {
  if [[ -n "$SOLUTION" && -f "$SOLUTION" ]]; then
    echo "$SOLUTION"
    return
  fi
  if [[ -f "Godot-MCP-SK-Plugin.slnx" ]]; then
    echo "Godot-MCP-SK-Plugin.slnx"
    return
  fi
  if [[ -f "GodotMcp.sln" ]]; then
    echo "GodotMcp.sln"
    return
  fi
  echo "No solution found. Use --solution <path>." >&2
  exit 1
}

run_step() {
  echo "==> $1"
  shift
  "$@"
}

has_local_tool_manifest() {
  [[ -f ".config/dotnet-tools.json" || -f "dotnet-tools.json" ]]
}

SOLUTION_PATH="$(resolve_solution)"
echo "Using solution: $SOLUTION_PATH"

do_restore() {
  if has_local_tool_manifest; then
    run_step "dotnet tool restore" dotnet tool restore
  else
    echo "==> dotnet tool restore (skipped: no local tool manifest)"
  fi

  if [[ "$NO_RESTORE" -eq 0 ]]; then
    run_step "dotnet restore $SOLUTION_PATH" dotnet restore "$SOLUTION_PATH"
  fi
}

do_lint() {
  run_step "dotnet format analyzers --verify-no-changes" \
    dotnet format analyzers "$SOLUTION_PATH" --verify-no-changes --no-restore
}

do_typecheck() {
  run_step "dotnet build -c $CONFIGURATION --no-restore -warnaserror" \
    dotnet build "$SOLUTION_PATH" -c "$CONFIGURATION" --no-restore -warnaserror
}

do_format() {
  if [[ "$FIX_FORMAT" -eq 1 ]]; then
    run_step "dotnet format (apply fixes)" \
      dotnet format "$SOLUTION_PATH" --no-restore
  else
    run_step "dotnet format --verify-no-changes" \
      dotnet format "$SOLUTION_PATH" --verify-no-changes --no-restore
  fi
}

do_build() {
  run_step "dotnet build -c $CONFIGURATION --no-restore" \
    dotnet build "$SOLUTION_PATH" -c "$CONFIGURATION" --no-restore
}

do_test() {
  run_step "dotnet test -c $CONFIGURATION --no-build" \
    dotnet test "$SOLUTION_PATH" -c "$CONFIGURATION" --no-build --nologo
}

do_verify() {
  do_format
  do_lint
  do_typecheck
  do_test
}

case "$TASK" in
  restore)
    do_restore
    ;;
  lint)
    [[ "$NO_RESTORE" -eq 0 ]] && do_restore
    do_lint
    ;;
  typecheck)
    [[ "$NO_RESTORE" -eq 0 ]] && do_restore
    do_typecheck
    ;;
  format)
    [[ "$NO_RESTORE" -eq 0 ]] && do_restore
    do_format
    ;;
  build)
    [[ "$NO_RESTORE" -eq 0 ]] && do_restore
    do_build
    ;;
  test)
    [[ "$NO_RESTORE" -eq 0 ]] && do_restore
    do_build
    do_test
    ;;
  verify)
    [[ "$NO_RESTORE" -eq 0 ]] && do_restore
    do_build
    do_verify
    ;;
  all)
    do_restore
    do_build
    do_verify
    ;;
  *)
    echo "Unknown task: $TASK" >&2
    exit 2
    ;;
esac

echo "Done."
