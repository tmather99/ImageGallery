VERSION = v1

context:
	kubectl config use-context docker-desktop
	
namespace:	
	kubectl create namespace dapr-monitoring
	kubectl create namespace argocd

helm-repo:
	helm repo add bitnami https://charts.bitnami.com/bitnami
	helm repo add prometheus-community https://prometheus-community.github.io/helm-charts
	helm repo add grafana https://grafana.github.io/helm-charts
	helm repo add metrics-server https://kubernetes-sigs.github.io/metrics-server/
	helm repo update

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

# helm repo add bitnami https://charts.bitnami.com/bitnami
# helm repo update
redis:
	helm install redis bitnami/redis

rm-redis:
	helm delete redis	

redis-commander:
	kubectl apply -f redis-commander.yml

rm-redis-commander:
	kubectl delete -f redis-commander.yml

# helm repo add bitnami https://charts.bitnami.com/bitnami
rabbitmq:
	helm install rabbitmq \
	--set auth.username=admin,auth.password=admin,auth.erlangCookie=secretcookie,replicaCount=3 \
	bitnami/rabbitmq
	
rm-rabbitmq:
	helm delete rabbitmq

ingress:
	kubectl apply -f imagegallery-ingress.yml
	kubectl apply -f dapr-dashboard-ingress.yml
	kubectl apply -f kd-ingress.yml

rm-ingress:
	kubectl delete -f imagegallery-ingress.yml
	kubectl delete -f dapr-dashboard-ingress.yml
	kubectl delete -f kd-ingress.yml

dapr-install:	
	helm upgrade --install dapr dapr/dapr --version=1.9 --namespace dapr-system --create-namespace --wait

dapr-uninstall:
	helm uninstall dapr --namespace dapr-system

dapr-dashboard:
	dapr dashboard -k -p 8888

dapr-statestore:
	kubectl apply -f dapr/statestore.yml

rm-dapr-statestore:
	kubectl delete -f dapr/statestore.yml

dapr-pubsub-rabbitmq:
	kubectl apply -f dapr/pubsub-rabbitmq.yml

rm-dapr-pubsub-rabbitmq:
	kubectl delete -f dapr/pubsub-rabbitmq.yml

idp:
	kubectl apply -f imagegallery-idp.yml

rm-idp:
	kubectl delete -f imagegallery-idp.yml

api:
	kubectl apply -f imagegallery-api.yml

rm-api:
	kubectl delete -f imagegallery-api.yml

# make scale-api REPLICAS=2
REPLICAS = 1

scale-api:
	kubectl scale deployment imagegallery-api-app --replicas=$(REPLICAS)

client:
	kubectl apply -f imagegallery-client.yml

rm-client:
	kubectl delete -f imagegallery-client.yml

sql:
	kubectl apply -f imagegallery-sql.yml

rm-sql:
	kubectl delete -f imagegallery-sql.yml

sqlpad:
	kubectl apply -f sqlpad.yml

rm-sqlpad:
	kubectl delete -f sqlpad.yml

up: sql idp api client

down: rm-sql rm-idp rm-api rm-client

# kubectl create namespace dapr-monitoring
secrets:
	kubectl create secret tls dapr-local-tls --key="certs/dapr-local.key" --cert="certs/dapr-local.crt" -n dapr-system
	kubectl create secret tls seq-local-tls --key="certs/seq-local.key" --cert="certs/seq-local.crt"
	kubectl create secret tls zipkin-local-tls --key="certs/zipkin-local.key" --cert="certs/zipkin-local.crt"
	kubectl create secret tls redis-local-tls --key="certs/redis-local.key" --cert="certs/redis-local.crt"
	kubectl create secret tls rabbitmq-local-tls --key="certs/rabbitmq-local.key" --cert="certs/rabbitmq-local.crt"
	kubectl create secret tls sqlpad-local-tls --key="certs/sqlpad-local.key" --cert="certs/sqlpad-local.crt"
	kubectl -n kubernetes-dashboard create secret tls kd-local-tls --key="certs/kd-local.key" --cert="certs/kd-local.crt"
	kubectl -n argocd create secret tls argocd-local-tls --key="certs/argocd-local.key" --cert="certs/argocd-local.crt"
	kubectl -n argocd create secret tls argocd-server-tls --key="certs/argocd-local.key" --cert="certs/argocd-local.crt"

rm-secrets:
	kubectl delete secret dapr-local-tls -n dapr-system
	kubectl delete secret seq-local-tls
	kubectl delete secret zipkin-local-tls
	kubectl delete secret redis-local-tls
	kubectl delete secret rabbitmq-local-tls
	kubectl delete secret sqlpad-local-tls
	kubectl delete secret -n kubernetes-dashboard kd-local-tls
	kubectl delete secret -n argocd argocd-local-tls
	kubectl delete secret -n argocd argocd-server-tls

get-token:
	kubectl -n kubernetes-dashboard create token admin-user

# kubectl create namespace argocd
argocd:
	kubectl apply -n argocd -f argocd-install.yml

rm-argocd:
	kubectl delete -n argocd -f argocd-install.yml

argocd-ingress:
	kubectl apply -f argocd-ingress.yml

rm-argocd-ingress:
	kubectl delete -f argocd-ingress.yml

argocd-password:
	kubectl -n argocd get secret argocd-initial-admin-secret -o jsonpath="{.data.password}" | base64 -d

argocd-application:
	kubectl apply -f argocd-application.yml

rm-argocd-application:
	kubectl delete -f argocd-application.yml
