resource "digitalocean_kubernetes_cluster" "cluster" {
  name    = var.cluster_name
  region  = var.region
  version = var.k8s_version

  node_pool {
    name       = var.node_pool_name
    size       = var.node_size
    auto_scale  = var.auto_scale
    min_nodes   = var.min_nodes
    max_nodes   = var.max_nodes
  }
}
