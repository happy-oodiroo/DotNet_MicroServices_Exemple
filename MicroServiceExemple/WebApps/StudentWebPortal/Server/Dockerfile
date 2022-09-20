#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["WebApps/StudentWebPortal/Server/StudentWebPortal.Server.csproj", "WebApps/StudentWebPortal/Server/"]
COPY ["WebApps/StudentWebPortal/Client/StudentWebPortal.Client.csproj", "WebApps/StudentWebPortal/Client/"]
COPY ["APIsClients/StudentInfo.ApiClient/StudentInfo.ApiClient.csproj", "APIsClients/StudentInfo.ApiClient/"]
COPY ["APIsClients/ApisClients.Common/ApisClients.Common.csproj", "APIsClients/ApisClients.Common/"]
RUN dotnet restore "WebApps/StudentWebPortal/Server/StudentWebPortal.Server.csproj"
COPY . .
WORKDIR "/src/WebApps/StudentWebPortal/Server"
RUN dotnet build "StudentWebPortal.Server.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "StudentWebPortal.Server.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "StudentWebPortal.Server.dll"]