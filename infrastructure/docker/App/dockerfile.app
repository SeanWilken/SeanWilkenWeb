FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# Node + pnpm for Fable/Vite client build
RUN curl -fsSL https://deb.nodesource.com/setup_20.x | bash - \
    && apt-get update \
    && apt-get install -y nodejs \
    && npm install -g pnpm \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /app

# -----------------------------
# Copy entire repo into image
# (relies on .dockerignore to skip bin/obj/node_modules/etc)
# -----------------------------
COPY . .

# -----------------------------
# Restore dotnet tools (Fable) and server deps
# -----------------------------
RUN dotnet tool restore
RUN dotnet restore src/Server/Server.fsproj

# -----------------------------
# Install client deps
# -----------------------------
WORKDIR /app/src/Client
RUN pnpm install

# -----------------------------
# Build-time env vars for Vite
# -----------------------------
ARG VITE_STRIPE_API_PK_TEST
ARG VITE_API_BASE_URL
ARG SERVER_PROXY_PORT

ENV VITE_STRIPE_API_PK_TEST=$VITE_STRIPE_API_PK_TEST \
    VITE_API_BASE_URL=$VITE_API_BASE_URL \
    SERVER_PROXY_PORT=$SERVER_PROXY_PORT

# -----------------------------
# Fable + Vite in one go (like dev)
# -----------------------------
# Equivalent to: dotnet fable Client.fsproj -o output --run "pnpm exec vite build"
RUN dotnet fable Client.fsproj -o output --run pnpm exec vite build

# -----------------------------
# Publish Server
# -----------------------------
WORKDIR /app/src/Server
RUN dotnet publish -c Release -o /out

# =============================
# Runtime Image
# =============================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

WORKDIR /app

ARG APP_ENV=Production
ENV ASPNETCORE_URLS=http://+:5000 \
    ASPNETCORE_ENVIRONMENT=$APP_ENV

COPY --from=build /out ./

# Saturn uses `use_static "public"`
COPY --from=build /app/deploy/public ./public

EXPOSE 5000

ENTRYPOINT ["dotnet", "Server.dll"]
