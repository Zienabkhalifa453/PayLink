# Mock billers image. Build from the REPOSITORY ROOT:
#   docker build -f mocks/PayLink.MockBillers/Dockerfile -t paylink/mock-billers .

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /repo
COPY Directory.Build.props Directory.Packages.props .editorconfig ./
COPY mocks/ mocks/
RUN dotnet restore mocks/PayLink.MockBillers/PayLink.MockBillers.csproj
RUN dotnet publish mocks/PayLink.MockBillers/PayLink.MockBillers.csproj \
      --configuration Release --no-restore \
      --output /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
RUN apt-get update \
 && apt-get install -y --no-install-recommends curl \
 && rm -rf /var/lib/apt/lists/*
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
USER app
HEALTHCHECK --interval=10s --timeout=3s --start-period=10s --retries=5 \
  CMD curl --fail --silent http://localhost:8080/health/live || exit 1
ENTRYPOINT ["dotnet", "PayLink.MockBillers.dll"]
