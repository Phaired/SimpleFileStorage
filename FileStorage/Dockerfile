﻿# Stage 1: Build environment
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0-jammy AS build-env

# Stage 2: Base image
FROM mcr.microsoft.com/dotnet/aspnet:8.0-jammy-chiseled AS base
USER $APP_UID
WORKDIR /app
EXPOSE 5000

# Stage 3: Build the application
FROM build-env AS build
ARG BUILD_CONFIGURATION=Release
ARG TARGETARCH
WORKDIR /src
COPY ["FileStorage.csproj", "./"]
RUN dotnet restore -a $TARGETARCH
COPY . .
RUN dotnet build -c $BUILD_CONFIGURATION -o /app/build

# Stage 4: Publish the application
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Stage 5: Final image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FileStorage.dll"]