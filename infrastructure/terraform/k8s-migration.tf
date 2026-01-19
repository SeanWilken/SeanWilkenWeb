resource "local_file" "patch_image" {
  content = templatefile(
    "../k8s/overlays/production/patch-image.yaml.tmpl",
    { tag = var.tag }
  )

  filename = "../k8s/overlays/production/patch-image.yaml"
}