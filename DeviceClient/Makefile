API_PORT = 8083
API_SERVER = flash.vmwuem.com
SEQ_SERVER_URL = http://flash.vmwuem.com:5341
VERSION = v1

dotnet-build:
	dotnet build -c Release

dotnet-run:
	powershell $$env:ASPNETCORE_URLS=\"http://*:8083\"; \
               $$env:ASPNETCORE_ENVIRONMENT=\"Development\"; \
			   $$env:SEQ_SERVER_URL=\'http://seq.local.com\'; \
	      dotnet run -c Release --urls http://localhost:8083

### Dapr statestore

dapr-run: 
	powershell $$env:ASPNETCORE_URLS=\"http://*:8083\"; \
               $$env:ASPNETCORE_ENVIRONMENT=\"Development\"; \
               $$env:SECRET_STORE_NAME=\"secretstore\"; \
               $$env:DAPR_HTTP_PORT=3500; \
               $$env:SEQ_SERVER_URL=\'$(SEQ_SERVER_URL)\'; \
		dapr run --app-id dapr-client \
		         --app-port 8083 \
                 --dapr-http-port $$env:DAPR_HTTP_PORT \
                 --components-path ../dapr/components \
                 --config ../dapr/config.yml \
                 -- dotnet run -c Release --urls http://localhost:8083
