# For the Ingress Version

# Apply Local Kubernetes Stuff (From repo root) (Loca version doesn't deploy a load balancer)

kubectl apply -f k8s-ingress

# Delete local Kubernetes Stuff (From repo root)

kubectl delete -f k8s-ingress

# Port Forward (Without Ingress Service) (Copy full Web Deployment Name and replace octopetshop-web-deployment-{GUID} )

kubectl get pods -o wide
kubectl port-forward octopetshop-web-deployment-{GUID}  5000:80

# Install Helm (For Ingress Controller)

choco install kubernetes-helm

# Install Helm Ingress Controller (No need for port forward, but remember to make sure iis isn't using port 80!)

helm upgrade --install ingress-nginx ingress-nginx  --repo https://kubernetes.github.io/ingress-nginx  --namespace ingress-nginx --create-namespace

# Watch the Ingress Controller for "localhost" to be provisioned

kubectl get ingress --watch