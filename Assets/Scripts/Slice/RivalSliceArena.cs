using System.Collections.Generic;
using UnityEngine;

namespace KindredSiege.Slice
{
    public sealed partial class RivalSliceView
    {
        Material _arenaMaterial;
        Mesh _orderRingMesh;
        readonly List<Mesh> _arenaMeshes = new List<Mesh>();

        // Authored silhouettes and a quiet fighting surface. No random props in the formations.
        void BuildArena()
        {
            _stageRoot = new GameObject("Fallen Star / arena").transform;
            var cameraObject = new GameObject("Slice camera");
            cameraObject.tag = "MainCamera";
            cameraObject.transform.SetParent(_stageRoot);
            _camera = cameraObject.AddComponent<Camera>();
            _camera.clearFlags = CameraClearFlags.SolidColor;
            _camera.backgroundColor = new Color(.24f,.30f,.30f);
            _camera.orthographic = true;
            _camera.orthographicSize = 5.2f;
            _camera.transform.position = CameraHome;
            _camera.transform.LookAt(CameraLook);
            RenderSettings.fog = false;
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(.8f,.79f,.74f);
            var sun = new GameObject("Late afternoon").AddComponent<Light>();
            sun.transform.SetParent(_stageRoot);
            sun.type = LightType.Directional;
            sun.intensity = .65f;
            sun.color = new Color(1f,.91f,.77f);
            sun.transform.rotation = Quaternion.Euler(48,-35,0);
            _arenaMaterial = new Material(Resources.Load<Shader>("RivalSlice/ArenaFlat"));

            Slab("Desert floor",new Vector3(0,-.18f,1),new Vector3(32,.16f,26),new Color(.57f,.49f,.37f));
            // The horizontal strata continue beyond the camera; this is a place, not a floating board.
            Ridge(-14,7.2f,7,2.4f,2.5f,new Color(.32f,.37f,.35f));
            Ridge(-6.7f,8.1f,6,3.2f,2.4f,new Color(.36f,.40f,.36f));
            Ridge(.3f,8.2f,8,2.1f,2.6f,new Color(.34f,.38f,.35f));
            Ridge(8.2f,7,6,3.1f,2,new Color(.31f,.36f,.34f));
            Ridge(-11,5.4f,5,1.1f,1.8f,new Color(.50f,.43f,.34f));
            Ridge(6.3f,5.9f,6,1.4f,1.6f,new Color(.49f,.40f,.31f));

            // Broad, stepped sediment patches keep contrast below that of the sprites.
            Patch("Worn approach",new Vector3(-5,-.005f,1),new float[]{-4,-3, 2,-3, 4,-1, 3,2, -2,3, -4,1},new Color(.60f,.52f,.40f));
            Patch("East dust",new Vector3(4,-.004f,1),new float[]{-3,-3, 4,-2, 4,2, 1,3, -3,2},new Color(.60f,.51f,.39f));
            Patch("Dry stream",new Vector3(0,.001f,0),new float[]{-.4f,-6, .5f,-6, .1f,-1, .6f,2, 0,6, -.5f,6, -.1f,2, -.7f,-1},new Color(.52f,.45f,.35f));
            // Buried ship ribs tie the arena to Fallen Star without competing with the combat.
            for(int i=0;i<5;i++)
            {
                float x=-5.7f+i*.62f;
                var rib=Slab("Exposed wreck rib",new Vector3(x,.32f,5.1f),new Vector3(.20f,.9f,1.8f),new Color(.24f,.29f,.28f));
                rib.transform.rotation=Quaternion.Euler(0,0,-18);
                Slab("Rib edge",new Vector3(x+.11f,.7f,4.4f),new Vector3(.08f,.12f,.25f),new Color(.66f,.61f,.44f));
            }
            var wreck=Slab("Buried hull",new Vector3(-4.5f,.1f,5.6f),new Vector3(3.6f,.26f,1.2f),new Color(.32f,.36f,.32f));
            wreck.transform.rotation=Quaternion.Euler(0,-12,0);
            BannerPost(-7.4f,2.5f,new Color(.29f,.57f,.52f));
            BannerPost(7.4f,2.5f,new Color(.65f,.27f,.20f));
            Ridge(-10,-3.9f,3.5f,.8f,1.2f,new Color(.34f,.30f,.25f));
            Ridge(6.7f,-4,3.8f,1,1.5f,new Color(.36f,.30f,.24f));
            float[,] stones={{-7,3.7f},{-6.8f,-1.3f},{-3.1f,4.7f},{2.3f,4.9f},{6.9f,-1.4f},{7,4.1f},{-5.8f,-3},{4.8f,-3.2f}};
            for(int i=0;i<stones.GetLength(0);i++)
                Slab("Shale",new Vector3(stones[i,0],.015f,stones[i,1]),new Vector3(.25f+(i%3)*.16f,.05f,.16f),new Color(.43f,.39f,.31f));
            for(int s=0;s<2;s++)
            {
                _traps[s]=OrderRing(_stageRoot,"Trap",new Vector3(s==0?-.6f:.6f,.025f,1.2f),.38f,new Color(.55f,.3f,.75f));
                _traps[s].SetActive(false);
                _focusRings[s]=OrderRing(_stageRoot,"Focus",Vector3.zero,.68f,s==0?Amber:Rival);
                _focusRings[s].SetActive(false);
            }
            _unitsRoot=new GameObject("Units").transform;
            _unitsRoot.SetParent(_stageRoot);
        }

