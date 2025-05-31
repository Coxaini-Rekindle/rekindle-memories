FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy NuGet configuration first
COPY ["NuGet.Config", "."]

# Copy solution file
COPY ["Rekindle.Memories.sln", "."]

# Copy all project files first for better Docker layer caching
COPY ["Rekindle.Memories.Api/Rekindle.Memories.Api.csproj", "Rekindle.Memories.Api/"]
COPY ["Rekindle.Memories.Application/Rekindle.Memories.Application.csproj", "Rekindle.Memories.Application/"]
COPY ["Rekindle.Memories.Contracts/Rekindle.Memories.Contracts.csproj", "Rekindle.Memories.Contracts/"]
COPY ["Rekindle.Memories.Domain/Rekindle.Memories.Domain.csproj", "Rekindle.Memories.Domain/"]
COPY ["Rekindle.Memories.Infrastructure/Rekindle.Memories.Infrastructure.csproj", "Rekindle.Memories.Infrastructure/"]

# Restore packages using the NuGet.Config
RUN dotnet restore "Rekindle.Memories.Api/Rekindle.Memories.Api.csproj" --configfile "./NuGet.Config"

# Copy everything else
COPY . .

# Build the main project
WORKDIR "/src/Rekindle.Memories.Api"
RUN dotnet build "./Rekindle.Memories.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Rekindle.Memories.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Rekindle.Memories.Api.dll"]
