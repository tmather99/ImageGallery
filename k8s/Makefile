VERSION = v1
CERT_TLS = local-tls.yml

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

init: \
	ns \
	helm-repos \
	nginx

ns:	
	kubectl apply -f namespaces/dapr-monitoring.yml
	kubectl apply -f namespaces/argocd.yml
	kubectl apply -f namespaces/kubernetes-dashboard.yml

rm-ns:	
	-kubectl delete -f namespaces/dapr-monitoring.yml
	-kubectl delete -f namespaces/argocd.yml

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
	-kubectl delete -f https://raw.githubusercontent.com/kubernetes/dashboard/v2.7.0/aio/deploy/recommended.yaml
	-kubectl delete -f https://gist.githubusercontent.com/dahlsailrunner/bbd453f3bb6259b66c08a70d0908283f/raw/5727723217e2df4b65d8933adf04d009cfb0fe3f/local-dashboard-account.yml

kd-install:
	helm install kubernetes-dashboard kubernetes-dashboard/kubernetes-dashboard

kd-uninstall:
	helm delete kubernetes-dashboard

kd-token:
	-kubectl -n kubernetes-dashboard create token admin-user

seq:
	kubectl apply -f utils/seq.yml

rm-seq:
	-kubectl delete -f utils/seq.yml

zipkin: dapr-tracing
	kubectl apply -f utils/zipkin.yml

rm-zipkin: rm-dapr-tracing
	-kubectl delete -f utils/zipkin.yml

redis:
	helm install redis bitnami/redis

rm-redis:
	-helm delete redis	

redis-commander:
	kubectl apply -f utils/redis-commander.yml

rm-redis-commander:
	-kubectl delete -f utils/redis-commander.yml

rabbitmq:
	helm install rabbitmq \
	--set auth.username=admin,auth.password=admin,auth.erlangCookie=secretcookie,replicaCount=3 \
	bitnami/rabbitmq
	
rm-rabbitmq:
	-helm delete rabbitmq

ingress:
	kubectl apply -f ingress/kd.yml
	kubectl apply -f ingress/dapr-dashboard.yml
	kubectl apply -f ingress/imagegallery.yml

rm-ingress:
	-kubectl delete -f ingress/kd.yml
	-kubectl delete -f ingress/dapr-dashboard.yml
	-kubectl delete -f ingress/imagegallery.yml

dapr-install:	
	helm upgrade --install dapr dapr/dapr --version=1.9 --namespace dapr-system --create-namespace --wait

dapr-uninstall:
	-helm uninstall dapr --namespace dapr-system

dapr-statestore:
	kubectl apply -f dapr/statestore.yml

rm-dapr-statestore:
	-kubectl delete -f dapr/statestore.yml

dapr-pubsub-rabbitmq:
	kubectl apply -f dapr/pubsub-rabbitmq.yml

rm-dapr-pubsub-rabbitmq:
	-kubectl delete -f dapr/pubsub-rabbitmq.yml

dapr-tracing:
	kubectl apply -f dapr/tracing.yml

rm-dapr-tracing:
	-kubectl delete -f dapr/tracing.yml

dapr: dapr-statestore \
      dapr-pubsub-rabbitmq \
	  dapr-tracing

rm-dapr: rm-dapr-statestore \
         rm-dapr-pubsub-rabbitmq \
	     rm-dapr-tracing

idp:
	kubectl apply -f imagegallery/idp.yml

rm-idp:
	-kubectl delete -f imagegallery/idp.yml

api:
	kubectl apply -f imagegallery/api.yml

rm-api:
	-kubectl delete -f imagegallery/api.yml

# make scale-api REPLICAS=2
REPLICAS = 1

scale-api:
	kubectl scale deployment imagegallery-api-app --replicas=$(REPLICAS)

client:
	kubectl apply -f imagegallery/client.yml

rm-client:
	-kubectl delete -f imagegallery/client.yml

sql:
	kubectl apply -f sql/imagegallery.yml

rm-sql:
	-kubectl delete -f sql/imagegallery.yml

sqlpad:
	kubectl apply -f sql/sqlpad.yml

rm-sqlpad:
	-kubectl delete -f sql/sqlpad.yml

up: dapr sql idp api client

down: rm-sql rm-idp rm-api rm-client

down-all: \
	down \
	rm-sqlpad \
	rm-redis \
	rm-redis-commander \
	rm-rabbitmq \
	rm-secrets \
	rm-ingress \
	rm-kd \
	rm-nginx \
	dapr-uninstall

# dotnet tool install --global dotnet-ef
ef-update:
	cd ../Marvin.IDP && dotnet ef database update
	cd ../ImageGallery.API && dotnet ef database update



# kubectl apply -f https://raw.githubusercontent.com/kubernetes/ingress-nginx/controller-v1.4.0/deploy/static/provider/cloud/deploy.yaml
nginx:
	kubectl apply -f ingress/nginx-controller-v1.4.0.yml

rm-nginx:
	-kubectl delete -f ingress/nginx-controller-v1.4.0.yml

# kubectl create secret tls imagegallery-api-local-tls --key="../https/api-imagegallery.key" --cert="../https/api-imagegallery.crt" --dry-run=client -o yaml
apps-secrets:
	kubectl apply -f tls-secrets/imagegallery-idp-$(CERT_TLS)
	kubectl apply -f tls-secrets/imagegallery-api-$(CERT_TLS)
	kubectl apply -f tls-secrets/imagegallery-client-$(CERT_TLS)

