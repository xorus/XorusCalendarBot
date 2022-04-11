FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["XorusCalendarBot/XorusCalendarBot.csproj", "XorusCalendarBot/"]
RUN dotnet restore "XorusCalendarBot/XorusCalendarBot.csproj"
COPY . .
WORKDIR "/src/XorusCalendarBot"
RUN dotnet build "XorusCalendarBot.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "XorusCalendarBot.csproj" -c Release -o /app/publish

FROM node:alpine AS web-build
RUN apk add yarn && mkdir -p /src
COPY ["Web/", "/src/Web/"] 
WORKDIR /src/Web
RUN yarn install --non-interactive && yarn build 

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV TZ=Europe/Paris
RUN ln -snf /usr/share/zoneinfo/$TZ /etc/localtime && echo $TZ > /etc/timezone
COPY --from=web-build /src/Web/out /web
ENV STATIC_HTML_PATH /web
ENV DB_PATH /app/database.db

ENTRYPOINT ["dotnet", "XorusCalendarBot.dll"]
