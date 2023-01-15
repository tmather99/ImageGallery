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
	cd ./Marvin.IDP && dotnet ef database update && cd ..
	cd ./ImageGallery.API && dotnet ef database update

dapr-sidecar: 
	powershell $$env:DAPR_HTTP_PORT=3500; \
		dapr run --dapr-http-port $$env:DAPR_HTTP_PORT 

### Dapr statestore

dapr-idp: 
	powershell $$env:ASPNETCORE_URLS=\"http://*:5001\"; \
               $$env:ASPNETCORE_ENVIRONMENT=\"onebox\"; \
               $$env:SECRET_STORE_NAME=\"secretstore\"; \
               $$env:DAPR_HTTP_PORT=3501; \
               $$env:SEQ_SERVER_URL=\'$(SEQ_SERVER_URL)\'; \
		dapr run --app-id imagegallery-idp \
		         --app-port 5001 \
                 --dapr-http-port $$env:DAPR_HTTP_PORT \
                 --components-path ./dapr/components \
                 --config ./dapr/config.yml \
                 -- dotnet run -c Debug --urls http://localhost:7075

dapr-api: 
	powershell $$env:ASPNETCORE_URLS=\"http://*:7075\"; \
               $$env:ASPNETCORE_ENVIRONMENT=\"onebox\"; \
               $$env:SECRET_STORE_NAME=\"secretstore\"; \
               $$env:DAPR_HTTP_PORT=3502; \
               $$env:SEQ_SERVER_URL=\'$(SEQ_SERVER_URL)\'; \
		dapr run --app-id imagegallery-api \
		         --app-port 7075 \
                 --dapr-http-port $$env:DAPR_HTTP_PORT \
                 --components-path ./dapr/components \
                 --config ./dapr/config.yml \
                 -- dotnet run -c Debug --urls http://localhost:7075
