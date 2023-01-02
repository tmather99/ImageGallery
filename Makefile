build:
	docker compose build

up:
	docker compose up

down:
	docker compose down

push:
	docker push tmather99/imagegallery-idp:$(VERSION)

run:
	docker run -p 8083:8083 \
               -e ASPNETCORE_URLS=http://*:8083 \
               -e ASPNETCORE_ENVIRONMENT="Development" \
               -e SEQ_SERVER_URL=$(SEQ_SERVER_URL) \
			   --name imagegallery-idp \
			   --rm tmather99/dapr_client 

stop:
	docker stop imagegallery-idp
