FROM mcr.microsoft.com/dotnet/core/aspnet:2.2-stretch-slim AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/core/sdk:2.2-stretch AS build-data
WORKDIR /src
COPY ["BetsData/BetsData.csproj", "BetsData/"]
RUN dotnet restore "BetsData/BetsData.csproj"
COPY "BetsData/." "BetsData/"
WORKDIR "/src/BetsData"
RUN dotnet build "BetsData.csproj" -c Release -o /app

FROM build-data AS build-api
WORKDIR /src
COPY ["BetsAPI/BetsAPI.csproj", "BetsAPI/"]
RUN dotnet restore "BetsAPI/BetsAPI.csproj"
COPY "BetsAPI/." "BetsAPI/"
WORKDIR "/src/BetsAPI"
RUN dotnet build "BetsAPI.csproj" -c Release -o /app

FROM build-api AS publish-api
RUN dotnet publish "BetsAPI.csproj" -c Release -o /app

FROM base AS final-api
WORKDIR /app
COPY --from=publish-api /app .
ENTRYPOINT ["dotnet", "BetsAPI.dll"]

#FROM build-data AS build-worker
#WORKDIR /src
#COPY ["BetsWorker/BetsWorker.csproj", "BetsWorker/"]
#RUN dotnet restore "BetsWorker/BetsWorker.csproj"
#COPY "BetsWorker/." "BetsWorker/"
#WORKDIR "/src/BetsWorker"
#RUN dotnet build "BetsWorker.csproj" -c Release -o /app

#FROM build-worker AS publish-worker
#RUN dotnet publish "BetsWorker.csproj" -c Release -o /app

#from base AS final-worker
#WORKDIR /app
#COPY --from=publish-worker /app .
#ENTRYPOINT ["dotnet", "BetsWorker.dll"]