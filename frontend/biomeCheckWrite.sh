#!/bin/bash

#docker build -t crystal-web .
#docker run -d -p 3000:80 --name frontend-dev crystal-web
pnpm biome check --write ./
pnpm biome check --write . --unsafe