rm-apps-secrets:
	kubectl delete -f tls-secrets/imagegallery-idp-$(CERT_TLS)
	kubectl delete -f tls-secrets/imagegallery-api-$(CERT_TLS)
	kubectl delete -f tls-secrets/imagegallery-client-$(CERT_TLS)

# kubectl create secret tls dapr-local-tls --key="certs/dapr-local.key" --cert="certs/dapr-local.crt" -n dapr-system --dry-run=client -o yaml
secrets: apps-secrets
	kubectl apply -f tls-secrets/dapr-$(CERT_TLS)
	kubectl apply -f tls-secrets/grafana-$(CERT_TLS)
	kubectl apply -f tls-secrets/rabbitmq-$(CERT_TLS)
	kubectl apply -f tls-secrets/redis-$(CERT_TLS)
	kubectl apply -f tls-secrets/seq-$(CERT_TLS)
	kubectl apply -f tls-secrets/sqlpad-$(CERT_TLS)
	kubectl apply -f tls-secrets/zipkin-$(CERT_TLS)

# kubectl delete secret dapr-local-tls -n dapr-system
rm-secrets: rm-apps-secrets
	kubectl delete -f tls-secrets/dapr-$(CERT_TLS)
	kubectl delete -f tls-secrets/grafana-$(CERT_TLS)
	kubectl delete -f tls-secrets/rabbitmq-$(CERT_TLS)
	kubectl delete -f tls-secrets/redis-$(CERT_TLS)
	kubectl delete -f tls-secrets/seq-$(CERT_TLS)
	kubectl delete -f tls-secrets/sqlpad-$(CERT_TLS)
	kubectl delete -f tls-secrets/zipkin-$(CERT_TLS)

argocd-ingress:
	kubectl apply -f ingress/argocd.yml

rm-argocd-ingress:
	kubectl delete -f ingress/argocd.yml

argocd-secrets:
	kubectl apply -f tls-secrets/argocd-$(CERT_TLS)
	kubectl apply -f tls-secrets/argocd-server-tls.yml

rm-argocd-secrets:
	kubectl delete -f tls-secrets/argocd-$(CERT_TLS)
	kubectl delete -f tls-secrets/argocd-server-tls.yml

argocd-install:
	kubectl apply -n argocd -f argocd/install.yml

argocd-uninstall:
	kubectl delete -n argocd -f argocd/install.yml

argocd-password:
	kubectl -n argocd get secret argocd-initial-admin-secret -o jsonpath="{.data.password}" | base64 -d

imagegallery-repo-secret:
	kubectl apply -f argocd/imagegallery-repo-secret.yml

rm-imagegallery-repo-secret:
	kubectl delete -f argocd/imagegallery-repo-secret.yml




argocd-deploy-dapr:
	kubectl apply -f argocd/dapr.yml
	
argocd-deploy-redis:
	kubectl apply -f argocd/redis.yml

argocd-deploy-rabbitmq:
	kubectl apply -f argocd/rabbitmq.yml

argocd-deploy-sql:
	kubectl apply -f argocd/sql.yml

argocd-deploy-utils:
	kubectl apply -f argocd/utils.yml

argocd-deploy-apps:
	kubectl apply -f argocd/apps.yml


argocd-undeploy-dapr:
	kubectl patch app -n argocd dapr  -p '{"metadata": {"finalizers": ["resources-finalizer.argocd.argoproj.io"]}}' --type merge
	kubectl delete app -n argocd dapr
	
argocd-undeploy-redis:
	kubectl patch app -n argocd redis  -p '{"metadata": {"finalizers": ["resources-finalizer.argocd.argoproj.io"]}}' --type merge
	kubectl delete app -n argocd redis

argocd-undeploy-rabbitmq:
	kubectl patch app -n argocd rabbitmq  -p '{"metadata": {"finalizers": ["resources-finalizer.argocd.argoproj.io"]}}' --type merge
	kubectl delete app -n argocd rabbitmq

argocd-undeploy-sql:
	kubectl patch app -n argocd sql  -p '{"metadata": {"finalizers": ["resources-finalizer.argocd.argoproj.io"]}}' --type merge
	kubectl delete app -n argocd sql

argocd-undeploy-utils:
	kubectl patch app -n argocd utils  -p '{"metadata": {"finalizers": ["resources-finalizer.argocd.argoproj.io"]}}' --type merge
	kubectl delete app -n argocd utils

argocd-undeploy-apps:
	kubectl patch app -n argocd apps  -p '{"metadata": {"finalizers": ["resources-finalizer.argocd.argoproj.io"]}}' --type merge
	kubectl delete app -n argocd apps


argocd-deploy: \
	argocd-deploy-dapr \
	argocd-deploy-redis \
	argocd-deploy-rabbitmq \
	argocd-deploy-utils \
	argocd-deploy-apps

argocd-undeploy:
	argocd-undeploy-dapr \
	argocd-undeploy-redis \
	argocd-undeploy-rabbitmq \
	argocd-undeploy-utils \
	argocd-undeploy-apps
