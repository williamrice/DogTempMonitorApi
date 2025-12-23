# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["TempMonitor.csproj", "./"]
RUN dotnet restore "TempMonitor.csproj"

# Copy everything else and build
COPY . .
RUN dotnet build "TempMonitor.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "TempMonitor.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Install curl for healthcheck
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Create logs directory
RUN mkdir -p /app/logs

EXPOSE 3000

COPY --from=publish /app/publish .

HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:3000/api/health || exit 1

ENTRYPOINT ["dotnet", "TempMonitor.dll"]
