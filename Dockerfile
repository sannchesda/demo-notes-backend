# Use the official .NET SDK image for build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY NotesAPI/NotesAPI.csproj ./NotesAPI/
RUN dotnet restore ./NotesAPI/NotesAPI.csproj

# Copy everything else and build
COPY NotesAPI/. ./NotesAPI/
WORKDIR /app/NotesAPI
RUN dotnet publish -c Release -o out

# Use the official ASP.NET runtime image for the final container
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/NotesAPI/out .

# Expose port 80
EXPOSE 80

# Start the API
ENTRYPOINT ["dotnet", "NotesAPI.dll"]
