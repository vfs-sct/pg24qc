using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace EditorTools.Editor
{
    public class MeshCombineImporter : AssetPostprocessor
    {
        private static string _collisionPrefix = "ucx";

        private class FilterRendererPair
        {
            public MeshFilter MeshFilter;
            public MeshRenderer MeshRenderer;

            public FilterRendererPair(MeshFilter meshFilter, MeshRenderer meshRenderer)
            {
                MeshFilter = meshFilter;
                MeshRenderer = meshRenderer;
            }
        }

        private void OnPostprocessModel(GameObject gameObject)
        {
            ModelImporter modelImporter = assetImporter as ModelImporter;
            Transform root = gameObject.transform;

            bool combineFlag = assetImporter.userData.Contains("meshCombine=true");
            SkinnedMeshRenderer[] skinnedMeshRenderers = gameObject.GetComponentsInChildren<SkinnedMeshRenderer>();
            bool isStaticMesh = skinnedMeshRenderers.Length == 0 && !gameObject.name.Contains("@");

            if(isStaticMesh && combineFlag)
            {
                Debug.Log($"Combining model meshes during import: {gameObject.name}", gameObject);

                MeshFilter[] meshFilters = gameObject.GetComponentsInChildren<MeshFilter>().Where(x => !x.name.ToLower().Contains(_collisionPrefix)).ToArray();
                
                if (meshFilters.Length >= 2)
                {
                    MeshRenderer[] meshRenderers = gameObject.GetComponentsInChildren<MeshRenderer>().Where(x => !x.name.ToLower().Contains(_collisionPrefix)).ToArray();
                    Dictionary<Material, List<FilterRendererPair>> materialSortedMeshes = new Dictionary<Material, List<FilterRendererPair>>();

                    for (int i = 0; i < meshRenderers.Length; i++)
                    {
                        MeshRenderer meshRenderer = meshRenderers[i];
                        meshRenderer.transform.SetParent(root, true);

                        if (meshRenderer.sharedMaterials.Length > 1) continue;
                        MeshFilter meshFilter = meshFilters[i];
                        Material material = meshRenderer.sharedMaterial;
                        FilterRendererPair pair = new FilterRendererPair(meshFilter, meshRenderer);
                        if (!materialSortedMeshes.ContainsKey(material)) materialSortedMeshes.Add(material, new List<FilterRendererPair>());
                        materialSortedMeshes[material].Add(pair);
                    }

                    foreach (var kvp in materialSortedMeshes)
                    {
                        MeshFilter combinedFilter = kvp.Value[0].MeshFilter;

                        CombineInstance[] combine = new CombineInstance[kvp.Value.Count];
                        for (int i = 0; i < kvp.Value.Count; i++)
                        {
                            combine[i].mesh = kvp.Value[i].MeshFilter.sharedMesh;
                            Matrix4x4 matrix = root.worldToLocalMatrix * kvp.Value[i].MeshFilter.transform.localToWorldMatrix;
                            combine[i].transform = matrix;
                        }

                        Mesh combinedMesh = new Mesh();
                        combinedMesh.CombineMeshes(combine, true, true, false);
                        combinedMesh.RecalculateBounds();

                        combinedFilter.sharedMesh.vertices = combinedMesh.vertices;
                        combinedFilter.sharedMesh.triangles = combinedMesh.triangles;
                        combinedFilter.sharedMesh.normals = combinedMesh.normals;
                        combinedFilter.sharedMesh.tangents = combinedMesh.tangents;
                        combinedFilter.sharedMesh.uv = combinedMesh.uv;
                        combinedFilter.sharedMesh.uv2 = combinedMesh.uv2;
                        combinedFilter.sharedMesh.uv3 = combinedMesh.uv3;
                        combinedFilter.sharedMesh.uv4 = combinedMesh.uv4;
                        combinedFilter.sharedMesh.colors = combinedMesh.colors;
                        combinedFilter.transform.localPosition = Vector3.zero;
                        combinedFilter.transform.localRotation = Quaternion.identity;
                        combinedFilter.transform.localScale = Vector3.one;

                        for (int i = 1; i < kvp.Value.Count; i++)
                        {
                            Object.DestroyImmediate(kvp.Value[i].MeshFilter.gameObject);
                        }
                    }
                }
            }
        }
    }
}