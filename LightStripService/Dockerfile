FROM    mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY    . .
RUN     dotnet restore
RUN     dotnet build
CMD     sh

FROM	mcr.microsoft.com/dotnet/aspnet:6.0-alpine
WORKDIR /app
COPY    --from=build /src/build/Debug/net6.0/. .
CMD	["dotnet", "/app/LightStripService.dll"]
