# =============================================================================
# Author: Vladyslav Zaiets | https://sarmkadan.com
# CTO & Software Architect
# =============================================================================

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS builder

WORKDIR /build

# Copy project files
COPY DotnetPluginEngine.slnx .
COPY src/ ./src/
COPY examples/ ./examples/

# Restore dependencies
RUN dotnet restore

# Build the project
RUN dotnet build -c Release --no-restore

# Run tests
RUN dotnet test -c Release --no-build --logger "console;verbosity=minimal"

# Create publish output
RUN dotnet publish src/PluginEngine/PluginEngine.csproj -c Release -o /publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/runtime:10.0

WORKDIR /app

# Copy published artifacts from builder
COPY --from=builder /publish ./

# Create non-root user for security
RUN useradd -m -u 1000 appuser && \
    chown -R appuser:appuser /app

# Create plugins directory
RUN mkdir -p /app/plugins && \
    chown -R appuser:appuser /app/plugins

# Create logs directory
RUN mkdir -p /app/logs && \
    chown -R appuser:appuser /app/logs

# Switch to non-root user
USER appuser

# Volume for plugins and logs
VOLUME ["/app/plugins", "/app/logs"]

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD dotnet --version || exit 1

# Default port
EXPOSE 5000

# Environment variables
ENV ASPNETCORE_ENVIRONMENT=Production
ENV DOTNET_RUNNING_IN_CONTAINER=true
ENV DOTNET_EnableDiagnostics=0

# Labels
LABEL maintainer="Vladyslav Zaiets <vladyslav.zaiets@sarmkadan.com>"
LABEL version="1.2.0"
LABEL description="Hot-reloadable plugin system for .NET"

# Entry point
ENTRYPOINT ["dotnet", "PluginEngine.dll"]
CMD ["--help"]
