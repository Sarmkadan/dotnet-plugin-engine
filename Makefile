.PHONY: help build test clean restore format lint pack publish docker-build docker-run docs

# Default target
.DEFAULT_GOAL := help

# Variables
DOTNET := dotnet
CONFIG := Release
OUTPUT_DIR := ./build
PACK_OUTPUT := ./nupkg
VERSION := 1.2.0

help: ## Display this help message
	@echo "dotnet-plugin-engine - Build and development commands"
	@echo ""
	@grep -E '^[a-zA-Z_-]+:.*?## .*$$' $(MAKEFILE_LIST) | sort | awk 'BEGIN {FS = ":.*?## "}; {printf "\033[36m%-15s\033[0m %s\n", $$1, $$2}'

clean: ## Remove build artifacts and output directories
	@echo "🧹 Cleaning build artifacts..."
	@$(DOTNET) clean -c $(CONFIG) || true
	@rm -rf $(OUTPUT_DIR) $(PACK_OUTPUT) bin obj coverage
	@echo "✓ Clean complete"

restore: ## Restore NuGet packages
	@echo "📦 Restoring packages..."
	@$(DOTNET) restore
	@echo "✓ Restore complete"

build: restore ## Build the project
	@echo "🔨 Building project ($(CONFIG))..."
	@$(DOTNET) build -c $(CONFIG) --no-restore
	@echo "✓ Build complete"

build-debug: restore ## Build in Debug configuration
	@echo "🔨 Building in Debug..."
	@$(DOTNET) build -c Debug --no-restore
	@echo "✓ Debug build complete"

test: build ## Run unit tests
	@echo "🧪 Running tests..."
	@$(DOTNET) test -c $(CONFIG) --no-build --verbosity normal
	@echo "✓ Tests passed"

test-coverage: ## Run tests with code coverage
	@echo "📊 Running tests with coverage..."
	@$(DOTNET) test -c $(CONFIG) /p:CollectCoverage=true /p:CoverageFormat=opencover
	@echo "✓ Coverage report generated"

format: ## Format code using dotnet format
	@echo "🎨 Formatting code..."
	@$(DOTNET) format
	@echo "✓ Format complete"

format-check: ## Check if code is properly formatted
	@echo "🎨 Checking code format..."
	@$(DOTNET) format --verify-no-changes --verbosity diagnostic
	@echo "✓ Code is properly formatted"

lint: format-check ## Run code linting
	@echo "🔍 Linting code..."
	@$(DOTNET) build -c $(CONFIG) /p:TreatWarningsAsErrors=true --no-restore
	@echo "✓ No linting issues"

pack: build ## Create NuGet package
	@echo "📦 Creating NuGet package..."
	@$(DOTNET) pack -c $(CONFIG) -o $(PACK_OUTPUT)
	@echo "✓ Package created: $(PACK_OUTPUT)"

pack-version: build ## Create NuGet package with specific version
	@echo "📦 Creating NuGet package v$(VERSION)..."
	@$(DOTNET) pack -c $(CONFIG) -o $(PACK_OUTPUT) /p:PackageVersion=$(VERSION)
	@echo "✓ Package created: $(PACK_OUTPUT)"

