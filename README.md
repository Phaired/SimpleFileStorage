# SimpleFileStorage

SimpleFileStorage is a lightweight file storage service built with .NET. This README provides instructions for using the Docker image with `docker-compose`.


## Instructions

1. **Create SQLite Database**: Create the file `documents.db` using `sqlite3 documents.db ".exit"`
2. **Create Uploads Directory**: Ensure you have an `uploads` directory in your project root to store uploaded files.


### Using Docker Compose

To run the SimpleFileStorage service with Docker Compose, create a `docker-compose.yml` file with the following content:

```yaml
services:
  simplestorage:
    image: ghcr.io/phaired/simplefilestorage:latest
    container_name: simplestorage
    ports:
      - "5000:5000"
    volumes:
      - ./uploads:/app/uploads
      - ./documents.db:/app/documents.db
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
```

### Running the Service

To start the SimpleFileStorage service, run the following command:

```sh
docker-compose up -d
```

This command will start the service in detached mode. You can access the service at `http://localhost:5000`.

### Stopping the Service

To stop the SimpleFileStorage service, run:

```sh
docker-compose down
```

## Contributing

Feel free to submit issues or pull requests.

## License

This project is licensed under the MIT License.
