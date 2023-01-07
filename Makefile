build:
	docker compose build

push:
	docker compose push

up:
	docker compose up

down:
	docker compose down

idp:
	docker compose up idp

rm-idp:
	docker compose stop idp

api:
	docker compose up api

rm-api:
	docker compose stop api

client:
	docker compose up client

rm-client:
	docker compose stop client

run:
	docker run -p 8083:8083 \
               -e ASPNETCORE_URLS=http://*:8083 \
               -e ASPNETCORE_ENVIRONMENT="Development" \
               -e SEQ_SERVER_URL=$(SEQ_SERVER_URL) \
			   --name imagegallery-idp \
			   --rm tmather99/dapr_client 

stop:
	docker stop imagegallery-idp
