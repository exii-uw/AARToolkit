using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Collections;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;

namespace AAR
{
    public enum ProjectorCullingMasks
    {
        AAR_CULLING_VIRTUALTEXTURE,
        AAR_CULLING_VIRTUALOBJECT,
        AAR_CULLING_ENVIRONMENTMAP
    }

    public enum ProjectorRenderMode
    {
        Invalid = -1,
        Default_Projection,
        View_Dependent_Projection,
        Static_Material_Projection
    }

    [Serializable]
    public class StaticMaterial
    {
        public delegate void ShaderParameters();

        public Material material = null;
        public Shader shader = null;
        public Texture texture = null;
        public Color color = Color.black;
        public ShaderParameters shaderParametersDel = null;

        public StaticMaterial() 
        { }

        public void Set(Shader _shader)
        {
            texture = null;
            shader = _shader;
            material = new Material(shader);
        }
        public void Set(Texture _texture)
        {
            texture = _texture;
            shader = Shader.Find("StaticMaterial/FlipCamaraImage");
            material = new Material(shader);
        }
        public void Set(Texture _texture, Shader _shader)
        {
            texture = _texture;
            shader = _shader;
            material = new Material(shader);
        }

        public void Set(ShaderParameters _del)
        {
            shaderParametersDel = _del;
        }

        internal void UpdateInternal()
        {
            if (material == null)
            {
                shader = (shader == null) ? Shader.Find("StaticMaterial/FlipCamaraImage") : shader;
                material = new Material(shader);
            }
            shaderParametersDel?.Invoke();
        }
    }

