# GoodBooks

GoodBooks is a Books API solution.

## Installation and usage

Download and install an appropriate Docker Desktop application from the docker official website: https://www.docker.com/get-started

Pull this repository from the GitHub.

Open GoodBooks.sln.

Set docker-compose.dcproj up as a start project and launch it in debug configuration.

Alternatively, you could run the same docker containers with the following commands in terminal and then open https://localhost:8001 to open GoodBooks API in Swagger.

```docker
docker-compose  -f "docker-compose.yml" -f "docker-compose.override.yml" --no-ansi build
docker-compose  -f "docker-compose.yml" -f "docker-compose.override.yml" --no-ansi up -d --no-build --force-recreate --remove-orphans

