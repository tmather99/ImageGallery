VERSION = v1

context:
	kubectl config use-context docker-desktop
	kubectl create namespace dapr-monitoring

dashboard:
	kubectl apply -f https://raw.githubusercontent.com/kubernetes/dashboard/v2.7.0/aio/deploy/recommended.yaml
	kubectl apply -f https://gist.githubusercontent.com/dahlsailrunner/bbd453f3bb6259b66c08a70d0908283f/raw/5727723217e2df4b65d8933adf04d009cfb0fe3f/local-dashboard-account.yml

rm-dashboard:
	kubectl delete -f https://raw.githubusercontent.com/kubernetes/dashboard/v2.7.0/aio/deploy/recommended.yaml
	kubectl delete -f https://gist.githubusercontent.com/dahlsailrunner/bbd453f3bb6259b66c08a70d0908283f/raw/5727723217e2df4b65d8933adf04d009cfb0fe3f/local-dashboard-account.yml

# kubectl apply -f https://raw.githubusercontent.com/kubernetes/ingress-nginx/controller-v1.4.0/deploy/static/provider/cloud/deploy.yaml
nginx:
	kubectl apply -f ingress-nginx-controller-v1.4.0.yml

rm-nginx:
	kubectl delete -f ingress-nginx-controller-v1.4.0.yml

seq:
	kubectl apply -f seq.yml

rm-seq:
	kubectl delete -f seq.yml

zipkin:
	kubectl apply -f zipkin.yml
	kubectl apply -f dapr/tracing.yml

rm-zipkin:
	kubectl delete -f zipkin.yml
	kubectl delete -f dapr/tracing.yml

ingress:
	kubectl apply -f imagegallery-ingress.yml
	kubectl apply -f dapr-dashboard-ingress.yml
	kubectl apply -f kd-ingress.yml

rm-ingress:
	kubectl delete -f imagegallery-ingress.yml
	kubectl delete -f dapr-dashboard-ingress.yml
	kubectl delete -f kd-ingress.yml

idp:
	kubectl apply -f imagegallery-idp.yml

rm-idp:
	kubectl delete -f imagegallery-idp.yml

api:
	kubectl apply -f imagegallery-api.yml

rm-api:
	kubectl delete -f imagegallery-api.yml

client:
	kubectl apply -f imagegallery-client.yml

rm-client:
	kubectl delete -f imagegallery-client.yml

up: idp api client

down: rm-idp rm-api rm-client

# kubectl create namespace dapr-monitoring
secrets:
	kubectl create secret tls dapr-local-tls --key="certs/dapr-local.key" --cert="certs/dapr-local.crt" -n dapr-system
	kubectl create secret tls seq-local-tls --key="certs/seq-local.key" --cert="certs/seq-local.crt"
	kubectl create secret tls zipkin-local-tls --key="certs/zipkin-local.key" --cert="certs/zipkin-local.crt"
	kubectl -n kubernetes-dashboard create secret tls kd-local-tls --key="certs/kd-local.key" --cert="certs/kd-local.crt"

rm-secrets:
	kubectl delete secret dapr-local-tls -n dapr-system
	kubectl delete secret seq-local-tls
	kubectl delete secret zipkin-local-tls
	kubectl delete secret -n kubernetes-dashboard kd-local-tls

get-token:
	kubectl -n kubernetes-dashboard create token admin-user

