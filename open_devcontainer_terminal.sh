#!/usr/bin/env bash

id=$(docker ps --filter "name=devcontainer-app" --format "{{.ID}}")

docker exec -it -w /var/www/html $id bash