
### dotnet CLI

dotnet-build:
	dotnet build -c Debug

dotnet-run:
	powershell $$env:ASPNETCORE_URLS=\"https://*:7075\"; \
               $$env:ASPNETCORE_ENVIRONMENT=\"onebox\"; \
			   $$env:SEQ_SERVER_URL=\'http://seq.local.com\'; \
	      dotnet run -c Debug --urls https://api.assistdevtest.com:7075

### Dapr sidecar for unit test debugging

dapr-sidecar: 
	powershell $$env:DAPR_HTTP_PORT=3500; \
		dapr run --dapr-http-port $$env:DAPR_HTTP_PORT 

### Dapr statestore

dapr-run: 
	powershell $$env:ASPNETCORE_URLS=\"https://*:7075\"; \
               $$env:ASPNETCORE_ENVIRONMENT=\"onebox\"; \
               $$env:DAPR_HTTP_PORT=3501; \
               $$env:SEQ_SERVER_URL=\'$(SEQ_SERVER_URL)\'; \
		dapr run --app-id api \
                 --dapr-http-port $$env:DAPR_HTTP_PORT \
                 --components-path ../dapr/components \
                 --config ../dapr/config.yml \
                 -- dotnet run -c Debug --urls https://api.assistdevtest.com:7075

ef-update:
	dotnet ef database update

