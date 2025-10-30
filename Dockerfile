FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

# Solution en Project files overzetten
COPY ./src/knmidownloader.sln .
COPY ./src/knmidownloader/knmidownloader.csproj ./knmidownloader/

# Restore voor eventuele dependencies
RUN dotnet restore

COPY ./src .

# Final build met alle source files
RUN dotnet publish -c release -o /app/buildOutput

FROM mcr.microsoft.com/dotnet/runtime:8.0 AS runtime

WORKDIR /app

COPY --from=build /app/buildOutput .

CMD ["dotnet", "knmidownloader.dll", "dodiscord"]
