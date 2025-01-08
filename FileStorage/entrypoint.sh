#!/bin/bash
set -e

# Set permissions for /app/data
chmod -R 755 /app/data

# Set permissions for /app/uploads
chmod -R 755 /app/uploads

# Optionally, change ownership if running as a non-root user
# chown -R appuser:appuser /app/data /app/uploads

# Execute the original entrypoint
exec "$@"
