FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081
RUN apt-get update && apt-get install -y --no-install-recommends \
    fontconfig \
    libgdiplus \
    fonts-dejavu-core \
    && rm -rf /var/lib/apt/lists/*

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

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./GoldEx.Server.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY ./fonts /usr/local/share/fonts/custom/

USER root

RUN mkdir -p /app/Reports_Seed && \
    if [ -d "/app/Reports" ]; then cp -r /app/Reports/* /app/Reports_Seed/; fi

RUN echo '#!/bin/bash' > /app/start.sh && \
    echo 'cp -rn /app/Reports_Seed/* /app/Reports/ 2>/dev/null || true' >> /app/start.sh && \
    echo 'exec "$@"' >> /app/start.sh && \
    chmod +x /app/start.sh

RUN mkdir -p /app/uploads /app/Reports /app/shared /var/cache/fontconfig && \
    chmod -R 777 /var/cache/fontconfig && \
    fc-cache -fv && \
    chown -R $APP_UID:$APP_UID /app/uploads /app/Reports /app/shared /app/Reports_Seed

USER $APP_UID

ENTRYPOINT ["/app/start.sh"]
CMD ["dotnet", "GoldEx.Server.dll"]