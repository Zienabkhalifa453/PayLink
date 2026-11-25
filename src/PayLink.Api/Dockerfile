# PayLink API image. Build from the REPOSITORY ROOT:
#   docker build -f src/PayLink.Api/Dockerfile -t paylink/api .

# ---- Build stage: full SDK, compiles and publishes -------------------------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /repo

# Repository-wide build settings first (shared by every project).
COPY Directory.Build.props Directory.Packages.props .editorconfig ./
COPY src/ src/

RUN dotnet restore src/PayLink.Api/PayLink.Api.csproj
RUN dotnet publish src/PayLink.Api/PayLink.Api.csproj \
      --configuration Release --no-restore \
      --output /app/publish /p:UseAppHost=false

# ---- Runtime stage: small ASP.NET image, no SDK ----------------------------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

# curl is used only by the container HEALTHCHECK.
RUN apt-get update \
 && apt-get install -y --no-install-recommends curl \
 && rm -rf /var/lib/apt/lists/*

WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

# Run as the non-root user that ships with the .NET 8 images.
USER app

HEALTHCHECK --interval=10s --timeout=3s --start-period=15s --retries=5 \
  CMD curl --fail --silent http://localhost:8080/health/live || exit 1

ENTRYPOINT ["dotnet", "PayLink.Api.dll"]
