# Notes API - Deployment Guide

## Prerequisites

- Docker and Docker Compose installed
- Git

## Production Deployment

### 1. Environment Setup

1. Copy the environment template:
   ```bash
   cp .env.example .env
   ```

2. Edit `.env` file with your production values:
   ```bash
   # Set a strong database password
   DB_PASSWORD=YourSecurePasswordHere123!
   
   # Set a strong JWT secret (minimum 32 characters)
   JWT_SECRET_KEY=YourSuperSecretJWTKeyThatShouldBeAtLeast32CharactersLongForProduction
   ```

### 2. Deploy the Application

Run the deployment script:
```bash
./deploy.sh
```

Or manually:
```bash
docker compose -f docker-compose.prod.yml up -d --build
```

### 3. Verify Deployment

- API Health: `http://localhost/health`
- API Swagger: `http://localhost/swagger`
- Database: `localhost:1433`

### 4. Monitoring

Check logs:
```bash
# All services
docker compose -f docker-compose.prod.yml logs

# Specific service
docker compose -f docker-compose.prod.yml logs server
docker compose -f docker-compose.prod.yml logs db
```

Check status:
```bash
docker compose -f docker-compose.prod.yml ps
```

### 5. Backup and Recovery

Backup database:
```bash
docker compose -f docker-compose.prod.yml exec db /opt/mssql-tools18/bin/sqlcmd \
  -S localhost -U sa -P "$DB_PASSWORD" \
  -Q "BACKUP DATABASE NotesAppDB TO DISK = '/var/opt/mssql/backup/notesapp.bak'"
```

### 6. Updates

To update the application:
```bash
git pull
./deploy.sh
```

## Security Notes

- Change default passwords in production
- Use environment variables for sensitive data
- Keep your `.env` file secure and never commit it to version control
- Regularly update Docker images
- Consider using a reverse proxy (nginx) for SSL termination
- Implement proper logging and monitoring

## Cloud Deployment

This application can be deployed to:
- AWS ECS/Fargate
- Azure Container Instances
- Google Cloud Run
- DigitalOcean App Platform
- Any Docker-compatible hosting platform
