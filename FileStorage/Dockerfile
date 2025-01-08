# Stage 1: Build environment
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:9.0 AS build-env

# Stage 2: Base image
FROM mcr.microsoft.com/dotnet/aspnet:9.0-noble-chiseled AS base
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

# Switch to root user to set up directories and permissions
USER root

# Create necessary directories and set permissions
RUN mkdir -p /app/data /app/uploads && chmod -R 755 /app/data /app/uploads

# (Optional) Create a non-root user for running the application
# Replace 'appuser' with your desired username
RUN groupadd -r appuser && useradd -r -g appuser appuser

# Change ownership of the directories to the non-root user
RUN chown -R appuser:appuser /app/data /app/uploads

# Switch back to the non-root user
USER appuser

# Copy the published application
COPY --from=publish /app/publish .

# Set the entrypoint
ENTRYPOINT ["dotnet", "FileStorage.dll"]
