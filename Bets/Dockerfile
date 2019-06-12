FROM mcr.microsoft.com/dotnet/core/aspnet:2.2-stretch-slim AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:2.2-stretch AS build-data
WORKDIR /src
COPY ["BetsData/BetsData.csproj", "BetsData/"]
RUN dotnet restore "BetsData/BetsData.csproj"
COPY "BetsData/." "BetsData/"
WORKDIR "/src/BetsData"
RUN dotnet build "BetsData.csproj" -c Release -o /app

FROM build-data AS build-worker-base
WORKDIR /src
COPY ["BetsWorkerBase/BetsWorkerBase.csproj", "BetsWorkerBase/"]
RUN dotnet restore "BetsWorkerBase/BetsWorkerBase.csproj"
COPY "BetsWorkerBase/." "BetsWorkerBase/"
WORKDIR "/src/BetsWorkerBase"
RUN dotnet build "BetsWorkerBase.csproj" -c Release -o /app

FROM build-data AS build-api
WORKDIR /src
COPY ["BetsAPI/BetsAPI.csproj", "BetsAPI/"]
RUN dotnet restore "BetsAPI/BetsAPI.csproj"
COPY "BetsAPI/." "BetsAPI/"
WORKDIR "/src/BetsAPI"
RUN dotnet build "BetsAPI.csproj" -c Release -o /app

FROM build-worker-base AS build-new-match-worker
WORKDIR /src
COPY ["BetsNewMatchWorker/BetsNewMatchWorker.csproj", "BetsNewMatchWorker/"]
RUN dotnet restore "BetsNewMatchWorker/BetsNewMatchWorker.csproj"
COPY "BetsNewMatchWorker/." "BetsNewMatchWorker/"
WORKDIR "/src/BetsNewMatchWorker"
RUN dotnet build "BetsNewMatchWorker.csproj" -c Release -o /app

FROM build-worker-base AS build-match-update-worker
WORKDIR /src
COPY ["BetsMatchUpdateWorker/BetsMatchUpdateWorker.csproj", "BetsMatchUpdateWorker/"]
RUN dotnet restore "BetsMatchUpdateWorker/BetsMatchUpdateWorker.csproj"
COPY "BetsMatchUpdateWorker/." "BetsMatchUpdateWorker/"
WORKDIR "/src/BetsMatchUpdateWorker"
RUN dotnet build "BetsMatchUpdateWorker.csproj" -c Release -o /app

FROM build-worker-base AS build-new-bet-worker
WORKDIR /src
COPY ["BetsNewBetWorker/BetsNewBetWorker.csproj", "BetsNewBetWorker/"]
RUN dotnet restore "BetsNewBetWorker/BetsNewBetWorker.csproj"
COPY "BetsNewBetWorker/." "BetsNewBetWorker/"
WORKDIR "/src/BetsNewBetWorker"
RUN dotnet build "BetsNewBetWorker.csproj" -c Release -o /app

FROM build-api AS publish-api
RUN dotnet publish "BetsAPI.csproj" -c Release -o /app

FROM build-new-match-worker AS publish-new-match-worker
RUN dotnet publish "BetsNewMatchWorker.csproj" -c Release -o /app

FROM build-match-update-worker AS publish-match-update-worker
RUN dotnet publish "BetsMatchUpdateWorker.csproj" -c Release -o /app

FROM build-new-bet-worker AS publish-new-bet-worker
RUN dotnet publish "BetsNewBetWorker.csproj" -c Release -o /app

FROM base AS final-api
WORKDIR /app
COPY --from=publish-api /app .
EXPOSE 80
ENTRYPOINT ["dotnet", "BetsAPI.dll"]

from base AS final-new-match-worker
WORKDIR /app
COPY --from=publish-new-match-worker /app .
ENTRYPOINT ["dotnet", "BetsNewMatchWorker.dll"]

from base AS final-match-update-worker
WORKDIR /app
COPY --from=publish-match-update-worker /app .
ENTRYPOINT ["dotnet", "BetsMatchUpdateWorker.dll"]

from base AS final-new-bet-worker
WORKDIR /app
COPY --from=publish-new-bet-worker /app .
ENTRYPOINT ["dotnet", "BetsNewBetWorker.dll"]