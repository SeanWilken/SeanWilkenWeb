output "kubeconfig" {
  value     = digitalocean_kubernetes_cluster.cluster.kube_config[0].raw_config
  sensitive = true
}

output "registry_endpoint" {
  value = digitalocean_container_registry.registry.endpoint
}

output "mongo_uri_private" {
  value     = digitalocean_database_cluster.mongo.private_uri
  sensitive = true
}

output "mongo_user" {
  value = digitalocean_database_user.app_user.name
}
