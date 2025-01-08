# SimpleFileStorage

SimpleFileStorage is a lightweight file storage service built with .NET. This README provides instructions for using the Docker image with `docker-compose`.

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
      - ./data:/app/data/
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - DownloadToken=sample
      - UploadToken=sample
```

You can provide some token if you want to add auth for upload/download file.


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
