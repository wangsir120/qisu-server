# 构建阶段
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["qisu-server.csproj", "./"]
RUN dotnet restore "qisu-server.csproj"
COPY . .
RUN dotnet publish "qisu-server.csproj" -c Release -o /app/publish --no-restore

# 运行阶段
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# 安装常用工具（用于调试）
RUN apt-get update && apt-get install -y --no-install-recommends \
    curl \
    && rm -rf /var/lib/apt/lists/*

# 创建非 root 用户
RUN adduser --disabled-password --gecos '' appuser

# 复制发布文件
COPY --from=build /app/publish .

# 创建 uploads 目录
RUN mkdir -p /app/uploads && chown -R appuser:appuser /app

# 切换到非 root 用户
USER appuser

# 暴露端口
EXPOSE 5000

# 健康检查
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:5000/api/ || exit 1

# 启动应用
ENTRYPOINT ["dotnet", "qisu-server.dll"]