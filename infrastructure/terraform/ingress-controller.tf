resource "helm_release" "ingress_nginx" {
  depends_on = [ digitalocean_kubernetes_cluster.cluster ]
  name             = "ingress-nginx"
  repository       = "https://kubernetes.github.io/ingress-nginx"
  chart            = "ingress-nginx"
  version          = "4.10.0" # or latest stable
  namespace        = "ingress-nginx"
  create_namespace = true

  wait    = true
  timeout = 300
}