publish: ## Publish to NuGet (requires NUGET_API_KEY environment variable)
	@echo "🚀 Publishing to NuGet..."
	@if [ -z "$$NUGET_API_KEY" ]; then echo "❌ NUGET_API_KEY not set"; exit 1; fi
	@$(DOTNET) nuget push $(PACK_OUTPUT)/*.nupkg --api-key $$NUGET_API_KEY --source https://api.nuget.org/v3/index.json
	@echo "✓ Published to NuGet"

docs: ## Generate documentation (if docfx installed)
	@echo "📚 Generating documentation..."
	@if command -v docfx &> /dev/null; then docfx docfx.json; else echo "⚠ docfx not found, skipping"; fi
	@echo "✓ Documentation generated"

watch: ## Watch for file changes and rebuild
	@echo "👀 Watching for changes..."
	@$(DOTNET) watch -p src/PluginEngine build -c Debug

ci: clean restore build format-check test lint ## Full CI pipeline
	@echo "✅ CI pipeline passed"

docker-build: ## Build Docker image
	@echo "🐳 Building Docker image..."
	@docker build -t dotnet-plugin-engine:$(VERSION) .
	@docker tag dotnet-plugin-engine:$(VERSION) dotnet-plugin-engine:latest
	@echo "✓ Docker image built"

docker-run: docker-build ## Run Docker container
	@echo "🐳 Running Docker container..."
	@docker run -it --name plugin-engine-demo \
		-p 5000:5000 \
		-v $$PWD/plugins:/app/plugins \
		-v $$PWD/logs:/app/logs \
		dotnet-plugin-engine:latest
	@echo "✓ Container running"

docker-clean: ## Remove Docker image and container
	@echo "🧹 Cleaning Docker..."
	@docker rm -f plugin-engine-demo || true
	@docker rmi dotnet-plugin-engine:latest || true
	@echo "✓ Docker cleanup complete"

examples: build ## Run examples
	@echo "▶️ Running examples..."
	@echo "Available examples:"
	@echo "  - BasicPluginHost"
	@echo "  - HotReloadExample"
	@echo "  - DependencyResolutionExample"
	@echo "  - ErrorHandlingExample"

example-basic: build ## Run BasicPluginHost example
	@echo "▶️ Running BasicPluginHost..."
	@$(DOTNET) run --project examples --args "BasicPluginHost"

bench: build ## Run benchmarks (if BenchmarkDotNet installed)
	@echo "⚡ Running benchmarks..."
	@$(DOTNET) run -c Release --project benchmarks
	@echo "✓ Benchmarks complete"

info: ## Display project information
	@echo "dotnet-plugin-engine v$(VERSION)"
	@echo ""
	@echo "🔧 Build Environment:"
	@$(DOTNET) --version
	@$(DOTNET) --info | grep "OS Platform"
	@echo ""
	@echo "📦 Project Structure:"
	@ls -la src/PluginEngine/PluginEngine.csproj
	@echo ""
	@echo "📚 Documentation:"
	@echo "  - README.md"
	@echo "  - docs/getting-started.md"
	@echo "  - docs/architecture.md"
	@echo "  - docs/api-reference.md"
	@echo "  - docs/deployment.md"
	@echo "  - docs/faq.md"

# Development targets
dev: ## Set up development environment
	@echo "🛠️ Setting up development environment..."
	@$(DOTNET) tool restore
	@echo "✓ Development environment ready"

version: ## Display current version
	@echo "Version: $(VERSION)"

# Advanced targets
analyze: build ## Run static code analysis
	@echo "🔍 Running static analysis..."
	@$(DOTNET) build -c $(CONFIG) /p:EnforceCodeStyleInBuild=true

security-scan: ## Scan for security vulnerabilities
	@echo "🔒 Scanning for vulnerabilities..."
	@$(DOTNET) list package --vulnerable
	@echo "✓ Security scan complete"

update-packages: ## Update NuGet packages to latest versions
	@echo "📦 Updating packages..."
	@$(DOTNET) list package --outdated
	@echo "Run: dotnet package update to apply updates"

benchmark-report: bench ## Generate benchmark report
	@echo "📊 Benchmark report generated in BenchmarkDotNet.Artifacts/"

# Cleanup targets
distclean: clean docker-clean ## Deep clean (removes all generated files)
	@echo "🧹 Deep clean complete"

# Installation targets
install-tools: ## Install required dotnet tools
	@echo "🛠️ Installing dotnet tools..."
	@$(DOTNET) tool install -g dotnet-format
	@$(DOTNET) tool install -g dotnet-coverage
	@$(DOTNET) tool install -g dotnet-outdated-tool
	@echo "✓ Tools installed"

# Release targets
release: ci pack-version ## Create a release build
	@echo "🎉 Release build complete"
	@echo "Package location: $(PACK_OUTPUT)"

pre-commit: format lint test ## Run pre-commit checks
	@echo "✅ Pre-commit checks passed"
