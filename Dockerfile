# Use the official .NET SDK image for build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY NotesAPI/NotesAPI.csproj ./NotesAPI/
RUN dotnet restore ./NotesAPI/NotesAPI.csproj

# Copy everything else and build
COPY NotesAPI/. ./NotesAPI/
WORKDIR /app/NotesAPI
RUN dotnet publish -c Release -o out --no-restore

# Use the official ASP.NET runtime image for the final container
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Create a non-root user
RUN adduser --disabled-password --gecos '' appuser

# Copy the published app
COPY --from=build /app/NotesAPI/out .

# Change ownership to appuser
RUN chown -R appuser:appuser /app
USER appuser

# Expose port 80
EXPOSE 80

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
  CMD curl -f http://localhost/health || exit 1

# Start the API
ENTRYPOINT ["dotnet", "NotesAPI.dll"]
