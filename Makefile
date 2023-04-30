REPO = tmather99
VERSION = v1

PROJS = idp \
        api \
		client \
		sql
        
build:
	docker compose build

build-device:
	cd DeviceClient && dotnet build

enroll:
	.\DeviceClient\bin\Debug\net7.0\DeviceClient.exe

tag:
	$(foreach proj,$(PROJS),docker tag imagegallery-$(proj):$(VERSION) $(REPO)/imagegallery-$(proj):$(VERSION) &)

push:
	$(foreach proj,$(PROJS),docker push $(REPO)/imagegallery-$(proj):$(VERSION) &)

up:
	docker compose up

down:
	docker compose down

clean:
	kubectl delete pvc --all 
	kubectl delete pv --all 
	docker system prune -f
	docker volume prune -f

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

redis:
	docker compose up redis-statestore

rm-redis:
	docker compose stop redis-statestore

redis-commander:
	docker compose up redis-commander

rm-redis-commander:
	docker compose stop redis-commander

rabbitmq:
	docker compose up rabbitmq

rm-rabbitmq:
	docker compose stop rabbitmq

# dotnet tool install --global dotnet-ef
ef-update:
	cd ./Marvin.IDP && dotnet ef database update
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
