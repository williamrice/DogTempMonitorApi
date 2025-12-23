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

# Create logs directory
RUN mkdir -p /app/logs

EXPOSE 3000

COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "TempMonitor.dll"]
