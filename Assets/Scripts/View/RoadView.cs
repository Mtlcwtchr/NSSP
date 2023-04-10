using Model;
using UnityEngine;

namespace View
{
    public class RoadView : MonoBehaviour
    {
        [SerializeField] private MeshFilter meshFilter;
        [SerializeField] private MeshRenderer meshRenderer;
        [SerializeField] private Material material;

        private Material _mat;
        
        private Road _model;
        public Road Model
        {
            get => _model;
            set
            {
                if (value != _model)
                {
                    _model = value;
                    _model.OnCapacityChanged += CapacityChanged;
                }
            }
        }

        private void CapacityChanged(float value)
        {
            if (_mat == null)
            {
                return;
            }

            _mat.SetFloat("_Thickness", value);
        }

        public void Draw()
        {
            Vector3 f = Model.From.WorldPosition;
            Vector3 t = Model.To.WorldPosition;
            Vector3 dir = t - f;
            Vector3 offsetRight = dir.magnitude/2.0f * Vector3.right;
            Vector3 p1 = -offsetRight;
            Vector3 p2 = offsetRight;
            Vector3[] verts = GetVerticesOfRoadPlane(p1, p2);
            Vector2[] uvs = new Vector2[verts.Length];
            for (var i = 0; i < verts.Length; i++)
            {
                uvs[i] = verts[i];
            }
            int[] triangles = GetTrianglesOfRoadPlane();
            
            Vector3 baseDirection = Vector3.right;
            dir.Normalize();
            var rot = Quaternion.FromToRotation(baseDirection, dir);
            transform.rotation = rot;

            Mesh mesh = new Mesh();
            mesh.vertices = verts;
            mesh.uv = uvs;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            meshFilter.mesh = mesh;

            _mat = Instantiate(material);
            meshRenderer.sharedMaterial = _mat;
            _mat.SetColor("_Color", 
                Model.AvailableForSupply ? 
                    Model.From.WarSide == WarSide.Player ? 
                        Color.green : 
                        Color.red : 
                    Color.gray);
            CapacityChanged(Model.CapacityFactor);
        }
        
        private Vector3[] GetVerticesOfRoadPlane(Vector3 p1, Vector3 p2)
        {
            float lineWidth = 1f;
            Vector3[] verts = new Vector3[4];
            Vector3 offset = new Vector3(0, lineWidth/2, 0);

            verts[0] = p1 + offset;
            verts[1] = p1 - offset;
            verts[2] = p2 + offset;
            verts[3] = p2 - offset;
            
            return verts;
        }

        private int[] GetTrianglesOfRoadPlane()
        {
            int[] triangles = new int[6];
            triangles[0] = 0;
            triangles[1] = 2;
            triangles[2] = 1;
            triangles[3] = 1;
            triangles[4] = 2;
            triangles[5] = 3;
            return triangles;
        }
    }
}