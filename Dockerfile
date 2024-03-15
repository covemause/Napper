# Learn about building .NET container images:
# https://github.com/dotnet/dotnet-docker/blob/main/samples/README.md
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG TARGETARCH
WORKDIR /source

# copy csproj and restore as distinct layers
COPY Napper/*.csproj .
RUN dotnet restore -a $TARGETARCH

# copy everything else and build app
COPY Napper/. .
RUN dotnet publish -a $TARGETARCH --no-restore -o /app


# final stage/image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
EXPOSE 5025
WORKDIR /app
COPY --from=build /app .
USER $APP_UID
ENTRYPOINT ["./Napper"]