        void BannerPost(float x,float z,Color cloth)
        {
            Slab("Signal post",new Vector3(x,1.05f,z),new Vector3(.1f,2.1f,.1f),new Color(.22f,.27f,.26f));
            Slab("Banner",new Vector3(x+.28f,1.62f,z),new Vector3(.48f,.65f,.08f),cloth);
            Slab("Banner stripe",new Vector3(x+.28f,1.65f,z-.05f),new Vector3(.3f,.07f,.02f),new Color(.89f,.8f,.59f));
        }

        GameObject OrderRing(Transform parent,string name,Vector3 position,float radius,Color color)
        {
            if(_orderRingMesh==null)
            {
                var vertices=new List<Vector3>();var triangles=new List<int>();
                for(int i=0;i<16;i++)
                {
                    float angle=i*Mathf.PI*2/16;var p=new Vector3(Mathf.Cos(angle),0,Mathf.Sin(angle));
                    vertices.Add(p);vertices.Add(p*.86f);
                    int a=i*2,b=((i+1)%16)*2;triangles.AddRange(new[]{a,a+1,b,b,a+1,b+1});
                }
                _orderRingMesh=new Mesh{name="Order ring"};_orderRingMesh.SetVertices(vertices);_orderRingMesh.SetTriangles(triangles,0);_orderRingMesh.RecalculateNormals();_orderRingMesh.RecalculateBounds();_arenaMeshes.Add(_orderRingMesh);
            }
            var go=new GameObject(name);go.transform.SetParent(parent,false);go.transform.localPosition=position;go.transform.localScale=Vector3.one*radius;
            go.AddComponent<MeshFilter>().sharedMesh=_orderRingMesh;go.AddComponent<MeshRenderer>();Surface(go,color);return go;
        }

        void Ridge(float x,float z,float width,float height,float depth,Color color)
        {
            Slab("Canyon lower shelf",new Vector3(x+width*.5f,height*.2f,z),new Vector3(width,height*.4f,depth),color);
            Slab("Canyon upper shelf",new Vector3(x+width*.48f,height*.65f,z+.25f),new Vector3(width*.81f,height*.5f,depth*.8f),color*.92f);
            Slab("Canyon cap",new Vector3(x+width*.47f,height*.94f,z+.2f),new Vector3(width*.86f,height*.08f,depth*.82f),color*1.15f);
        }

        GameObject Slab(string name,Vector3 position,Vector3 size,Color color)
        {
            float x=size.x*.5f,z=size.z*.5f,c=Mathf.Min(.3f,Mathf.Min(x,z)*.3f);
            var rim=new[]{new Vector2(-x+c,-z),new Vector2(x-c,-z),new Vector2(x,-z+c),new Vector2(x,z-c),new Vector2(x-c,z),new Vector2(-x+c,z),new Vector2(-x,z-c),new Vector2(-x,-z+c)};
            var vertices=new List<Vector3>();var colors=new List<Color>();var triangles=new List<int>();
            for(int i=0;i<rim.Length;i++)
            {
                Vector2 a=rim[i],b=rim[(i+1)%rim.Length];
                int n=vertices.Count;
                vertices.Add(new Vector3(0,size.y*.5f,0));vertices.Add(new Vector3(b.x,size.y*.5f,b.y));vertices.Add(new Vector3(a.x,size.y*.5f,a.y));
                for(int j=0;j<3;j++){colors.Add(color*1.09f);triangles.Add(n+j);}
                n=vertices.Count;
                vertices.Add(new Vector3(a.x,-size.y*.5f,a.y));vertices.Add(new Vector3(a.x,size.y*.5f,a.y));
                vertices.Add(new Vector3(b.x,size.y*.5f,b.y));vertices.Add(new Vector3(b.x,-size.y*.5f,b.y));
                Color shade=color*(i<3?.82f:.67f);for(int j=0;j<4;j++)colors.Add(shade);
                triangles.AddRange(new[]{n,n+1,n+2,n,n+2,n+3});
            }
            return ArenaMesh(name,position,vertices,colors,triangles);
        }

        void Patch(string name,Vector3 position,float[] coords,Color color)
        {
            var vertices=new List<Vector3>();var colors=new List<Color>();var triangles=new List<int>();
            for(int i=0;i<coords.Length;i+=2){vertices.Add(new Vector3(coords[i],0,coords[i+1]));colors.Add(color);}
            for(int i=1;i<vertices.Count-1;i++)triangles.AddRange(new[]{0,i+1,i});
            ArenaMesh(name,position,vertices,colors,triangles);
        }

        GameObject ArenaMesh(string name,Vector3 position,List<Vector3> vertices,List<Color> colors,List<int> triangles)
        {
            var mesh=new Mesh{name=name};mesh.SetVertices(vertices);mesh.SetColors(colors);mesh.SetTriangles(triangles,0);mesh.RecalculateBounds();
            _arenaMeshes.Add(mesh);
            var go=new GameObject(name);go.transform.SetParent(_stageRoot);go.transform.position=position;
            go.AddComponent<MeshFilter>().sharedMesh=mesh;go.AddComponent<MeshRenderer>().sharedMaterial=_arenaMaterial;return go;
        }

        void DisposePresentation()
        {
            if(_hudCanvas!=null)Destroy(_hudCanvas.gameObject);
            if(_ownedEventSystem!=null)Destroy(_ownedEventSystem);
            if(_arenaMaterial!=null)Destroy(_arenaMaterial);
            foreach(var mesh in _arenaMeshes)Destroy(mesh);
            if(_stageRoot!=null)Destroy(_stageRoot.gameObject);
        }
    }
}
