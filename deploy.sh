#!/bin/bash

# Production deployment script
set -e

echo "🚀 Starting production deployment..."

# Check if .env file exists
if [ ! -f .env ]; then
    echo "❌ Error: .env file not found!"
    echo "Please copy .env.example to .env and configure your production values."
    exit 1
fi

# Source environment variables
source .env

# Validate required environment variables
if [ -z "$DB_PASSWORD" ] || [ -z "$JWT_SECRET_KEY" ]; then
    echo "❌ Error: Required environment variables not set!"
    echo "Please ensure DB_PASSWORD and JWT_SECRET_KEY are set in .env file."
    exit 1
fi

# Stop existing containers
echo "🛑 Stopping existing containers..."
docker compose -f docker-compose.prod.yml down

# Pull latest images
echo "📥 Pulling latest images..."
docker compose -f docker-compose.prod.yml pull

# Build and start services
echo "🔨 Building and starting services..."
docker compose -f docker-compose.prod.yml up -d --build

# Wait for services to be healthy
echo "⏳ Waiting for services to be healthy..."
sleep 30

# Check if services are running
if docker compose -f docker-compose.prod.yml ps --services --filter "status=running" | grep -q server; then
    echo "✅ Application deployed successfully!"
    echo "🌐 API is available at: http://localhost"
    echo "📊 Database is available at: localhost:1433"
else
    echo "❌ Deployment failed! Check logs with: docker compose -f docker-compose.prod.yml logs"
    exit 1
fi
