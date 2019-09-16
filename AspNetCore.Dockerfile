FROM mcr.microsoft.com/dotnet/core/aspnet:2.2-alpine3.9 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/core/sdk:2.2-alpine3.9 AS build
WORKDIR /src
COPY src/AspNetCore /src/AspNetCore
COPY src/Drift /src/Drift
RUN dotnet publish /src/AspNetCore/AspNetCore.csproj -c Release -o /app

FROM base as final
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT [ "dotnet", "Drift.AspNetCore.dll" ]