    public class AARProjectorRenderingManager : 
        MonoBehaviour,
        AARProxyCameraInterface
    {
        ////////////////////////////////////////////////////////////////////////////////////////
        /// SINGLETON ACCESS 
        ////////////////////////////////////////////////////////////////////////////////////////
        private static AARProjectorRenderingManager _instance;
        public static AARProjectorRenderingManager Instance => _instance;

        void OnEnable()
        {
            // init instance singleton
            if (_instance != null && _instance != this)
            {
                Destroy(this.gameObject);
            }
            else
            {
                _instance = this;
            }
        }


        ////////////////////////////////////////////////////////////////////////////////////////
        /// PUBLIC VARS
        ////////////////////////////////////////////////////////////////////////////////////////

        // TODO: Create editor backend
        public Color DefualtBackground = new Color(0, 0, 0, 0);
        public LayerMask VirtualTextureMask;
        public LayerMask VirtualObjectMask;
        public LayerMask EnvironmentMappingMask;


        [Space(10)]
        public ProjectorRenderMode ProjectorRenderingMode = ProjectorRenderMode.Default_Projection;
        [Space(10)]
        public StaticMaterial ActiveStaticRenderMaterial = null;



        ////////////////////////////////////////////////////////////////////////////////////////
        /// PUBLIC API
        ////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Returns the current rendering mode. 
        /// </summary>        
        public ProjectorRenderMode GetCurrentProjectorRenderingMode()
        {
            return ProjectorRenderingMode;
        }

        /// <summary>
        /// Sets rendering mode to default
        /// </summary>
        public void ResetProjectorRenderingMode()
        {
            ProjectorRenderingMode = ProjectorRenderMode.Default_Projection;
        }

        /// <summary>
        /// Sets rendering mode to view dependent projection mapping. 
        /// - Note: This requires a AARUserView in the scene. 
        /// </summary>
        /// <param name="_enable">
        ///     Turn projection mapping on or off (default)
        /// </param>
        public void EnableViewDependentProjection(bool _enable = true)
        {
            if (!_enable)
            {
                ResetProjectorRenderingMode();
                return;
            }

            ProjectorRenderingMode = ProjectorRenderMode.View_Dependent_Projection;
        }

        /// <summary>
        /// Sets rendering mode to static material
        /// </summary>
        /// <param name="_enable">
        ///     Turns mode on or off. 
        /// </param>
        /// <param name="_staticMat">
        ///     The material to set. Can be a combination of shader / texture / material
        /// </param>
        public void EnableStaticMaterialRender(bool _enable, StaticMaterial _staticMat = null)
        {
            if (!_enable)
            {
                ResetProjectorRenderingMode();
                return;
            }

            ProjectorRenderingMode = ProjectorRenderMode.Static_Material_Projection;
            ActiveStaticRenderMaterial = _staticMat != null ? _staticMat : new StaticMaterial();
        }

        /// <summary>
        /// Sets the culling masks used for rendering. 
        /// </summary>
        /// <param name="_maskType"></param>
        /// <param name="_mask"></param>
        public void SetProjectorCullingMask(ProjectorCullingMasks _maskType, LayerMask _mask)
        {
            switch (_maskType)
            {
                case ProjectorCullingMasks.AAR_CULLING_VIRTUALTEXTURE:
                    VirtualTextureMask = _mask;
                    break;
                case ProjectorCullingMasks.AAR_CULLING_VIRTUALOBJECT:
                    VirtualObjectMask = _mask;
                    break;
                case ProjectorCullingMasks.AAR_CULLING_ENVIRONMENTMAP:
                    EnvironmentMappingMask = _mask;
                    break;
            }
        }


#region _INTERNAL


        ////////////////////////////////////////////////////////////////////////////////////////
        /// PRIVATE VARS 
        ////////////////////////////////////////////////////////////////////////////////////////

        private List<CSProjectorController> m_projectorControllers = new List<CSProjectorController>();
        private List<AARUserViewCamera> m_userViewCameras = new List<AARUserViewCamera>();
        private AARProxyCamera m_proxyCamera = null;

        private LayerMask m_ProjectorOnlyMask;


        ////////////////////////////////////////////////////////////////////////////////////////
        /// UNITY OVERRIDES
        ////////////////////////////////////////////////////////////////////////////////////////

        private void Awake()
        {
            m_ProjectorOnlyMask = LayerMask.GetMask("AARProjectorOnly");
        }

        private void Start()
        {
            // Create Proxy camera pipeline
            if (m_proxyCamera == null)
            {
                GameObject proxyCam = new GameObject();
                proxyCam.name = "AARProxyCam";
                proxyCam.transform.parent = transform;
                proxyCam.transform.localPosition = Vector3.zero;
                proxyCam.transform.localRotation = Quaternion.identity;

                m_proxyCamera = proxyCam.AddComponent<AARProxyCamera>();
                m_proxyCamera.AddListener(this);
            }

            // Collect all projectors in scene
            var projectors = GameObject.FindObjectsOfType<CSProjectorController>();
            foreach (var projector in projectors)
            {
                projector.OnPostRender_Callback += OnPostRender_ProjectorCallback;
                m_proxyCamera.AddListener(projector);

                m_projectorControllers.Add(projector);
            }


            // Collect all users in scene
            var userViews = GameObject.FindObjectsOfType<AARUserViewCamera>();
            foreach (var userView in userViews)
            {
                m_userViewCameras.Add(userView);
            }
        }

        private void Update()
        {
            
        }


        ///////////////////////////////////////////////////////////////////////////////////////////////
        /// CAMERA PROXY CALLBACKS
        ////////////////////////////////////////////////////////////////////////////////////////////////

        void AARProxyCameraInterface.OnPreCullProxy()
        {
        }

        void AARProxyCameraInterface.OnPreRenderProxy()
        {
            foreach (var userView in m_userViewCameras)
            {
                userView.RenderUserView();
            }
        }

        void AARProxyCameraInterface.OnPostRenderProxy()
        {
        }


        ///////////////////////////////////////////////////////////////////////////////////////////////
        /// OnPostRender Callback
        ////////////////////////////////////////////////////////////////////////////////////////////////
        private void OnPostRender_ProjectorCallback(int _id, CSProjectorController _projectorController)
        {
            Camera projectorCamera = _projectorController.GetCamera();

            // Render projector here
            if (ProjectorRenderingMode == ProjectorRenderMode.View_Dependent_Projection)
            {
                RenderViewPersepective(_id, projectorCamera);
                return;
            }

            if (ProjectorRenderingMode == ProjectorRenderMode.Static_Material_Projection)
            {
                // Blit Material to RenderTexture
                RenderTexture rt = _projectorController.GetActiveRT();

                // Update active static material
                ActiveStaticRenderMaterial.UpdateInternal();

                // Set Variables
                Shader.SetGlobalFloat("_ProjectorAspectRatio", _projectorController.GetAspectRatio());
                Shader.SetGlobalColor("_BackgroundColor", ActiveStaticRenderMaterial.color);
                Graphics.Blit(ActiveStaticRenderMaterial.texture, rt, ActiveStaticRenderMaterial.material);
                return;
            }

            
            // Default render path
            {
                projectorCamera.depth = 1;
                projectorCamera.backgroundColor = DefualtBackground;
                projectorCamera.clearFlags = CameraClearFlags.SolidColor;
                projectorCamera.cullingMask = VirtualTextureMask | m_ProjectorOnlyMask;
                projectorCamera.Render();
            }
        }

        private void RenderViewPersepective(int _id, Camera _projectorCamera)
        {
            int prevCulling = _projectorCamera.cullingMask;

            _projectorCamera.depth = 1;
            _projectorCamera.backgroundColor = DefualtBackground;
            _projectorCamera.clearFlags = CameraClearFlags.SolidColor;
            _projectorCamera.cullingMask = VirtualTextureMask;
            _projectorCamera.Render();

            _projectorCamera.clearFlags = CameraClearFlags.Nothing;
            for (int i = 0; i < m_userViewCameras.Count; i++)
            {
                AARUserViewCamera userView = m_userViewCameras[i];
                if (!userView.isActiveAndEnabled)
                    continue;
                userView.RenderProjection(_projectorCamera, EnvironmentMappingMask);
            }

            //Reset
            _projectorCamera.cullingMask = prevCulling;
            _projectorCamera.clearFlags = CameraClearFlags.SolidColor;
        }

#endregion // _INTERNAL

    }
}
