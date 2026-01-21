resource "digitalocean_database_cluster" "mongo" {
  name       = var.mongo_name
  engine     = "mongodb"
  version    = var.mongo_version
  region     = var.region
  size       = var.mongo_size
  node_count = 1

  # This turns on private networking for managed DB
  private_network_uuid = digitalocean_kubernetes_cluster.cluster.vpc_uuid
}

resource "digitalocean_database_user" "app_user" {
  cluster_id = digitalocean_database_cluster.mongo.id
  name       = "app"
}

resource "digitalocean_database_db" "db" {
  cluster_id = digitalocean_database_cluster.mongo.id
  name       = var.mongo_db
}
