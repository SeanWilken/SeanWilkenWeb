provider "digitalocean" {
  token = var.do_token
}

resource "digitalocean_container_registry" "registry" {
  name                   = var.registry_name
  subscription_tier_slug = "basic"
  region                 = var.region
}

resource "kubernetes_namespace_v1" "wilkenweb_prod" {
  metadata {
    name = "wilkenweb-prod"
  }
}

