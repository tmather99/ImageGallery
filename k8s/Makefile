VERSION = v1

.PHONY: ingress sql

login:
	docker login

get-contexts:
	kubectl config get-contexts

use-context-desktop:
	kubectl config use-context docker-desktop

use-context-onprem:
	kubectl config use-context kubernetes-admin@kubernetes

context: use-context-desktop

clean:
	kubectl delete pvc --all 
	kubectl delete pv --all 
	docker system prune -f
	docker volume prune -f

ns:	
	kubectl create namespace dapr-monitoring
	kubectl create namespace argocd

helm-repos:
	helm repo add bitnami https://charts.bitnami.com/bitnami
	helm repo add prometheus-community https://prometheus-community.github.io/helm-charts
	helm repo add grafana https://grafana.github.io/helm-charts
	helm repo add metrics-server https://kubernetes-sigs.github.io/metrics-server/
	helm repo add kubernetes-dashboard https://kubernetes.github.io/dashboard/
	helm repo update

kd:
	kubectl apply -f https://raw.githubusercontent.com/kubernetes/dashboard/v2.7.0/aio/deploy/recommended.yaml
	kubectl apply -f https://gist.githubusercontent.com/dahlsailrunner/bbd453f3bb6259b66c08a70d0908283f/raw/5727723217e2df4b65d8933adf04d009cfb0fe3f/local-dashboard-account.yml

rm-kd:
	kubectl delete -f https://raw.githubusercontent.com/kubernetes/dashboard/v2.7.0/aio/deploy/recommended.yaml
	kubectl delete -f https://gist.githubusercontent.com/dahlsailrunner/bbd453f3bb6259b66c08a70d0908283f/raw/5727723217e2df4b65d8933adf04d009cfb0fe3f/local-dashboard-account.yml

kd-install:
	helm install kubernetes-dashboard kubernetes-dashboard/kubernetes-dashboard

kd-uninstall:
	helm delete kubernetes-dashboard

# kubectl apply -f https://raw.githubusercontent.com/kubernetes/ingress-nginx/controller-v1.4.0/deploy/static/provider/cloud/deploy.yaml
nginx:
	kubectl apply -f ingress/nginx-controller-v1.4.0.yml

rm-nginx:
	kubectl delete -f ingress/nginx-controller-v1.4.0.yml

seq:
	kubectl apply -f utils/seq.yml

rm-seq:
	kubectl delete -f utils/seq.yml

zipkin:
	kubectl apply -f utils/zipkin.yml
	kubectl apply -f dapr/tracing.yml

rm-zipkin:
	kubectl delete -f utils/zipkin.yml
	kubectl delete -f dapr/tracing.yml

# helm repo add bitnami https://charts.bitnami.com/bitnami
# helm repo update
redis:
	helm install redis bitnami/redis

rm-redis:
	helm delete redis	

redis-commander:
	kubectl apply -f utils/redis-commander.yml

rm-redis-commander:
	kubectl delete -f utils/redis-commander.yml

# helm repo add bitnami https://charts.bitnami.com/bitnami
rabbitmq:
	helm install rabbitmq \
	--set auth.username=admin,auth.password=admin,auth.erlangCookie=secretcookie,replicaCount=3 \
	bitnami/rabbitmq
	
rm-rabbitmq:
	helm delete rabbitmq

ingress:
	kubectl apply -f ingress/imagegallery.yml
	kubectl apply -f ingress/dapr-dashboard.yml
	kubectl apply -f ingress/kd.yml

rm-ingress:
	kubectl delete -f ingress/imagegallery.yml
	kubectl delete -f ingress/dapr-dashboard.yml
	kubectl delete -f ingress/kd.yml

dapr-install:	
	helm upgrade --install dapr dapr/dapr --version=1.9 --namespace dapr-system --create-namespace --wait

dapr-uninstall:
	helm uninstall dapr --namespace dapr-system

dapr-statestore:
	kubectl apply -f dapr/statestore.yml

rm-dapr-statestore:
	kubectl delete -f dapr/statestore.yml

dapr-pubsub-rabbitmq:
	kubectl apply -f dapr/pubsub-rabbitmq.yml

rm-dapr-pubsub-rabbitmq:
	kubectl delete -f dapr/pubsub-rabbitmq.yml

idp:
	kubectl apply -f apps/imagegallery-idp.yml

rm-idp:
	kubectl delete -f apps/imagegallery-idp.yml

api:
	kubectl apply -f apps/imagegallery-api.yml

rm-api:
	kubectl delete -f apps/imagegallery-api.yml

# make scale-api REPLICAS=2
REPLICAS = 1

scale-api:
	kubectl scale deployment imagegallery-api-app --replicas=$(REPLICAS)

client:
	kubectl apply -f apps/imagegallery-client.yml

rm-client:
	kubectl delete -f apps/imagegallery-client.yml

sql:
	kubectl apply -f sql/imagegallery-sql.yml

rm-sql:
	kubectl delete -f sql/imagegallery-sql.yml

sqlpad:
	kubectl apply -f sql/sqlpad.yml

rm-sqlpad:
	kubectl delete -f sql/sqlpad.yml

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
	kubectl apply -n argocd -f argocd/install.yml

rm-argocd:
	kubectl delete -n argocd -f argocd/install.yml

argocd-ingress:
	kubectl apply -f ingress/argocd.yml

rm-argocd-ingress:
	kubectl delete -f ingress/argocd.yml

argocd-password:
	kubectl -n argocd get secret argocd-initial-admin-secret -o jsonpath="{.data.password}" | base64 -d

