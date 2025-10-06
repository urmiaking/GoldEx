# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY nuget ./nuget
COPY ["nuget.config", "."]
COPY ["src/App/Server/GoldEx.Server/GoldEx.Server.csproj", "src/App/Server/GoldEx.Server/"]
COPY ["src/App/Client/GoldEx.Client/GoldEx.Client.csproj", "src/App/Client/GoldEx.Client/"]
COPY ["src/App/Client/GoldEx.Client.Components/GoldEx.Client.Components.csproj", "src/App/Client/GoldEx.Client.Components/"]
COPY ["src/Sdk/GoldEx.Sdk.Client/GoldEx.Sdk.Client.csproj", "src/Sdk/GoldEx.Sdk.Client/"]
COPY ["src/Sdk/GoldEx.Sdk.Common/GoldEx.Sdk.Common.csproj", "src/Sdk/GoldEx.Sdk.Common/"]
COPY ["src/App/Shared/GoldEx.Shared/GoldEx.Shared.csproj", "src/App/Shared/GoldEx.Shared/"]
COPY ["src/App/Client/GoldEx.Client.Abstractions/GoldEx.Client.Abstractions.csproj", "src/App/Client/GoldEx.Client.Abstractions/"]
COPY ["src/App/Client/GoldEx.Client.Services/GoldEx.Client.Services.csproj", "src/App/Client/GoldEx.Client.Services/"]
COPY ["src/App/Server/GoldEx.Server.Application/GoldEx.Server.Application.csproj", "src/App/Server/GoldEx.Server.Application/"]
COPY ["src/App/Server/GoldEx.Server.Infrastructure/GoldEx.Server.Infrastructure.csproj", "src/App/Server/GoldEx.Server.Infrastructure/"]
COPY ["src/App/Server/GoldEx.Server.Domain/GoldEx.Server.Domain.csproj", "src/App/Server/GoldEx.Server.Domain/"]
COPY ["src/Sdk/GoldEx.Sdk.Server/GoldEx.Sdk.Server.csproj", "src/Sdk/GoldEx.Sdk.Server/"]
RUN dotnet restore "./src/App/Server/GoldEx.Server/GoldEx.Server.csproj"

RUN rm -rf ./nuget ./nuget.config

COPY . .
WORKDIR "/src/src/App/Server/GoldEx.Server"
RUN dotnet build "./GoldEx.Server.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./GoldEx.Server.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "GoldEx.Server.dll"]