FROM mcr.microsoft.com/dotnet/runtime:5.0-alpine

# RUN sed -i 's/dl-cdn.alpinelinux.org/mirrors.aliyun.com/g' /etc/apk/repositories

COPY . /app
WORKDIR /app
EXPOSE 80

ENTRYPOINT ["/app/WebApi", "--urls=http://*:80"]