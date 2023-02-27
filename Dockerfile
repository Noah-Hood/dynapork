#### BUILD STAGE ####
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src/

COPY ./main/src* ./
RUN dotnet build --configuration=Release --output=/dist/

#### DEPLOY STAGE ####
FROM mcr.microsoft.com/dotnet/runtime:7.0 AS deploy
WORKDIR /dynapork/

COPY --from=build /dist/* /dynapork/
CMD ["dotnet", "/dynapork/main.dll"]