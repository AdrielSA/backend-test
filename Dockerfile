# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/BackendTest.Api/BackendTest.Api.csproj", "BackendTest.Api/"]
COPY ["src/BackendTest.Application/BackendTest.Application.csproj", "BackendTest.Application/"]
COPY ["src/BackendTest.Infrastructure/BackendTest.Infrastructure.csproj", "BackendTest.Infrastructure/"]
COPY ["src/BackendTest.Core/BackendTest.Core.csproj", "BackendTest.Core/"]

RUN dotnet restore "BackendTest.Api/BackendTest.Api.csproj"

COPY src/ .

WORKDIR "/src/BackendTest.Api"
RUN dotnet build "BackendTest.Api.csproj" -c Release -o /app/build

RUN dotnet publish "BackendTest.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

RUN useradd -m -u 1001 appuser && chown -R appuser /app
USER appuser

COPY --from=build --chown=appuser /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "BackendTest.Api.dll"]
