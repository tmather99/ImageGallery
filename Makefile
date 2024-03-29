REPO = tmather99
VERSION = v10

PROJS = idp \
        api \
        client \
        sql
        
build:
	docker compose build

build-idp:
	docker compose build idp

build-device:
	cd DeviceClient && dotnet build

enroll:
ifeq ($(OS),Windows_NT)
	.\DeviceClient\bin\Debug\net8.0\DeviceClient.exe
else
	./DeviceClient/bin/Debug/net8.0/DeviceClient
endif

tag-idp:
	docker tag imagegallery-idp:$(VERSION) $(REPO)/imagegallery-idp:$(VERSION)

tag-api:
	docker tag imagegallery-api:$(VERSION) $(REPO)/imagegallery-api:$(VERSION)

tag-client:
	docker tag imagegallery-client:$(VERSION) $(REPO)/imagegallery-client:$(VERSION)

tag-sql:
	docker tag imagegallery-sql:$(VERSION) $(REPO)/imagegallery-sql:$(VERSION)

push-idp: tag-idp
	docker push $(REPO)/imagegallery-idp:$(VERSION)

push-api: tag-api
	docker push $(REPO)/imagegallery-api:$(VERSION)

push-client: tag-client
	docker push $(REPO)/imagegallery-client:$(VERSION)

push-sql: tag-sql
	docker push $(REPO)/imagegallery-sql:$(VERSION)

push: \
	push-idp \
	push-api \
	push-client \
	push-sql

up:
	docker compose up

down:
	docker compose down

#	kubectl delete pvc --all 
#	kubectl delete pv --all 
clean:
	docker image  prune -a -f
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
	cd ./Marvin.IDP && dotnet ef database update --context IdentityDbContext
	cd ./Marvin.IDP && dotnet ef database update --context PersistedGrantDbContext
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
