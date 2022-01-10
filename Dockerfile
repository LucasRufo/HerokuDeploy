FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["HerokuDeploy.API/HerokuDeploy.API.csproj", "HerokuDeploy.API/"]
RUN dotnet restore "HerokuDeploy.API/HerokuDeploy.API.csproj"
COPY . .
WORKDIR "/src/HerokuDeploy.API"
RUN dotnet build "HerokuDeploy.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "HerokuDeploy.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
CMD ASPNETCORE_URLS=http://*:$PORT dotnet HerokuDeploy.API.dll

