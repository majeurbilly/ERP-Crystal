#!/bin/bash

docker compose down
docker compose up -d --build
cd frontend
pnpx json-server --watch db.json --port 5000
