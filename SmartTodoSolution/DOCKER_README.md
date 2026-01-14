# SmartTodo API - Docker Setup

This document describes how to run the SmartTodo API in a containerized environment using Docker.

## Prerequisites

- Docker Engine 20.10 or later
- Docker Compose V2 (comes with Docker Desktop)

## Quick Start

1. Navigate to the SmartTodoSolution directory:
   ```bash
   cd SmartTodoSolution
   ```

2. Build and start the containers:
   ```bash
   docker compose up -d
   ```

3. The API will be available at: `http://localhost:8080`

4. Access the Swagger UI at: `http://localhost:8080/swagger`

## Container Services

The Docker Compose setup includes two services:

### PostgreSQL Database
- **Image**: postgres:17-alpine
- **Port**: 5432
- **Database**: smarttodo
- **Username**: stododev
- **Password**: P@ssw0rd
- **Volume**: postgres_data (persistent storage)

### SmartTodo API
- **Port**: 8080
- **Environment**: Docker
- **Dependencies**: PostgreSQL (waits for health check)

## Useful Commands

### Start the containers
```bash
docker compose up -d
```

### Stop the containers
```bash
docker compose down
```

### Stop and remove volumes (wipes database)
```bash
docker compose down -v
```

### View logs
```bash
# All services
docker compose logs -f

# API only
docker compose logs -f api

# PostgreSQL only
docker compose logs -f postgres
```

### Rebuild after code changes
```bash
docker compose build
docker compose up -d
```

### Access PostgreSQL directly
```bash
docker exec -it smarttodo-postgres psql -U stododev -d smarttodo
```

## Configuration

### Environment Variables

The API container uses the following environment variables (defined in docker-compose.yml):
- `ASPNETCORE_ENVIRONMENT=Docker`
- `ASPNETCORE_URLS=http://+:8080`
- `Database__UsePostgreSQL=true`
- `ConnectionStrings__PostgreSQL=Host=postgres;Port=5432;Database=smarttodo;Username=stododev;Password=P@ssw0rd`

### Configuration Files

- `Dockerfile` - Multi-stage build for the .NET API
- `docker-compose.yml` - Orchestrates the API and PostgreSQL services
- `.dockerignore` - Excludes unnecessary files from Docker build context
- `appsettings.Docker.json` - Docker-specific configuration

## Networking

The containers communicate on a bridge network called `smarttodo-network`. The API connects to PostgreSQL using the hostname `postgres` (Docker's internal DNS).

## Volumes

- `postgres_data` - Persists PostgreSQL data between container restarts

## Health Checks

PostgreSQL includes a health check that verifies the database is ready before the API starts. This prevents connection errors during startup.

## Production Considerations

For production deployments, consider:
1. Using secrets management instead of hardcoded passwords
2. Adding HTTPS/TLS support
3. Implementing container resource limits
4. Setting up proper logging and monitoring
5. Using a reverse proxy (nginx, Traefik)
6. Implementing backup strategies for the database

## Troubleshooting

### API won't start
- Check logs: `docker compose logs api`
- Verify PostgreSQL is healthy: `docker compose ps`

### Database connection errors
- Ensure PostgreSQL container is running: `docker compose ps postgres`
- Check connection string in docker-compose.yml

### Port already in use
- Change the port mapping in docker-compose.yml:
  ```yaml
  ports:
    - "8081:8080"  # Use 8081 instead of 8080
  ```
