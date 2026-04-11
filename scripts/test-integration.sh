#!/usr/bin/env bash
set -euo pipefail

dotnet test BookingApp.IntegrationTests/BookingApp.IntegrationTests.csproj -m:1 /nodeReuse:false
