variable "do_token" { type = string }



variable "region" { 
    type = string 
    default = "nyc3" 
}

variable "cluster_name" { 
    type = string
    default = "wilkenweb-prod"
}

variable "k8s_version" { 
    type = string
    default = "1.34.1-do.2"
}

# NOTE: verify size slug via `doctl kubernetes options sizes`
variable "node_pool_name" { 
    type = string 
    default = "wilken-web-pool"
}
variable "node_size" { 
    type = string 
    default = "s-1vcpu-2gb-amd" 
}
variable "auto_scale" { 
    type = bool 
    default = true
}

variable "min_nodes" { 
    type = number 
    default = 1 
}
variable "max_nodes" { 
    type = number 
    default = 3 
}

variable "registry_name" { 
    type = string
    default = "wilkenweb"
}

# Managed Mongo
variable "mongo_name" { 
    type = string 
    default = "xeroeffort-prod-mongo"
}
variable "mongo_db" { 
    type = string 
    default = "xeroeffort"
}
variable "mongo_size" { 
    type = string
    default = "db-s-1vcpu-2gb"
} # verify via doctl databases options

variable "mongo_version" { 
    type = string
    default = "8"
}

variable "kubeconfig_path" {
  type = string
}

variable "tag" {
  type = string
  description = "Git SHA or version tag for container images"
}