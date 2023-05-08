#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["src/AIdentities.UI/AIdentities.UI.csproj", "src/AIdentities.UI/"]
COPY ["src/AIdentities.Shared/AIdentities.Shared.csproj", "src/AIdentities.Shared/"]
COPY ["src/Plugins/AIdentities.Chat/AIdentities.Chat.csproj", "src/Plugins/AIdentities.Chat/"]
COPY ["src/Plugins/AIdentities.BooruAIdentityImporter/AIdentities.BooruAIdentityImporter.csproj", "src/Plugins/AIdentities.BooruAIdentityImporter/"]
RUN dotnet restore "src/AIdentities.UI/AIdentities.UI.csproj"
COPY . .
WORKDIR "/src/src/AIdentities.UI"
RUN dotnet build "AIdentities.UI.csproj" -c Release -o /app/build /p:AddPluginBuilder=false /p:ExcludeAdiManifest=true

FROM build AS publish
RUN dotnet publish "AIdentities.UI.csproj" -c Release -o /app/publish /p:UseAppHost=false /p:AddPluginBuilder=false /p:ExcludeAdiManifest=true

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AIdentities.UI.dll"]
