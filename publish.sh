#!/bin/bash

cd src/WebApi

rm -rf ../../publish

dotnet publish -c Release -o ../../publish -r alpine-x64 /p:PublishTrimmed=true /p:DebugType=None /p:DebugSymbols=false