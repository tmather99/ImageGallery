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

build-sql:
	docker compose build globosql

sql:
	docker compose up globosql

rm-sql:
	docker compose stop globosql

# dotnet tool install --global dotnet-ef
ef-update:
	cd ./Marvin.IDP && dotnet ef database update
	cd ./ImageGallery.API && dotnet ef database update
