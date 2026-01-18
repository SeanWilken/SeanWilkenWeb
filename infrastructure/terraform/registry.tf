provider "digitalocean" {
  token = var.do_token
}

resource "digitalocean_container_registry" "registry" {
  name                   = var.registry_name
  subscription_tier_slug = "basic"
  region                 = var.region
}
