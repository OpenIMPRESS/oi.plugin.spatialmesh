
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if !UNITY_EDITOR && UNITY_METRO
using HoloToolkit.Unity.SpatialMapping;
#endif
using oi.core.network;

namespace oi.plugin.spatialmesh {

    public class MeshSender : MonoBehaviour {

        public float intervalSec = 2;
        public UDPConnector connector;

        private float timer = 0;
#if !UNITY_EDITOR && UNITY_METRO
        List<int> remove_ids;
        List<ObjectSurfaceObserver.SurfaceObject> send_new;
#endif

        // Use this for initialization
        void Start() {
#if !UNITY_EDITOR && UNITY_METRO
            remove_ids = new List<int>();
            send_new = new List<ObjectSurfaceObserver.SurfaceObject>();
            SpatialMappingManager.Instance.SurfaceObserver.SurfaceAdded += SurfaceObserver_SurfaceAdded;
            SpatialMappingManager.Instance.SurfaceObserver.SurfaceRemoved += SurfaceObserver_SurfaceRemoved;
            SpatialMappingManager.Instance.SurfaceObserver.SurfaceUpdated += SurfaceObserver_SurfaceUpdated;
#endif
        }

#if !UNITY_EDITOR && UNITY_METRO
        private void SurfaceObserver_SurfaceUpdated(object sender, DataEventArgs<SpatialMappingSource.SurfaceUpdate> e) {
            send_new.Add(e.Data.New);
        }

        private void SurfaceObserver_SurfaceRemoved(object sender, DataEventArgs<SpatialMappingSource.SurfaceObject> e) {
            remove_ids.Add(e.Data.ID);
        }

        private void SurfaceObserver_SurfaceAdded(object sender, DataEventArgs<SpatialMappingSource.SurfaceObject> e) {
            send_new.Add(e.Data);
        }
#endif

        // Update is called once per frame
        void Update() {
            timer += Time.deltaTime;
            OIMSG msg = connector.GetNewData();
            if (msg != null && msg.data != null) {
                Debug.Log("Got msg on spatialmesh connector: "+msg.data.Length);
            }
            if (timer >= intervalSec) {
                timer = 0;
                SendMeshes();
            }
        }

        private void SendMeshes() {
#if !UNITY_EDITOR && UNITY_METRO
            List<ObjectSurfaceObserver.SurfaceObject> surfaces = new List<SpatialMappingSource.SurfaceObject>();
            surfaces.AddRange(send_new);
            send_new.Clear();

            for (int index = 0; index < surfaces.Count; index++) {
                MeshFilter filter = surfaces[index].Filter;
                Mesh source = filter.sharedMesh;
                Mesh clone = new Mesh();
                List<Vector3> verts = new List<Vector3>();
                verts.AddRange(source.vertices);

                for (int vertIndex = 0; vertIndex < verts.Count; vertIndex++) {
                    verts[vertIndex] = filter.transform.TransformPoint(verts[vertIndex]);
                }

                clone.SetVertices(verts);
                clone.SetTriangles(source.triangles, 0);

                OIMSG msg = new OIMSG((byte) OI_MSGFAMILY.XR, (byte) OI_MSGTYPE_XR.SPATIAL_MESH_ADD, OIMeshSerializer.OISerialize(surfaces[index].ID, clone));

                connector.SendData(msg);
            }

            if (remove_ids.Count > 0) {
                OIMSG msg = new OIMSG((byte)OI_MSGFAMILY.XR, (byte)OI_MSGTYPE_XR.SPATIAL_MESH_REMOVE, IdsSerializer.Serialize(remove_ids));

                remove_ids.Clear();
                connector.SendData(msg);
            }
#endif
        }
    }
}