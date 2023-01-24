VERSION = v1

context:
	kubectl config use-context kubernetes-admin@kubernetes
	kubectl create namespace dapr-monitoring

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

ingress:
	kubectl apply -f imagegallery-ingress.yml 
	kubectl apply -f dapr-dashboard-ingress.yml
	kubectl apply -f kd-ingress.yml
	kubectl apply -f grafana-ingress.yml

rm-ingress:
	kubectl delete -f imagegallery-ingress.yml
	kubectl delete -f dapr-dashboard-ingress.yml
	kubectl delete -f kd-ingress.yml
	kubectl delete -f grafana-ingress.yml

seq:
	kubectl apply -f seq.yml

rm-seq:
	kubectl delete -f seq.yml

dapr-dashboard:
	dapr dashboard -k -p 8888

pv-pvc:
	kubectl apply -f pv-pvc.yml

# kubectl patch pv  storage-dapr-prom-alertmanager -p '{"metadata":{"finalizers":null}}'
# kubectl patch pvc storage-dapr-prom-alertmanager-0 -n dapr-monitoring -p '{"metadata":{"finalizers":null}}'
rm-pv-pvc:
	kubectl delete -f pv-pvc.yml

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

# helm repo add prometheus-community https://prometheus-community.github.io/helm-charts
# helm repo update
# kubectl create namespace dapr-monitoring
prometheus:
	helm install dapr-prom prometheus-community/prometheus -n dapr-monitoring \
		--set nodeExporter.hostRootfs=false \
		--set alertmanager.persistentVolume.enable=false \
		--set pushgateway.persistentVolume.enabled=false \
		--set server.persistentVolume.enabled=false

rm-prometheus::
	helm delete dapr-prom -n dapr-monitoring 

# helm repo add grafana https://grafana.github.io/helm-charts
# helm repo update
# https://github.com/RicardoNiepel/dapr-docs/blob/master/howto/setup-monitoring-tools/setup-prometheus-grafana.md
# http://dapr-prom-prometheus-server.dapr-monitoring
grafana:
	helm install grafana grafana/grafana -n dapr-monitoring

rm-grafana:
	helm delete grafana -n dapr-monitoring

grafana-password:
	kubectl get secret --namespace dapr-monitoring grafana -o jsonpath="{.data.admin-password}" | base64 --decode ; echo

zipkin:
	kubectl apply -f zipkin.yml
	kubectl apply -f dapr/tracing.yml

rm-zipkin:
	kubectl delete -f zipkin.yml
	kubectl delete -f dapr/tracing.yml

# helm repo add metrics-server https://kubernetes-sigs.github.io/metrics-server/
# helm repo update     
metrics-server:
	helm install --set 'args={--kubelet-insecure-tls}' -n kube-system metrics metrics-server/metrics-server

rm-metrics-server:
	helm delete metrics -n kube-system 

dapr-pubsub-rabbitmq:
	kubectl apply -f dapr/pubsub-rabbitmq.yml

rm-dapr-pubsub-rabbitmq:
	kubectl delete -f dapr/pubsub-rabbitmq.yml

dapr-statestore:
	kubectl apply -f dapr/statestore.yml

rm-dapr-statestore:
	kubectl delete -f dapr/statestore.yml

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

sql:
	kubectl apply -f imagegallery-sql.yml

rm-sql:
	kubectl delete -f imagegallery-sql.yml

up: idp api client

down: rm-idp rm-api rm-client

secrets:
	kubectl create secret tls seq-onprem-tls --key="certs/seq-onprem.key" --cert="certs/seq-onprem.crt"
	kubectl create secret tls zipkin-onprem-tls --key="certs/zipkin-onprem.key" --cert="certs/zipkin-onprem.crt"
	kubectl create secret tls redis-onprem-tls --key="certs/redis-onprem.key" --cert="certs/redis-onprem.crt"
	kubectl create secret tls rabbitmq-onprem-tls --key="certs/rabbitmq-onprem.key" --cert="certs/rabbitmq-onprem.crt"
	kubectl create secret tls grafana-onprem-tls --key="certs/grafana-onprem.key" --cert="certs/grafana-onprem.crt" -n dapr-monitoring
	kubectl -n kubernetes-dashboard create secret tls kd-onprem-tls --key="certs/kd-onprem.key" --cert="certs/kd-onprem.crt"

rm-secrets:
	kubectl delete secret seq-onprem-tls
	kubectl delete secret zipkin-onprem-tls
	kubectl delete secret redis-onprem-tls
	kubectl delete secret rabbitmq-onprem-tls
	kubectl delete secret grafana-onprem-tls -n dapr-monitoring
	kubectl delete secret -n kubernetes-dashboard kd-onprem-tls

kd-token:
	kubectl -n kubernetes-dashboard create token admin-user
