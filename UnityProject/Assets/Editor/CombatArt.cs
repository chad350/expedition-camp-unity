using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections.Generic;
namespace ExpeditionCamp.EditorTools
{
    public static class CombatArt
    {
        static Material Mat(string name, Color c)
        {
            string path="Assets/Materials/Model_"+name+".mat";
            var m=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(m==null){m=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(m,path);}
            m.color=c;m.SetFloat("_Smoothness",.12f);return m;
        }
        static Mesh Gem()
        {
            const string path="Assets/Art/ModelFacet.asset";
            var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(path);if(mesh!=null)return mesh;
            var verts=new List<Vector3>();var tris=new List<int>();
            Vector3[] ring={Vector3.forward,Vector3.right,Vector3.back,Vector3.left};
            for(int i=0;i<4;i++)foreach(float sign in new[]{1f,-1f})
            {
                Vector3 a=Vector3.up*sign,b=ring[i],c=ring[(i+1)%4];
                if(sign<0){var t=b;b=c;c=t;}
                Vector3 ab=(a+b).normalized,bc=(b+c).normalized,ca=(c+a).normalized;
                Vector3[] vs={a,ab,ca, ab,b,bc, ca,bc,c, ab,bc,ca};
                foreach(var v in vs){tris.Add(verts.Count);verts.Add(v*.5f);}
            }
            mesh=new Mesh{name="Faceted rounded solid"};mesh.SetVertices(verts);mesh.SetTriangles(tris,0);mesh.RecalculateNormals();AssetDatabase.CreateAsset(mesh,path);return mesh;
        }
        static Transform Part(Transform root,string name,Vector3 pos,Vector3 scale,Material mat,Vector3 rot=default)
        {
            var t=new GameObject(name).transform;t.SetParent(root,false);t.localPosition=pos;t.localScale=scale;t.localEulerAngles=rot;
            t.gameObject.AddComponent<MeshFilter>().sharedMesh=Gem();t.gameObject.AddComponent<MeshRenderer>().sharedMaterial=mat;return t;
        }
        public static HitFeedback Build(GameObject root,string key)
        {
            var v=new GameObject("Visual").transform;v.SetParent(root.transform,false);
            var motion=v.gameObject.AddComponent<ModelMotion>();
            var cream=Mat("Ivory",new Color(.94f,.85f,.65f));var dark=Mat("Ink",new Color(.055f,.095f,.10f));
            var orange=Mat("Fox",new Color(.86f,.38f,.12f));var teal=Mat("Cloth",new Color(.08f,.37f,.34f));
            bool crab=key.Contains("100002"),rabbit=key.Contains("100003"),bat=key.Contains("100010"),golem=key.Contains("100014"),wolf=key.Contains("100016");
            Material body=crab?Mat("Coral",new Color(.69f,.22f,.14f)):rabbit?cream:bat?Mat("Bat",new Color(.30f,.22f,.42f)):golem?Mat("Stone",new Color(.39f,.47f,.43f)):wolf?Mat("Wolf",new Color(.32f,.43f,.49f)):orange;
            if(crab)
            {
                Part(v,"Shell",new Vector3(0,.65f,0),new Vector3(1.5f,.7f,1.15f),body);
                for(int side=-1;side<=1;side+=2){for(int i=0;i<3;i++)Part(v,"Leg",new Vector3(side*.72f,.27f,-.4f+i*.38f),new Vector3(.85f,.17f,.18f),body,new Vector3(0,side*(i-1)*22,side*23));Part(v,"Claw",new Vector3(side*.85f,.8f,.65f),new Vector3(.6f,.5f,.65f),body);Part(v,"Eye",new Vector3(side*.32f,1.08f,.4f),new Vector3(.16f,.25f,.16f),dark);}
            }
            else if(golem)
            {
                Part(v,"Torso",new Vector3(0,1.1f,0),new Vector3(1.4f,1.4f,.9f),body);Part(v,"Head",new Vector3(0,2,0),new Vector3(.85f,.75f,.75f),body);
                for(int side=-1;side<=1;side+=2){Part(v,"Arm",new Vector3(side*.9f,1.05f,.1f),new Vector3(.65f,1.25f,.7f),body);Part(v,"Foot",new Vector3(side*.4f,.28f,.1f),new Vector3(.65f,.55f,.8f),body);Part(v,"Eye",new Vector3(side*.19f,2.07f,.34f),new Vector3(.15f,.12f,.12f),teal);}Part(v,"Heart",new Vector3(0,1.25f,.47f),new Vector3(.35f,.5f,.15f),teal);
            }
            else if(bat)
            {
                motion.flying=true;Part(v,"Body",new Vector3(0,1.2f,0),new Vector3(.6f,.8f,.55f),body);
                for(int side=-1;side<=1;side+=2){Part(v,"Wing",new Vector3(side*.72f,1.25f,0),new Vector3(1.25f,.12f,.8f),body,new Vector3(0,0,side*18));Part(v,"Ear",new Vector3(side*.2f,1.72f,0),new Vector3(.22f,.5f,.2f),body);Part(v,"Eye",new Vector3(side*.15f,1.4f,.26f),new Vector3(.12f,.13f,.10f),cream);}
            }
            else
            {
                Part(v,"Body",new Vector3(0,.72f,0),new Vector3(.82f,.95f,1.05f),body);
                Part(v,"Head",new Vector3(0,1.35f,.35f),new Vector3(.95f,.85f,.85f),body);
                Part(v,"Muzzle",new Vector3(0,1.15f,.74f),new Vector3(.62f,.42f,.45f),cream);
                Part(v,"Nose",new Vector3(0,1.27f,.94f),new Vector3(.19f,.14f,.14f),dark);
                for(int side=-1;side<=1;side+=2)
                {
                    Part(v,"Ear",new Vector3(side*.3f,rabbit?2.0f:1.83f,.25f),new Vector3(.32f,rabbit?1.05f:.62f,.28f),body,new Vector3(0,0,side* -14));
                    Part(v,"InnerEar",new Vector3(side*.3f,rabbit?2.0f:1.83f,.38f),new Vector3(.15f,rabbit?.72f:.34f,.06f),crab?body:cream,new Vector3(0,0,side* -14));
                    Part(v,"Eye",new Vector3(side*.28f,1.44f,.69f),new Vector3(.14f,.17f,.11f),dark);
                    foreach(float z in new[]{-.33f,.35f})Part(v,"Paw",new Vector3(side*.29f,.22f,z),new Vector3(.32f,.4f,.48f),wolf?dark:cream);
                }
                Part(v,"Tail",new Vector3(0,.85f,-.78f),new Vector3(.55f,.55f,rabbit?.45f:1.15f),body,new Vector3(-32,0,0));
                if(!rabbit&&!wolf){Part(v,"Scarf",new Vector3(0,1.01f,.18f),new Vector3(.94f,.24f,.8f),teal);Part(v,"Backpack",new Vector3(0,1.04f,-.42f),new Vector3(.65f,.65f,.4f),teal);}
            }
            foreach(var t in v.GetComponentsInChildren<Transform>())t.gameObject.layer=root.layer;
            Effects(v);
            return v.gameObject.AddComponent<HitFeedback>();
        }
        static void Effects(Transform parent)
        {
            const string path="Assets/Materials/CombatSpark.mat";
            var mat=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(mat==null){mat=new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));AssetDatabase.CreateAsset(mat,path);}
            mat.SetColor("_BaseColor",new Color(1.8f,1.3f,.5f,1));
            var fx=parent.gameObject.AddComponent<CombatEffects>();
            var go=new GameObject("Hit Sparks");go.transform.SetParent(parent,false);go.transform.localPosition=Vector3.up;
            var ps=go.AddComponent<ParticleSystem>();ps.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
            var main=ps.main;main.playOnAwake=false;main.loop=false;main.duration=.3f;main.startLifetime=new ParticleSystem.MinMaxCurve(.12f,.25f);main.startSpeed=new ParticleSystem.MinMaxCurve(2,4);main.startSize=new ParticleSystem.MinMaxCurve(.035f,.085f);main.gravityModifier=.5f;main.simulationSpace=ParticleSystemSimulationSpace.World;main.maxParticles=32;
            var emission=ps.emission;emission.enabled=false;var shape=ps.shape;shape.shapeType=ParticleSystemShapeType.Sphere;shape.radius=.15f;
            ps.GetComponent<ParticleSystemRenderer>().sharedMaterial=mat;
            var arc=new GameObject("Attack Arc");arc.transform.SetParent(parent,false);var line=arc.AddComponent<LineRenderer>();line.useWorldSpace=false;line.positionCount=12;line.widthMultiplier=.1f;line.sharedMaterial=mat;line.startColor=line.endColor=new Color(1,.86f,.46f);line.enabled=false;
            EditorUtil.SetField(fx,"sparks",ps);EditorUtil.SetField(fx,"slash",line);
        }
        public static void Drop(Transform parent)
        {
            Part(parent,"LootCrystal",Vector3.zero,new Vector3(.38f,.6f,.38f),Mat("LootGold",new Color(.95f,.66f,.18f)));
        }
        public static void Lighting(bool cave)
        {
            var cam=Camera.main;var data=cam.GetUniversalAdditionalCameraData();data.renderPostProcessing=true;data.antialiasing=AntialiasingMode.SubpixelMorphologicalAntiAliasing;cam.allowHDR=true;
            var volume=new GameObject("Global Volume — Art Direction").AddComponent<Volume>();volume.isGlobal=true;volume.priority=10;
            string path="Assets/Settings/URP/Combat"+(cave?"Cave":"Forest")+"Volume.asset";
            var profile=AssetDatabase.LoadAssetAtPath<VolumeProfile>(path);
            if(profile==null){profile=ScriptableObject.CreateInstance<VolumeProfile>();AssetDatabase.CreateAsset(profile,path);}
            foreach(var old in profile.components)Object.DestroyImmediate(old,true);profile.components.Clear();
            var tone=profile.Add<Tonemapping>(true);tone.mode.Override(TonemappingMode.ACES);
            var color=profile.Add<ColorAdjustments>(true);color.postExposure.Override(.25f);color.contrast.Override(12);color.saturation.Override(-8);
            var bloom=profile.Add<Bloom>(true);bloom.threshold.Override(1.15f);bloom.intensity.Override(.18f);bloom.scatter.Override(.55f);
            var vignette=profile.Add<Vignette>(true);vignette.intensity.Override(.18f);vignette.smoothness.Override(.45f);
            foreach(var c in profile.components)AssetDatabase.AddObjectToAsset(c,profile);EditorUtility.SetDirty(profile);volume.sharedProfile=profile;
            RenderSettings.ambientMode=AmbientMode.Trilight;RenderSettings.ambientSkyColor=new Color(.64f,.73f,.79f);RenderSettings.ambientEquatorColor=new Color(.36f,.43f,.41f);RenderSettings.ambientGroundColor=new Color(.22f,.25f,.19f);
            foreach(var light in Object.FindObjectsByType<Light>(FindObjectsSortMode.None))if(light.type==LightType.Directional){light.intensity=1.65f;light.color=new Color(1,.88f,.69f);light.transform.rotation=Quaternion.Euler(48,-35,0);light.shadows=LightShadows.Soft;light.shadowBias=.035f;light.shadowNormalBias=.2f;}
            var pipe=GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;if(pipe!=null){pipe.msaaSampleCount=4;pipe.shadowDistance=55;pipe.shadowCascadeCount=2;var so=new SerializedObject(pipe);so.FindProperty("m_SoftShadowsSupported").boolValue=true;so.ApplyModifiedPropertiesWithoutUndo();EditorUtility.SetDirty(pipe);}
        }
    }
}
