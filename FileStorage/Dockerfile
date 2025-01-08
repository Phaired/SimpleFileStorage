# Stage 1: Build environment
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:9.0 AS build-env

# Stage 2: Base image (corrected)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
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

# Copy the entrypoint script
COPY entrypoint.sh /app/entrypoint.sh

# Make the entrypoint script executable
RUN chmod +x /app/entrypoint.sh

# Create necessary directories (if not using entrypoint.sh for this)
RUN mkdir -p /app/data /app/uploads

# (Optional) Create a non-root user for running the application
RUN groupadd -r appuser && useradd -r -g appuser appuser

# Switch to the non-root user
USER appuser

# Copy the published application
COPY --from=publish /app/publish .

# Set the entrypoint to the script
ENTRYPOINT ["/app/entrypoint.sh"]