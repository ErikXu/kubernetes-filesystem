#!/bin/bash

docker rm -f kubernetes-filesystem

docker run --name kubernetes-filesystem -d \
    -v /usr/local/bin/kubectl:/usr/local/bin/kubectl \
    -v /root/k8s-config/:/root/k8s-config/ \
    -p 80:80 \
    kubernetes-filesystem:1.0.0
