variable "do_token" { type = string }

variable "region" { type = string default = "nyc3" }

variable "cluster_name" { type = string default = "wilkenweb-prod" }
variable "k8s_version" { type = string default = "1.30.2-do.0" }

# NOTE: verify size slug via `doctl kubernetes options sizes`
variable "node_size" { type = string default = "s-1vcpu-2gb-amd" }
variable "node_count" { type = number default = 1 }

variable "registry_name" { type = string default = "wilkenweb" }

# Managed Mongo
variable "mongo_name" { type = string default = "xeroeffort-prod-mongo" }
variable "mongo_size" { type = string default = "db-s-1vcpu-2gb" } # verify via doctl databases options
variable "mongo_version" { type = string default = "8" }
