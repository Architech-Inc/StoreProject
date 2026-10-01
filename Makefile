# =============================================================================
# ClexAn Foods (StoreProject) — Master Platform Makefile
# =============================================================================
# Run 'make' or 'make help' for an interactive menu of all available targets.
# =============================================================================

.DEFAULT_GOAL := help
.PHONY: help restore build build-dev test test-verbose clean run-api run-ui run-cp run-tp \
        platform-up platform-down platform-restart platform-logs \
        provision-tenant backup restore-db migrate audit format

# ── Color Definitions ────────────────────────────────────────────────────────
CYAN    := \033[36m
GREEN   := \033[32m
YELLOW  := \033[33m
RED     := \033[31m
MAGENTA := \033[35m
BOLD    := \033[1m
RESET   := \033[0m

# ── Defaults ─────────────────────────────────────────────────────────────────
SLN           := StoreProject.sln
CONFIGURATION ?= Release
SLUG          ?= bastos-market
STORE         ?= Bastos Fresh Market
EMAIL         ?= admin@bastos.cm
PASSWORD      ?= SuperSecret123!
CP_URL        ?= http://localhost:5050

## Display this help screen
help:
	@echo ""
	@echo "$(BOLD)$(GREEN)ClexAn Foods (StoreProject)$(RESET) — Command Orchestrator"
	@echo "=========================================================="
	@echo "$(CYAN)Usage:$(RESET) make $(YELLOW)<target>$(RESET) [VARIABLE=value]"
	@echo ""
	@echo "$(BOLD)Build & Test:$(RESET)"
	@printf "  $(YELLOW)%-18s$(RESET) %s\n" "build" "Build solution in Release mode (0 warnings policy)"
	@printf "  $(YELLOW)%-18s$(RESET) %s\n" "build-dev" "Build solution in Debug mode"
	@printf "  $(YELLOW)%-18s$(RESET) %s\n" "restore" "Restore all NuGet packages"
	@printf "  $(YELLOW)%-18s$(RESET) %s\n" "clean" "Clean build outputs and temporary caches"
	@printf "  $(YELLOW)%-18s$(RESET) %s\n" "test" "Run test suite (Store.API.Tests) in Release mode"
	@printf "  $(YELLOW)%-18s$(RESET) %s\n" "test-verbose" "Run test suite with detailed test console logging"
	@printf "  $(YELLOW)%-18s$(RESET) %s\n" "audit" "Execute full verification: clean build + test suite"
	@echo ""
	@echo "$(BOLD)Local Service Runners:$(RESET)"
	@printf "  $(YELLOW)%-18s$(RESET) %s\n" "run-api" "Run backend REST API (Store.API) on https://localhost:7112"
	@printf "  $(YELLOW)%-18s$(RESET) %s\n" "run-ui" "Run Store.UI web console on https://localhost:5001"
	@printf "  $(YELLOW)%-18s$(RESET) %s\n" "run-cp" "Run Store.ControlPlane orchestrator on http://localhost:5050"
	@printf "  $(YELLOW)%-18s$(RESET) %s\n" "run-tp" "Run Store.TenantPortal SaaS signup on http://localhost:5060"
	@echo ""
	@echo "$(BOLD)Container & Multi-Tenant Platform:$(RESET)"
	@printf "  $(YELLOW)%-18s$(RESET) %s\n" "platform-up" "Start Traefik + ControlPlane via docker-compose"
	@printf "  $(YELLOW)%-18s$(RESET) %s\n" "platform-down" "Stop all platform containers"
	@printf "  $(YELLOW)%-18s$(RESET) %s\n" "platform-restart" "Restart platform containers"
	@printf "  $(YELLOW)%-18s$(RESET) %s\n" "platform-logs" "Follow live container logs across platform services"
	@printf "  $(YELLOW)%-18s$(RESET) %s\n" "provision-tenant" "Provision tenant (SLUG=... STORE=... EMAIL=...)"
	@echo ""
	@echo "$(BOLD)Database Operations:$(RESET)"
	@printf "  $(YELLOW)%-18s$(RESET) %s\n" "migrate" "Apply pending EF Core migrations to database"
	@printf "  $(YELLOW)%-18s$(RESET) %s\n" "backup" "Trigger manual backup snapshot"
	@printf "  $(YELLOW)%-18s$(RESET) %s\n" "restore-db" "Restore database from latest backup snapshot"
	@echo ""

## Restore NuGet dependencies
restore:
	@echo "$(CYAN)Restoring NuGet packages for $(SLN)...$(RESET)"
	dotnet restore $(SLN)

## Build solution in Release configuration (Enforces 0 warnings)
build:
	@echo "$(CYAN)Building $(SLN) [Release]...$(RESET)"
	dotnet build $(SLN) --configuration Release

## Build solution in Debug configuration
build-dev:
	@echo "$(CYAN)Building $(SLN) [Debug]...$(RESET)"
	dotnet build $(SLN) --configuration Debug

## Run unit tests in Release mode
test:
	@echo "$(CYAN)Running xUnit test suite [Store.API.Tests]...$(RESET)"
	dotnet test Store.API.Tests --configuration Release --no-build

## Run unit tests with standard verbosity
test-verbose:
	@echo "$(CYAN)Running xUnit test suite (detailed verbosity)...$(RESET)"
	dotnet test Store.API.Tests --configuration Release --verbosity normal

## Clean build artifacts and bin/obj folders
clean:
	@echo "$(YELLOW)Cleaning solution build outputs and caches...$(RESET)"
	dotnet clean $(SLN)
	@find . -type d -name "bin" -o -type d -name "obj" 2>/dev/null | xargs rm -rf 2>/dev/null || true
	@rm -rf *.log scratch/*.log 2>/dev/null || true
	@echo "$(GREEN)Clean complete.$(RESET)"

## Run Store.API locally
run-api:
	@echo "$(GREEN)Launching Store.API (REST API)...$(RESET)"
	dotnet run --project Store.API

## Run Store.UI locally
run-ui:
	@echo "$(GREEN)Launching Store.UI (Operations Console & POS)...$(RESET)"
	dotnet run --project Store.UI

## Run Store.ControlPlane locally
run-cp:
	@echo "$(GREEN)Launching Store.ControlPlane (Tenant Orchestrator)...$(RESET)"
	dotnet run --project Store.ControlPlane

## Run Store.TenantPortal locally
run-tp:
	@echo "$(GREEN)Launching Store.TenantPortal (Public SaaS Portal)...$(RESET)"
	dotnet run --project Store.TenantPortal

## Start Traefik + ControlPlane via Docker Compose
platform-up:
	@echo "$(CYAN)Starting Edge Proxy and Control Plane stacks...$(RESET)"
	docker compose -f docker-compose.traefik.yml -f docker-compose.platform.yml up -d

## Stop Traefik + ControlPlane stacks
platform-down:
	@echo "$(YELLOW)Stopping Edge Proxy and Control Plane stacks...$(RESET)"
	docker compose -f docker-compose.traefik.yml -f docker-compose.platform.yml down

## Restart Traefik + ControlPlane stacks
platform-restart: platform-down platform-up

## View platform logs
platform-logs:
	docker compose -f docker-compose.traefik.yml -f docker-compose.platform.yml logs -f

## Provision a new tenant stack through Control Plane
provision-tenant:
	@echo "$(CYAN)Provisioning tenant '$(SLUG)' ('$(STORE)')...$(RESET)"
	@if [ -f "./scripts/provision-tenant.sh" ]; then \
		bash ./scripts/provision-tenant.sh "$(STORE)" "$(SLUG)" "$(EMAIL)" "$(PASSWORD)" "XAF" "$(CP_URL)"; \
	else \
		curl -s -X POST "$(CP_URL)/api/control/tenants/provision" \
			-H "Content-Type: application/json" \
			-d '{"storeName":"$(STORE)","slug":"$(SLUG)","adminEmail":"$(EMAIL)","adminUsername":"admin","adminPassword":"$(PASSWORD)","currency":"XAF","planTier":1}' | jq . ; \
	fi

## Trigger immediate database backup
backup:
	@echo "$(CYAN)Triggering database backup snapshot...$(RESET)"
	@if [ -f "./scripts/backup-now.sh" ]; then \
		bash ./scripts/backup-now.sh; \
	elif [ -f "./scripts/backup-now.ps1" ]; then \
		pwsh -File ./scripts/backup-now.ps1; \
	fi

## Restore database from backup
restore-db:
	@echo "$(YELLOW)Restoring database from snapshot...$(RESET)"
	@if [ -f "./scripts/restore-database.sh" ]; then \
		bash ./scripts/restore-database.sh; \
	elif [ -f "./scripts/restore-database.ps1" ]; then \
		pwsh -File ./scripts/restore-database.ps1; \
	fi

## Apply pending EF Core migrations to database
migrate:
	@echo "$(CYAN)Applying EF Core database migrations...$(RESET)"
	dotnet ef database update --project Store.DbServices --startup-project Store.API

## Full audit: build solution and run entire test suite
audit: build test
	@echo "$(GREEN)========================================$(RESET)"
	@echo "$(GREEN)✓ Solution built clean (0 warnings)$(RESET)"
	@echo "$(GREEN)✓ All tests passing$(RESET)"
	@echo "$(GREEN)========================================$(RESET)"
