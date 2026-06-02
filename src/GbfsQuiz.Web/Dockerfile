# Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Directory.Build.props ./
COPY src/GbfsQuiz.Domain/GbfsQuiz.Domain.csproj src/GbfsQuiz.Domain/
COPY src/GbfsQuiz.Application/GbfsQuiz.Application.csproj src/GbfsQuiz.Application/
COPY src/GbfsQuiz.Infrastructure/GbfsQuiz.Infrastructure.csproj src/GbfsQuiz.Infrastructure/
COPY src/GbfsQuiz.Web/GbfsQuiz.Web.csproj src/GbfsQuiz.Web/
RUN dotnet restore src/GbfsQuiz.Web/GbfsQuiz.Web.csproj

COPY . .
# Publish with restore against the full source so the Blazor framework static web assets
# (e.g. _framework/blazor.web.js) are emitted. A csproj-only restore + --no-restore can
# omit them on newer SDK builds (the runtime image ships SDK 10.0.300).
RUN dotnet publish src/GbfsQuiz.Web/GbfsQuiz.Web.csproj -c Release -o /app

# Run
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
# libgssapi-krb5-2 silences Npgsql's optional GSSAPI probe at startup.
RUN apt-get update && apt-get install -y --no-install-recommends libgssapi-krb5-2 \
    && rm -rf /var/lib/apt/lists/*
COPY --from=build /app ./
EXPOSE 8080
ENTRYPOINT ["dotnet", "GbfsQuiz.Web.dll"]
