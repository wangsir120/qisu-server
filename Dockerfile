# 使用ASP.NET Core运行时作为基础镜像
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
EXPOSE 5000

# 复制所有文件
COPY . .

# 启动应用
ENTRYPOINT ["dotnet", "qisu-server.dll"]