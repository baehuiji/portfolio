using UnityEngine;
using UnityEngine.XR;
using UnityEngine.Rendering;
using System.Collections.Generic;

public enum Mode
{
    None,
    Solid,
    Culled,
    Inverted
}

public class BufferObject {
    public CommandBuffer buffer;
    public RenderTargetIdentifier id;
    public RenderTexture tex;
    public bool dirtyBuffer = true;
    public bool dirtyTexture = true;
    public int systemCount = 0;
}

[RequireComponent(typeof(Camera))]
public class EasyOutlineSystem : MonoBehaviour
{
    [Tooltip("Outline modifier")]
    public Mode outlineMode = Mode.None;
    [Tooltip("Fill modifier")]
    public Mode fillMode = Mode.None;
    [Tooltip("Outline Color")]
    public Color outlineColor = new Color(1.0f, 1.0f, 1.0f, 1.0f);
    [Tooltip("Fill Color")]
    public Color fillColor = new Color(1.0f, 1.0f, 1.0f, 0.5f);
    [Tooltip("Thickness of the outline")]
    [Range(0, 15)]
    public float outlineThickness = 2f;
    [Tooltip("How many samples should be used to create the outline")]
    [Range(4, 256)]
    public int sampleCount = 32;
    [Tooltip("List of all renderers to apply outline to")]
    public List<Renderer> rendererList;

    private Material easyOutlineMaterial;
    private Material depthMaterial;
    private Camera cam;
	private Texture2D texture;
    private RenderTexture isolatedTexture;
    private RenderTargetIdentifier isolatedID;
    private CommandBuffer isolatedCmdBuf;
    private int[] resolution = { 0, 0 };
	protected const CameraEvent cmdBufEvent = CameraEvent.AfterImageEffectsOpaque;
    private RenderTextureFormat renTexFmt = RenderTextureFormat.RFloat;
    private Color clearColor = Color.white;
    private static Dictionary<Camera, BufferObject> cameraSceneMap;
    private bool openGLFix = false;

    public int index;

    public string objName;

