# SmartTodo API - Docker Setup

This document describes how to run the SmartTodo API in a containerized environment using Docker.

## Prerequisites

- Docker Engine 20.10 or later
- Docker Compose V2 (comes with Docker Desktop)
- An existing PostgreSQL database (running in Docker or on your host machine)

## Quick Start

1. **Configure your PostgreSQL connection** (choose one method):

   **Method A: Using .env file (Recommended)**
   ```bash
   cp .env.example .env
   # Edit .env and set your PostgreSQL connection details
   ```

   **Method B: Edit docker-compose.yml directly**

   Update the connection string in `docker-compose.yml` to point to your existing PostgreSQL instance.

   See the [Configuration](#configuration) section below for detailed options.

2. Navigate to the SmartTodoSolution directory:
   ```bash
   cd SmartTodoSolution
   ```

3. Build and start the API container:
   ```bash
   docker compose up -d
   ```

4. The API will be available at: `http://localhost:8080`

5. Access the Swagger UI at: `http://localhost:8080/swagger`

## Container Services

The Docker Compose setup includes:

### SmartTodo API
- **Port**: 8080
- **Environment**: Docker
- **Database**: Connects to your existing PostgreSQL instance

## Useful Commands

### Start the containers
```bash
docker compose up -d
```

### Stop the containers
```bash
docker compose down
```

### View logs
```bash
docker compose logs -f api
```

### Rebuild after code changes
```bash
docker compose build
docker compose up -d
```

## Configuration

### Connecting to Your Existing PostgreSQL Database

The `docker-compose.yml` file needs to be configured to connect to your existing PostgreSQL instance. There are several options:

#### Option 1: PostgreSQL on Host Machine (Recommended for Docker Desktop)

If your PostgreSQL is running on your host machine (not in Docker), use `host.docker.internal`:

```yaml
environment:
  - ConnectionStrings__PostgreSQL=Host=host.docker.internal;Port=5432;Database=smarttodo;Username=YOUR_USER;Password=YOUR_PASSWORD
```

This is already configured in the default `docker-compose.yml`.

#### Option 2: PostgreSQL in Another Docker Container (Same Network)

If your PostgreSQL is in another Docker container, add the API to the same network:

1. Find your PostgreSQL container's network:
   ```bash
   docker inspect YOUR_POSTGRES_CONTAINER_NAME | grep NetworkMode
   ```

2. Update `docker-compose.yml`:
   ```yaml
   services:
     api:
       # ... other settings ...
       environment:
         - ConnectionStrings__PostgreSQL=Host=YOUR_POSTGRES_CONTAINER_NAME;Port=5432;Database=smarttodo;Username=YOUR_USER;Password=YOUR_PASSWORD
       networks:
         - your-postgres-network

   networks:
     your-postgres-network:
       external: true
       name: YOUR_NETWORK_NAME
   ```

#### Option 3: PostgreSQL in Another Docker Container (Bridge to Network)

Alternatively, connect your PostgreSQL container to the API:

```bash
docker network create smarttodo-network
docker network connect smarttodo-network YOUR_POSTGRES_CONTAINER_NAME
```

Then update the connection string to use your PostgreSQL container name:
```yaml
- ConnectionStrings__PostgreSQL=Host=YOUR_POSTGRES_CONTAINER_NAME;Port=5432;Database=smarttodo;Username=YOUR_USER;Password=YOUR_PASSWORD
```

#### Option 4: Using Host Network Mode (Linux only)

For Linux hosts, you can use host networking:

```yaml
services:
  api:
    # ... other settings ...
    network_mode: host
    environment:
      - ConnectionStrings__PostgreSQL=Host=localhost;Port=5432;Database=smarttodo;Username=YOUR_USER;Password=YOUR_PASSWORD
```

Note: Remove the `ports` section when using `network_mode: host`.

### Environment Variables

The API container uses the following environment variables (defined in docker-compose.yml):
- `ASPNETCORE_ENVIRONMENT=Docker`
- `ASPNETCORE_URLS=http://+:8080`
- `Database__UsePostgreSQL=true`
- `ConnectionStrings__PostgreSQL` - Connection string to your PostgreSQL instance

### Configuration Files

- `Dockerfile` - Multi-stage build for the .NET API
- `docker-compose.yml` - Defines the API service configuration
- `.dockerignore` - Excludes unnecessary files from Docker build context
- `.env.example` - Template for environment variables (copy to `.env` and customize)
- `appsettings.Docker.json` - Docker-specific configuration

## Networking

The API container needs network access to your existing PostgreSQL database. By default, the configuration uses `host.docker.internal` which allows the container to connect to services running on the host machine. See the [Configuration](#configuration) section for other networking options.

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
- Verify the container is running: `docker compose ps`

### Database connection errors

Common causes and solutions:

1. **"No such host" or "Name resolution failure"**
   - If using `host.docker.internal`: Ensure you're using Docker Desktop or have the `extra_hosts` configuration
   - Try using your host's IP address instead: `Host=192.168.X.X;...`

2. **"Connection refused"**
   - Verify PostgreSQL is running: `docker ps` (if in container) or `systemctl status postgresql` (if on host)
   - Check PostgreSQL is listening on the correct port
   - Verify PostgreSQL accepts connections from Docker containers (check `pg_hba.conf`)

3. **Authentication failure**
   - Double-check username and password in the connection string
   - Verify the database exists: `psql -U YOUR_USER -l`

4. **Network issues**
   - If PostgreSQL is in another container, ensure both containers are on the same network
   - Test connectivity: `docker exec -it smarttodo-api ping YOUR_POSTGRES_HOST`

### Port already in use
- Change the port mapping in docker-compose.yml:
  ```yaml
  ports:
    - "8081:8080"  # Use 8081 instead of 8080
  ```

### Finding your PostgreSQL connection details

If you're unsure of your PostgreSQL connection details:

```bash
# List all running containers
docker ps

# Inspect your PostgreSQL container
docker inspect YOUR_POSTGRES_CONTAINER_NAME

# Check what port PostgreSQL is listening on
docker port YOUR_POSTGRES_CONTAINER_NAME
```
