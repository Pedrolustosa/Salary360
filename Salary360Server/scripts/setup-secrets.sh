#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT="$SCRIPT_DIR/../src/Salary360.API"

random_token() {
  head -c 48 /dev/urandom | base64 | tr -d '\n=+/' | head -c "$1"
}

random_password() {
  echo "Aa1-$(random_token 16)"
}

read_password() {
  local label="$1" value
  read -r -s -p "$label (leave empty to generate): " value
  echo >&2
  if [ -z "$value" ]; then
    value="$(random_password)"
    echo "Generated password for $label: $value" >&2
  fi
  printf '%s' "$value"
}

dotnet user-secrets init --project "$PROJECT" >/dev/null

dotnet user-secrets set "Jwt:Secret" "$(random_token 64)" --project "$PROJECT" >/dev/null
dotnet user-secrets set "Seed:Admin:Password" "$(read_password 'Seed Admin password')" --project "$PROJECT" >/dev/null
dotnet user-secrets set "Seed:User:Password" "$(read_password 'Seed User password')" --project "$PROJECT" >/dev/null

echo "User secrets configured. Keys stored:"
dotnet user-secrets list --project "$PROJECT" | sed -E 's/=.*/= ********/'
echo "Generated passwords are shown only once above; delete salary360.db to recreate the seed users."
