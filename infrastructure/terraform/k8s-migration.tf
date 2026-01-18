 resource "kubectl_manifest" "migration_job" {
  yaml_body = templatefile("${path.module}/k8s/base/migration-job.yaml", {
    tag = var.tag
  })
}