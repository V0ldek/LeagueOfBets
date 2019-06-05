FROM mcr.microsoft.com/dotnet/core/aspnet:2.2-stretch-slim AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/core/sdk:2.2-stretch AS build
WORKDIR /src
COPY ["MatchesAPI.csproj", "MatchesAPI/"]
RUN dotnet restore "MatchesAPI/MatchesAPI.csproj"
COPY . "MatchesAPI/"
WORKDIR "/src/MatchesAPI"
RUN dotnet build "MatchesAPI.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "MatchesAPI.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "MatchesAPI.dll"]