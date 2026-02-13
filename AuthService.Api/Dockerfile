# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["AuthService.Api/AuthService.Api.csproj", "AuthService.Api/"]
COPY ["AuthService.Application/AuthService.Application.csproj", "AuthService.Application/"]
COPY ["AuthService.Domain/AuthService.Domain.csproj", "AuthService.Domain/"]
COPY ["AuthService.Infrastructure/AuthService.Infrastructure.csproj", "AuthService.Infrastructure/"]
RUN dotnet restore "AuthService.Api/AuthService.Api.csproj"

# Copy all source code and build
COPY . .
WORKDIR "/src/AuthService.Api"
RUN dotnet build "./AuthService.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./AuthService.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8082

# Copy published output
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "AuthService.Api.dll"]
