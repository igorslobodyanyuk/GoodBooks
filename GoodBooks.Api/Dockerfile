#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["GoodBooks.Api/GoodBooks.Api.csproj", "GoodBooks.Api/"]
COPY ["GoodBooks.BusinessLogic/GoodBooks.BusinessLogic.csproj", "GoodBooks.BusinessLogic/"]
COPY ["GoodBooks.Common/GoodBooks.Common.csproj", "GoodBooks.Common/"]
COPY ["GoodBooks.Data.Model/GoodBooks.Data.Model.csproj", "GoodBooks.Data.Model/"]
RUN dotnet restore "GoodBooks.Api/GoodBooks.Api.csproj"
COPY . .
WORKDIR "/src/GoodBooks.Api"
RUN dotnet build "GoodBooks.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "GoodBooks.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "GoodBooks.Api.dll"]
