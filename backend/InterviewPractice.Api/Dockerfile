# -----------------------------
# BUILD
# -----------------------------
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

WORKDIR /src

COPY ["InterviewPractice.Api/InterviewPractice.Api.csproj", "InterviewPractice.Api/"]
COPY ["InterviewPractice.Application/InterviewPractice.Application.csproj", "InterviewPractice.Application/"]
COPY ["InterviewPractice.Domain/InterviewPractice.Domain.csproj", "InterviewPractice.Domain/"]
COPY ["InterviewPractice.Infrastructure/InterviewPractice.Infrastructure.csproj", "InterviewPractice.Infrastructure/"]

RUN dotnet restore "InterviewPractice.Api/InterviewPractice.Api.csproj"

COPY . .

WORKDIR "/src/InterviewPractice.Api"

RUN dotnet publish "InterviewPractice.Api.csproj" \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false


# -----------------------------
# RUNTIME
# -----------------------------
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final

WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Development

EXPOSE 8080

ENTRYPOINT ["dotnet", "InterviewPractice.Api.dll"]