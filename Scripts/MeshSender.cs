#if !UNITY_EDITOR && UNITY_METRO
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Unity.SpatialMapping;
using oi.core.network;

namespace oi.plugin.spatialmesh {

    public class MeshSender : MonoBehaviour
    {

        public float intervalSec = 2;
        public UDPConnector connector;

        private float timer = 0;
        List<int> remove_ids;
        List<ObjectSurfaceObserver.SurfaceObject> send_new;

        // Use this for initialization
        void Start() {
            remove_ids = new List<int>();
            send_new = new List<ObjectSurfaceObserver.SurfaceObject>();
            SpatialMappingManager.Instance.SurfaceObserver.SurfaceAdded += SurfaceObserver_SurfaceAdded;
            SpatialMappingManager.Instance.SurfaceObserver.SurfaceRemoved += SurfaceObserver_SurfaceRemoved;
            SpatialMappingManager.Instance.SurfaceObserver.SurfaceUpdated += SurfaceObserver_SurfaceUpdated;
        }

        private void SurfaceObserver_SurfaceUpdated(object sender, DataEventArgs<SpatialMappingSource.SurfaceUpdate> e) {
            Debug.Log("MS UPDATE " + e.Data.New.ID);
            send_new.Add(e.Data.New);
        }

        private void SurfaceObserver_SurfaceRemoved(object sender, DataEventArgs<SpatialMappingSource.SurfaceObject> e) {
            Debug.Log("MS REMOVE " + e.Data.ID);
            remove_ids.Add(e.Data.ID);
        }

        private void SurfaceObserver_SurfaceAdded(object sender, DataEventArgs<SpatialMappingSource.SurfaceObject> e) {
            Debug.Log("MS NEW " + e.Data.ID);
            send_new.Add(e.Data);
        }

        // Update is called once per frame
        void Update() {
            timer += Time.deltaTime;
            if (timer >= intervalSec) {
                timer = 0;
                SendMeshes();
            }
        }

        private void SendMeshes() {
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

                OIMSG msg = new OIMSG((byte) OI_MSGFAMILY.XR, (byte) OI_MSGTYPE_XR.SPATIAL_MESH, OIMeshSerializer.OISerialize(surfaces[index].ID, clone));
                connector.SendData(msg);
            }

            if (remove_ids.Count > 0) {
                byte[] idsSerialized = IdsSerializer.Serialize(remove_ids);
                remove_ids.Clear();
                connector.SendData(idsSerialized);
            }
        }

    }
}
#endif