#### BUILD STAGE ####
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src/

# Restore separately, since fsproj files change rarely. 
# Speeds up the build.
COPY ./main/src/main.fsproj ./
RUN dotnet restore

# Build source files separately, which change often.
COPY ./main/src/*.fs ./
RUN dotnet publish --configuration=Release --no-restore --output=/dist/

#### DEPLOY STAGE ####
FROM mcr.microsoft.com/dotnet/runtime:7.0 AS deploy
WORKDIR /dynapork/

ENV ENVIRONMENT=PRODUCTION

COPY --from=build /dist/* /dynapork/
ENTRYPOINT ["dotnet", "/dynapork/main.dll"]