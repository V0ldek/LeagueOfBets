FROM mcr.microsoft.com/dotnet/core/aspnet:2.2-stretch-slim AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/core/sdk:2.2-stretch AS build
WORKDIR /src
COPY ["UsersAPI.csproj", "UsersAPI/"]
RUN dotnet restore "UsersAPI/UsersAPI.csproj"
COPY . "UsersAPI/"
WORKDIR "/src/UsersAPI"
RUN dotnet build "UsersAPI.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "UsersAPI.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "UsersAPI.dll"]