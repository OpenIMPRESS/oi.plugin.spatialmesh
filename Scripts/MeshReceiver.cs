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

        float lastSent = 0f;

        void Update() {
            OIMSG msg = oiudp.GetNewData();
            if (msg == null || msg.data == null || msg.data.Length == 0) return;
            HandleMeshMessage(msg);

            /*
            if (lastSent + 1.0f < Time.time) {
                oiudp.SendData(new byte[] { 0x00, 0x00, 0x00 });
                lastSent = Time.time;

            }*/
        }

        public void HandleMeshMessage(OIMSG msg) {
            if (msg.msgFamily != (byte)OI_MSGFAMILY.XR) return;
            if (msg.msgType == (byte)OI_MSGTYPE_XR.SPATIAL_MESH_ADD) {
                MeshStruct ms = OIMeshSerializer.OIDeserialize(msg.data);
                Transform existing = GetMeshObject(ms.ID);
                if (existing != null) { // update
                    GameObject.Destroy(existing.gameObject);
                }
                AddMeshObject(ms.mesh, ms.ID);
            } else if (msg.msgType == (byte) OI_MSGTYPE_XR.SPATIAL_MESH_REMOVE) {
                List<int> remove_ids = new List<int>(IdsSerializer.Deserialize(msg.data));
                foreach (int ID in remove_ids) {
                    Transform existing = GetMeshObject(ID);
                    if (existing != null) Destroy(existing.gameObject);
                }
            }
        }
        private Transform GetMeshObject(int meshID = 0) {
            string object_name = "SM_" + meshID.ToString();
            return transform.Find(object_name);
        }

        private Transform AddMeshObject(Mesh mesh, int meshID = 0) {
            string object_name = "SM_" + meshID.ToString();
            SurfaceObject surfaceObject = new SurfaceObject();
            surfaceObject.ID = meshID;
            surfaceObject.UpdateID = 0;
            surfaceObject.Object = new GameObject(object_name, componentsRequiredForSurfaceMesh);
            surfaceObject.Object.transform.position += transform.position;
            surfaceObject.Object.transform.SetParent(transform);

            surfaceObject.Filter = surfaceObject.Object.GetComponent<MeshFilter>();
            surfaceObject.Filter.sharedMesh = mesh;

            surfaceObject.Renderer = surfaceObject.Object.GetComponent<MeshRenderer>();
            surfaceObject.Renderer.sharedMaterial = surfaceMaterial;

            return surfaceObject.Object.transform;
        }
    }
}