#!/bin/bash

IMAGE_NAME=${IMAGE_NAME:-kubernetes-filesystem}
echo "IMAGE_NAME: "$IMAGE_NAME

IMAGE_TAG=${IMAGE_TAG:-1.0.0}
echo "IMAGE_TAG: "$IMAGE_TAG

docker build --no-cache --disable-content-trust=true -t $IMAGE_NAME:${IMAGE_TAG} -f ./docker/Dockerfile ./publish/