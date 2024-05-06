# Use the .NET SDK image to build the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY *.csproj ./
RUN dotnet restore

# Copy everything else and build
COPY . ./
RUN dotnet publish -c Release -o out

# Now, create the runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# Expose the ports your application uses. Adjust these as needed.
#EXPOSE 8080
#EXPOSE 8081
#EXPOSE 5432

# Copy the published app from the build environment to the runtime environment
COPY --from=build-env /app/out ./
ENTRYPOINT ["dotnet", "ztlme.dll"]
