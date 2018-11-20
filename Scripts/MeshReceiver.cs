using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using oi.core.network;

namespace oi.plugin.spatialmesh {

    [RequireComponent(typeof(UDPConnector))]
    public class MeshReceiver : MonoBehaviour {

        public Material surfaceMaterial;

        public struct SurfaceObject {
            public int ID;
            public int UpdateID;
            public GameObject Object;
            public MeshRenderer Renderer;
            public MeshFilter Filter;
        }

        protected readonly System.Type[] componentsRequiredForSurfaceMesh = {
            typeof(MeshFilter),
            typeof(MeshRenderer),
            typeof(MeshCollider)
        };

        private UDPConnector oiudp;

        void Start() {
            oiudp = GetComponent<UDPConnector>();

        }
        void Update() {
            OIMSG msg = oiudp.GetNewData();
            if (msg == null || msg.data == null || msg.data.Length == 0) return;
            HandleMeshMessage(msg);
        }

        public void HandleMeshMessage(OIMSG msg) {
            if (msg.msgFamily != (byte)OI_MSGFAMILY.XR) return;
            if (msg.msgType == (byte)OI_MSGTYPE_XR.SPATIAL_MESH_ADD) { 
                MeshStruct ms = OIMeshSerializer.OIDeserialize(msg.data);
                string object_name = "SM_" + ms.ID.ToString();
                Transform existing = transform.Find(object_name);
                if (existing != null) { // update
                    GameObject.Destroy(existing);
                }
                AddMeshObject(ms.mesh, object_name, ms.ID);
            } else if (msg.msgType == (byte) OI_MSGTYPE_XR.SPATIAL_MESH_REMOVE) {

            }
        }

        public GameObject AddMeshObject(Mesh mesh, string objectName, int meshID = 0) {
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
}