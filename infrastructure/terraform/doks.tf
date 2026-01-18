resource "digitalocean_kubernetes_cluster" "cluster" {
  name    = var.cluster_name
  region  = var.region
  version = var.k8s_version

  node_pool {
    name       = "default-pool"
    size       = var.node_size
    node_count = var.node_count
  }
}
