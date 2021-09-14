# kubernetes 文件系统

一个用于浏览、上传、下载 kubernetes 文件的系统

## 语言

[English Doc](README.md)

## 如何使用

- 创建一个集群，并上传 `kubeconfig`

![image](https://user-images.githubusercontent.com/6275608/133240180-d7c95f8c-ee7b-4e0d-8714-013ac564567c.png)

- 浏览、上传、下载文件

![image](https://user-images.githubusercontent.com/6275608/133241063-8e8c1aa9-1352-4ada-b8c8-2042f1e82aec.png)

## 系统要求

安装了 `Docker`

## 如何运行

### 编译

```bash
bash build.sh
```

### 打包 Docker 镜像

```bash
bash pack.sh
```

### 运行

```bash
docker run --name kubernetes-filesystem -d \
    -v /usr/local/bin/kubectl:/usr/local/bin/kubectl \  # 把 kubectl 挂载到容器
    -v /root/k8s-config/:/root/k8s-config/ \            # 可选, 配置文件持久化
    -p 80:80 \
    kubernetes-filesystem:1.0.0
```

或者

```bash
bash run.sh
```

## 如何开发

```bash
git clone https://github.com/ErikXu/kubernetes-filesystem.git
cd kubernetes-filesystem/src/WebApi
dotnet restore
dotnet run
```
