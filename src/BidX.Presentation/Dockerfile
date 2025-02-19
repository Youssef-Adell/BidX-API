# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# Copy project files and restore first to take advantage of docker layer caching (This avoids re-downloading and restoring all the NuGet packages in future builds)
COPY src/BidX.sln ./
COPY src/BidX.Presentation/BidX.Presentation.csproj BidX.Presentation/
COPY src/BidX.BusinessLogic/BidX.BusinessLogic.csproj BidX.BusinessLogic/
COPY src/BidX.DataAccess/BidX.DataAccess.csproj BidX.DataAccess/

# Restore dependencies for all projects
RUN dotnet restore

# Copy the rest of the source code and build/publish the Web API
COPY src/. .
RUN dotnet publish BidX.Presentation/BidX.Presentation.csproj -c Release -o /publish


# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /release

# Copy published files from the build stage
COPY --from=build /publish ./

# Run the API
ENTRYPOINT ["dotnet", "BidX.Presentation.dll"]
