#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:3.1 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:3.1 AS build
WORKDIR /src
COPY ["src/MyProject.Web.Host/MyProject.Web.Host.csproj", "src/MyProject.Web.Host/"]
COPY ["src/MyProject.Web.Core/MyProject.Web.Core.csproj", "src/MyProject.Web.Core/"]
COPY ["src/MyProject.Application/MyProject.Application.csproj", "src/MyProject.Application/"]
COPY ["src/MyProject.Core/MyProject.Core.csproj", "src/MyProject.Core/"]
COPY ["src/MyProject.EntityFrameworkCore/MyProject.EntityFrameworkCore.csproj", "src/MyProject.EntityFrameworkCore/"]
RUN dotnet restore "src/MyProject.Web.Host/MyProject.Web.Host.csproj"
COPY . .
WORKDIR "/src/src/MyProject.Web.Host"
RUN dotnet build "MyProject.Web.Host.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MyProject.Web.Host.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MyProject.Web.Host.dll"]