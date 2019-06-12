FROM mcr.microsoft.com/dotnet/core/aspnet:2.2-stretch-slim AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:2.2-stretch AS build-data
WORKDIR /src
COPY ["MatchesData/MatchesData.csproj", "MatchesData/"]
RUN dotnet restore "MatchesData/MatchesData.csproj"
COPY "MatchesData/." "MatchesData/"
WORKDIR "/src/MatchesData"
RUN dotnet build "MatchesData.csproj" -c Release -o /app

FROM build-data AS build-api
WORKDIR /src
COPY ["MatchesAPI/MatchesAPI.csproj", "MatchesAPI/"]
RUN dotnet restore "MatchesAPI/MatchesAPI.csproj"
COPY "MatchesAPI/." "MatchesAPI/"
WORKDIR "/src/MatchesAPI"
RUN dotnet build "MatchesAPI.csproj" -c Release -o /app

FROM build-api AS publish-api
RUN dotnet publish "MatchesAPI.csproj" -c Release -o /app

FROM base AS final-api
WORKDIR /app
COPY --from=publish-api /app .
EXPOSE 80
ENTRYPOINT ["dotnet", "MatchesAPI.dll"]

FROM build-data AS build-worker
WORKDIR /src
COPY ["MatchesWorker/MatchesWorker.csproj", "MatchesWorker/"]
RUN dotnet restore "MatchesWorker/MatchesWorker.csproj"
COPY "MatchesWorker/." "MatchesWorker/"
WORKDIR "/src/MatchesWorker"
RUN dotnet build "MatchesWorker.csproj" -c Release -o /app

FROM build-worker AS publish-worker
RUN dotnet publish "MatchesWorker.csproj" -c Release -o /app

from base AS final-worker
WORKDIR /app
COPY --from=publish-worker /app .
ENTRYPOINT ["dotnet", "MatchesWorker.dll"]