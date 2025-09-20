# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy solution file
COPY Sanad.sln .

# Copy project files
COPY Sanad/*.csproj ./Sanad/
RUN dotnet restore

# Copy everything else
COPY Sanad/. ./Sanad/
WORKDIR /app/Sanad
RUN dotnet publish -c Release -o /app/publish

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# Add this line to bind Kestrel to Railway's dynamic port
ENV ASPNETCORE_URLS=http://+:${PORT}

COPY --from=build /app/publish .
EXPOSE 80
ENTRYPOINT ["dotnet", "Sanad.dll"]
