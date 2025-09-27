# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Backend.csproj", ""]
RUN dotnet restore "Backend.csproj"
COPY . .
# Copy .env file if it exists
COPY .env* ./
WORKDIR "/src"
RUN dotnet build "Backend.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Backend.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
# Copy .env file if it exists
COPY .env* ./

# Create startup script
RUN echo '#!/bin/bash\n\
echo "Loading environment variables from .env file..."\n\
if [ -f .env ]; then\n\
  export $(cat .env | grep -v "^#" | xargs)\n\
  echo "Environment variables loaded from .env file"\n\
else\n\
  echo "No .env file found, using system environment variables"\n\
fi\n\
echo "Waiting for database..."\n\
sleep 10\n\
echo "Running database migrations..."\n\
dotnet ef database update --no-build\n\
echo "Starting application..."\n\
dotnet Backend.dll' > /app/startup.sh && chmod +x /app/startup.sh

ENTRYPOINT ["/app/startup.sh"]