#!/bin/bash
set -e

# Ensure /app/data and /app/uploads exist
mkdir -p /app/data /app/uploads

# Set permissions for /app/data and /app/uploads
chmod -R 755 /app/data /app/uploads

# Change ownership to appuser
chown -R appuser:appuser /app/data /app/uploads

# Execute the application as appuser
exec su appuser -c "dotnet FileStorage.dll"
