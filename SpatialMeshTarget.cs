using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public class SpatialMeshTarget : MonoBehaviour {

    public Material surfaceMaterial;

    public struct SurfaceObject
    {
        public int ID;
        public int UpdateID;
        public GameObject Object;
        public MeshRenderer Renderer;
        public MeshFilter Filter;
    }

    /// <summary>
    /// When a mesh is created we will need to create a game object with a minimum 
    /// set of components to contain the mesh.  These are the required component types.
    /// </summary>
    protected readonly Type[] componentsRequiredForSurfaceMesh =
    {
            typeof(MeshFilter),
            typeof(MeshRenderer),
            typeof(MeshCollider)
        };

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public GameObject AddMeshObject(Mesh mesh, string objectName, int meshID = 0)
    {
        SurfaceObject surfaceObject = new SurfaceObject();
        surfaceObject.ID = meshID;
        surfaceObject.UpdateID = 0;

        surfaceObject.Object = new GameObject(objectName, componentsRequiredForSurfaceMesh);
        surfaceObject.Object.transform.position += transform.position;
        surfaceObject.Object.transform.SetParent(transform);

        surfaceObject.Filter = surfaceObject.Object.GetComponent<MeshFilter>();
        surfaceObject.Filter.sharedMesh = mesh;

        surfaceObject.Renderer = surfaceObject.Object.GetComponent<MeshRenderer>();
        surfaceObject.Renderer.sharedMaterial = surfaceMaterial;

        return surfaceObject.Object;
    }

    
}
