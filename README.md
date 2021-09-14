# kubernetes-filesystem

System to browse, upload or download files for pods or containers in kubernetes.

## Language 

[中文文档](README_CN.md)

## How to Use

- Create your cluster with your `kubeconfig`

![image](https://user-images.githubusercontent.com/6275608/133240180-d7c95f8c-ee7b-4e0d-8714-013ac564567c.png)

- Browse, upload or download file(s)

![image](https://user-images.githubusercontent.com/6275608/133241063-8e8c1aa9-1352-4ada-b8c8-2042f1e82aec.png)

## Requirements

`Docker` is installed in your system.

## How to Run

### Build

```bash
bash build.sh
```

### Pack to docker image

```bash
bash pack.sh
```

### Run

```bash
docker run --name kubernetes-filesystem -d \
    -v /usr/local/bin/kubectl:/usr/local/bin/kubectl \  # mount kubectl into container
    -v /root/k8s-config/:/root/k8s-config/ \            # optional, config persistence
    -p 80:80 \
    kubernetes-filesystem:1.0.0
```

or

```bash
bash run.sh
```

## How to Develop

```bash
git clone https://github.com/ErikXu/kubernetes-filesystem.git
cd kubernetes-filesystem/src/WebApi
dotnet restore
dotnet run
```
