FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY nuget.config .
COPY .nuget/ .nuget/

# Copy csproj files
COPY TechnoOnlineShop.IdentityServer.Api/*.csproj ./TechnoOnlineShop.IdentityServer.Api/
COPY TechnoOnlineShop.IdentityServer.Application/*.csproj ./TechnoOnlineShop.IdentityServer.Application/
COPY TechnoOnlineShop.IdentityServer.Application.Contracts/*.csproj ./TechnoOnlineShop.IdentityServer.Application.Contracts/
COPY TechnoOnlineShop.IdentityServer.Domain/*.csproj ./TechnoOnlineShop.IdentityServer.Domain/
COPY TechnoOnlineShop.IdentityServer.Infrastructure.EF/*.csproj ./TechnoOnlineShop.IdentityServer.Infrastructure.EF/
COPY TechnoOnlineShop.IdentityServer.Api/certificates/ ./certificates/


# Copy license into container
# COPY TechnoOnlineShop.Api/sixlabors.lic /app/sixlabors.lic #COPY sixlabors.lic /app/sixlabors.lic

# ✅ Copy solution file (THIS IS CRITICAL)
COPY *.slnx ./

# ✅ Restore using correct path
# Cache NuGet packages
RUN --mount=type=cache,id=iden-nuget,target=/root/.nuget/packages \
    dotnet restore TechnoOnlineShop.IdentityServer.slnx

# Copy everything else
COPY . .

# Build & publish
WORKDIR /src/TechnoOnlineShop.IdentityServer.Api
RUN --mount=type=cache,id=iden-nuget,target=/root/.nuget/packages \
    dotnet publish -c Release -o /app/publish --no-restore

# ========================
# RUNTIME STAGE
# ========================
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app

# ✅ FIX: Required for PostgreSQL (Npgsql)
#RUN apt-get update && apt-get install -y \
    #libkrb5-3 \
    #libgssapi-krb5-2 \
    #&& rm -rf /var/lib/apt/lists/*

# (Optional: only if you render images/captcha)
# RUN apt-get update && apt-get install -y \
#     fontconfig fonts-dejavu fonts-liberation fonts-noto \
#     && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .
#COPY --from=build /app/sixlabors.lic /app/sixlabors.lic

# Copy certificates
#COPY TechnoOnlineShop.IdentityServer.Api/certificates/ ./certificates/

EXPOSE 5000
ENTRYPOINT ["dotnet", "TechnoOnlineShop.IdentityServer.Api.dll"]