    void Awake() {
        cam = GetComponent<Camera>();
        if (cameraSceneMap == null) {
            cameraSceneMap = new Dictionary<Camera, BufferObject>();
        }

        if (!cameraSceneMap.ContainsKey(cam)) {
            BufferObject entry = new BufferObject();
            CommandBuffer buffer = new CommandBuffer();
            buffer.name = "SceneBuffer";
            entry.buffer = buffer;
            cameraSceneMap.Add(cam, entry);
        }

		isolatedCmdBuf = new CommandBuffer();
		isolatedCmdBuf.name = "IsolatedSceneBuffer";
        easyOutlineMaterial = new Material(Shader.Find("EasyOutline/System"));
        openGLFix = (SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLES2 || SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLES3 || SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLCore);
        depthMaterial = new Material(Shader.Find("EasyOutline/RenderDepth"));
        clearColor = openGLFix ? Color.white : Color.black;
        renTexFmt = SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.RFloat) ? RenderTextureFormat.RFloat : RenderTextureFormat.Default;
    }

    void OnEnable() {
        if (cam.commandBufferCount == 0) {
            cam.AddCommandBuffer(cmdBufEvent, cameraSceneMap[cam].buffer);
        }
        cam.AddCommandBuffer(cmdBufEvent, isolatedCmdBuf);
        cameraSceneMap[cam].systemCount++;
    }

    void OnDisable() {
        cam.RemoveCommandBuffer(cmdBufEvent, isolatedCmdBuf);
        cameraSceneMap[cam].systemCount--;
        if (cameraSceneMap[cam].systemCount <= 0) {
            cam.RemoveCommandBuffer(cmdBufEvent, cameraSceneMap[cam].buffer);
        }
    }

    private List<Renderer> getRenderers() {
        List<Renderer> result = new List<Renderer>();
        foreach (var item in rendererList) {
            result.Add(item);
        }
        return result;
    }
    private List<Renderer> getAllRenderers()  {
        List<Renderer> result = new List<Renderer>();
        result.AddRange(GameObject.FindObjectsOfType<Renderer>());
        return result;
    }

    private void updateRenderTextures() {
        resolution[0] = Screen.width;
        resolution[1] = Screen.height;

        if (XRSettings.enabled) {
            RenderTextureDescriptor desc = UnityEngine.XR.XRSettings.eyeTextureDesc;
            desc.colorFormat = renTexFmt;
            isolatedTexture = new RenderTexture(desc);
            if (cameraSceneMap[cam].dirtyTexture) {
                cameraSceneMap[cam].tex = new RenderTexture(desc);
            }
        }
        else {
            isolatedTexture = new RenderTexture(resolution[0], resolution[1], 0, renTexFmt);
            if (cameraSceneMap[cam].dirtyTexture) {
                cameraSceneMap[cam].tex = new RenderTexture(resolution[0], resolution[1], 0, renTexFmt);
            }
        }
        isolatedID = new RenderTargetIdentifier(isolatedTexture);
        if (cameraSceneMap[cam].dirtyTexture) {
            cameraSceneMap[cam].id = new RenderTargetIdentifier(cameraSceneMap[cam].tex);
        }
        cameraSceneMap[cam].dirtyTexture = false;
    }

    private void updateCommandBuffers() {
        isolatedCmdBuf.Clear();
        isolatedCmdBuf.SetRenderTarget(isolatedID);
        isolatedCmdBuf.ClearRenderTarget(true, true, clearColor);
        isolatedCmdBuf.SetProjectionMatrix(cam.projectionMatrix);
        isolatedCmdBuf.Blit(texture, isolatedTexture);
        foreach (var item in getRenderers()) {
            if (item.enabled && item.gameObject.activeInHierarchy) {
                isolatedCmdBuf.DrawRenderer(item, depthMaterial, 0, openGLFix ? 1 : 0);
            }
        }

        if (cameraSceneMap[cam].dirtyBuffer) {
            cameraSceneMap[cam].buffer.Clear();
            cameraSceneMap[cam].buffer.SetRenderTarget(cameraSceneMap[cam].id);
            cameraSceneMap[cam].buffer.ClearRenderTarget(true, true, clearColor);
            cameraSceneMap[cam].buffer.SetProjectionMatrix(cam.projectionMatrix);
            cameraSceneMap[cam].buffer.Blit(texture, cameraSceneMap[cam].tex);
            foreach (var item in getAllRenderers()) {
                if (item.enabled && item.gameObject.activeInHierarchy) {
                    cameraSceneMap[cam].buffer.DrawRenderer(item, depthMaterial, 0, openGLFix ? 1 : 0);
                }
            }
            cameraSceneMap[cam].dirtyBuffer = false;
        }
    }

    public void OnPreRender() {
        if (Screen.width != resolution[0] || Screen.height != resolution[1]) {
            updateRenderTextures();
        }
        updateCommandBuffers();
    }

    void OnRenderImage(RenderTexture before, RenderTexture after) {
        easyOutlineMaterial.SetTexture("_IsolatedSceneTex", isolatedTexture);
        easyOutlineMaterial.SetTexture("_SceneTex", cameraSceneMap[cam].tex);
        easyOutlineMaterial.SetColor("_OutlineColor", outlineColor);
        easyOutlineMaterial.SetColor("_FillColor", fillColor);
        easyOutlineMaterial.SetFloat("_OutlineThickness", outlineThickness);
        easyOutlineMaterial.SetFloat("_SampleCount", sampleCount);
        easyOutlineMaterial.SetInt("_OutlineMode", (int)outlineMode);
        easyOutlineMaterial.SetInt("_FillMode", (int)fillMode);
        easyOutlineMaterial.SetInt("_FlipUV", (XRSettings.enabled && !openGLFix) ? 1 : 0);
        easyOutlineMaterial.SetInt("_OpenGLFix", openGLFix ? 1 : 0);
        Graphics.Blit(before, after, easyOutlineMaterial);

        cameraSceneMap[cam].dirtyBuffer = true;
        cameraSceneMap[cam].dirtyTexture = true;
    }
}
