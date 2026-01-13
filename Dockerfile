# Multi-stage build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ["SmsService.sln", "."]
COPY ["SmsService.Core/SmsService.Core.csproj", "SmsService.Core/"]
COPY ["SmsService.Infrastructure/SmsService.Infrastructure.csproj", "SmsService.Infrastructure/"]
COPY ["SmsService.Api/SmsService.Api.csproj", "SmsService.Api/"]
COPY ["SmsService.Tests/SmsService.Tests.csproj", "SmsService.Tests/"]

# Restore dependencies
RUN dotnet restore "SmsService.sln"

# Copy source code
COPY . .

# Build
RUN dotnet build "SmsService.sln" -c Release

# Run tests
RUN dotnet test "SmsService.Tests/SmsService.Tests.csproj" -c Release --no-build --logger "console;verbosity=minimal"

# Publish
RUN dotnet publish "SmsService.Api/SmsService.Api.csproj" -c Release -o /app/publish

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
  CMD curl -f http://localhost:5000/health || exit 1

EXPOSE 5000 5001

ENTRYPOINT ["dotnet", "SmsService.Api.dll"]
