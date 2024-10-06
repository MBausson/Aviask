FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

EXPOSE 8081

COPY *.csproj ./
RUN dotnet restore

COPY . ./

RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/sdk:8.0 as runtime
WORKDIR /app

COPY --from=build /app/out ./

ENTRYPOINT [ "dotnet", "AviaskApi.dll" ]
