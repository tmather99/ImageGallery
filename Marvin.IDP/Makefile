SEQ_SERVER_URL = http://borg.vmwuem.com:5341

seq:
	docker run --name seq.local.com -d --restart unless-stopped -e ACCEPT_EULA=Y -p 5341:80 datalust/seq:latest

dapr-dashboard:
	dapr dashboard -p 8888

zipkin-dashboard:
	powershell start-process http://localhost:9411

dapr-client-swagger:
	powershell start-process http://localhost:8083/swagger

dapr-server-swagger:
	powershell start-process http://localhost:8081/swagger

### dotnet CLI

dotnet-build:
	dotnet build -c Release

dotnet-run:
	powershell $$env:ASPNETCORE_URLS=\"https://*:5001\"; \
               $$env:ASPNETCORE_ENVIRONMENT=\"onebox\"; \
			   $$env:SEQ_SERVER_URL=\'http://seq.local.com\'; \
	      dotnet run -c Release --urls https://idp.imagegallery.com:5001

### Dapr sidecar for unit test debugging

dapr-sidecar: 
	powershell $$env:DAPR_HTTP_PORT=3500; \
		dapr run --dapr-http-port $$env:DAPR_HTTP_PORT 

### Dapr statestore

dapr-run: 
	powershell $$env:ASPNETCORE_URLS=\"https://*:5001\"; \
               $$env:ASPNETCORE_ENVIRONMENT=\"onebox\"; \
               $$env:DAPR_HTTP_PORT=3500; \
               $$env:SEQ_SERVER_URL=\'$(SEQ_SERVER_URL)\'; \
		dapr run --app-id idp \
		         --app-port 5001 \
                 --dapr-http-port $$env:DAPR_HTTP_PORT \
                 --components-path ../dapr/components \
                 --config ../dapr/config.yml \
                 -- dotnet run -c Release --urls https://idp.imagegallery.com:5001
